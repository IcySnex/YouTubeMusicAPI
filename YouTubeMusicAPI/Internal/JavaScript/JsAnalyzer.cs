using Acornima;
using Acornima.Ast;
using System.Reflection;

namespace YouTubeMusicAPI.Internal.JavaScript;

internal class JsAnalyzer
{
    class Scope(
        HashSet<string> names,
        string type)
    {
        public HashSet<string> Names { get; set; } = names;

        public string Type { get; set; } = type;
    }


    public static readonly HashSet<string> BuiltIns =
    [
        "AbortController", "AbortSignal", "Array", "ArrayBuffer", "AsyncContext", "Atomics", "AudioContext", "BigInt", "BigInt64Array", "BigUint64Array",
        "Blob", "Boolean", "BroadcastChannel", "Buffer", "CanvasRenderingContext2D", "clearImmediate", "clearInterval", "clearTimeout", "confirm",
        "console", "Crypto", "CustomEvent", "DataView", "Date", "decodeURI", "decodeURIComponent", "document", "Element", "encodeURI",
        "encodeURIComponent", "Error", "escape", "eval", "Event", "EventTarget", "fetch", "File", "FileReader", "Float32Array", "Float64Array",
        "FormData", "function", "global", "globalThis", "hasOwnProperty", "Headers", "History", "HTMLElement", "HTMLCollection", "IDBKeyRange",
        "Infinity", "Int16Array", "Int32Array", "Int8Array", "Intl", "IntersectionObserver", "isFinite", "isNaN", "isPrototypeOf", "JSON",
        "location", "log", "Map", "Math", "MediaRecorder", "MediaSource", "MediaStream", "MemberExpression", "MutationObserver", "NaN",
        "navigator", "Node", "NodeList", "Number", "Object", "OfflineAudioContext", "parse", "parseFloat", "parseInt", "Performance",
        "process", "Promise", "prompt", "prototype", "Proxy", "ReadableStream", "Reflect", "RegExp", "requestAnimationFrame", "requestIdleCallback",
        "Request", "Response", "ResizeObserver", "Screen", "setImmediate", "setInterval", "setTimeout", "SharedArrayBuffer", "SharedWorker",
        "SourceBuffer", "split", "String", "stringify", "structuredClone", "SubtleCrypto", "Symbol", "TextDecoder", "TextEncoder", "this",
        "toString", "TransformStream", "Uint16Array", "Uint32Array", "Uint8Array", "Uint8ClampedArray", "undefined", "unescape", "URL",
        "URLSearchParams", "valueOf", "WeakMap", "WeakSet", "WebAssembly", "WebGLRenderingContext", "window", "Worker", "WritableStream",
        "XMLHttpRequest", "alert", "arguments", "atob", "btoa", "cancelAnimationFrame", "cancelIdleCallback", "queueMicrotask"
    ];


    readonly ExtractionState[] extractionStates;
    readonly Script ast;

    public JsAnalyzer(
        string source,
        (string FunctionName, JsMatchers.Delegate TryMatch, bool CollectDependencies)[] extractors)
    {
        Source = source;

        extractionStates = [.. extractors
            .Select(extractor => new ExtractionState(
                extractor,
                null,
                null,
                [],
                [],
                null,
                false))];

        ast = new Parser().ParseScript(source);
        AnalyzeAst();
    }

    void AnalyzeAst()
    {
        Dictionary<string, string> prototypeAliases = [];
        int prototypeAliasCounter = 0;

        void RegisterRelatedMemberAssignment(
            string memberName,
            VariableMetadata metadata)
        {
            HashSet<string> baseNames = [];

            int prototypeIndex = memberName.IndexOf(".prototype.");
            if (prototypeIndex != -1)
                baseNames.Add(memberName.Substring(0, prototypeIndex));

            int computedIndex = memberName.IndexOf("[");
            if (computedIndex != -1)
                baseNames.Add(memberName.Substring(0, computedIndex));

            foreach (string baseName in baseNames)
            {
                if (RelatedMemberAssignments.TryGetValue(baseName, out HashSet<VariableMetadata> existing))
                    existing.Add(metadata);
                else
                    RelatedMemberAssignments[baseName] = [metadata];
            }
        }

        void RegisterPrototypeAliasAssignment(
            string baseName,
            Node node,
            Node right,
            string syntheticHint)
        {
            string syntheticName = $"[[proto:{baseName}:{syntheticHint}:{prototypeAliasCounter++}]]";

            VariableMetadata metadata = new(
                syntheticName,
                node,
                null,
                FindDependencies(right, syntheticName),
                [],
                false);
            metadata.Dependencies.Add(baseName);

            if (PrototypeAliasAssignments.TryGetValue(baseName, out HashSet<VariableMetadata> existing))
                existing.Add(metadata);
            else
                PrototypeAliasAssignments[baseName] = [metadata];
        }


        BlockStatement? lifeBody = null;
        foreach (Statement statement in ast.Body)
        {
            if (statement is not ExpressionStatement expressionStmt ||
                expressionStmt.Expression is not CallExpression callExpr ||
                callExpr.Callee is not FunctionExpression functionExpr)
                continue;

            Node? firstParam = functionExpr.Params.FirstOrDefault();

            if (LifeParamName is null && firstParam is Identifier identifier)
                LifeParamName = identifier.Name;

            if (functionExpr.Body is BlockStatement blockStmt)
            {
                lifeBody = blockStmt;
                break;
            }
        }

        if (lifeBody is null)
            return;

        AstWalker.Walk(lifeBody, (currentNode, parent, ancestors) =>
        {
            if (currentNode != lifeBody && (currentNode is FunctionDeclaration || currentNode is FunctionExpression || currentNode is ArrowFunctionExpression))
                return AstWalker.SKIP;


            switch (currentNode)
            {
                case ExpressionStatement expressionStmt:
                    if (expressionStmt.Expression is not AssignmentExpression assignmentExpr)
                        break;

                    var right = assignmentExpr.Right;

                    if (assignmentExpr.Left is Identifier leftId)
                    {
                        if (!DeclaredVariables.TryGetValue(leftId.Name, out VariableMetadata existingVariable))
                            break;

                        // holy shit, luan wtf did u do here?? this a hack frfr
                        // why u manually overwriting the init; breaks the entire node code lookup
                        if (existingVariable.Node is VariableDeclarator variableDecl)
                        {
                            // now im requried to this even hackier reflection shit smh
                            FieldInfo? initField = typeof(VariableDeclarator).GetField("<Init>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                            initField?.SetValue(variableDecl, assignmentExpr.Right);

                            // this would be better but i dont know the side effects
                            //existingVariable.Node = new VariableDeclarator(variableDecl.Id, assignmentExpr.Right)
                            //{
                            //    Location = variableDecl.Location,
                            //    Range = variableDecl.Range,
                            //    UserData = variableDecl.UserData,
                            //};
                        }

                        if (NeedsDependencyAnalysis(right))
                            existingVariable.Dependencies = FindDependencies(right, leftId.Name);

                        if (existingVariable.Node is not null && TryMatch(existingVariable.Node, existingVariable))
                            return AstWalker.STOP;
                    }
                    else if (assignmentExpr.Left is MemberExpression memberExpr)
                    {
                        string? memberName = memberExpr.MemberToString(Source);
                        if (memberName is null)
                            break;

                        string? rightMemberName = right is MemberExpression rm ? rm.MemberToString(Source) : null;

                        // Handle prototype aliases
                        if (rightMemberName?.EndsWith(".prototype") == true)
                        {
                            string baseName = rightMemberName.Substring(0, rightMemberName.Length - ".prototype".Length);
                            prototypeAliases[memberName] = baseName;

                            RegisterPrototypeAliasAssignment(baseName, currentNode, right, memberName);
                        }
                        else
                        {
                            string? aliasObjectName = memberExpr.MemberBaseName(Source);

                            if (aliasObjectName is not null && prototypeAliases.TryGetValue(aliasObjectName, out string protoBase))
                                RegisterPrototypeAliasAssignment(protoBase, currentNode, right, memberName);
                        }

                        VariableMetadata metadata = DeclaredVariables.TryGetValue(memberName, out VariableMetadata exisiting) ? exisiting : new(
                            memberName,
                            currentNode,
                            currentNode,
                            [],
                            DependentsTracker.TryGetValue(memberName, out HashSet<string> dependents) ? dependents : [],
                            false);
                        metadata.Node = currentNode;
                        metadata.EmitNode = currentNode;
                        metadata.Dependencies = FindDependencies(right, memberName);
                        metadata.IsPredeclared = false;

                        string? baseName2 = memberExpr.MemberBaseName(Source);
                        if (baseName2 is not null && baseName2 != memberName && !baseName2.StartsWith("this."))
                            metadata.Dependencies.Add(baseName2.Replace(".prototype", ""));

                        DependentsTracker.Remove(memberName);
                        DeclaredVariables[memberName] = metadata;
                        RegisterRelatedMemberAssignment(memberName, metadata);

                        if (TryMatch(currentNode, metadata))
                            return AstWalker.STOP;
                    }
                    break;

                case VariableDeclaration variableDecl:
                    foreach (var declaration in variableDecl.Declarations)
                    {
                        if (declaration.Id is not Identifier declId) continue;

                        var metadata = new VariableMetadata(
                            declId.Name,
                            declaration,
                            parent is ForStatement fs && fs.Init == variableDecl ? parent : declaration,
                            [],
                            DependentsTracker.TryGetValue(declId.Name, out HashSet<string> dependents) ? dependents : [],
                            false);

                        if (declaration.Init is null && variableDecl.Kind == VariableDeclarationKind.Var)
                            metadata.IsPredeclared = true;
                        else if (declaration.Init is not null && NeedsDependencyAnalysis(declaration.Init))
                            metadata.Dependencies = FindDependencies(declaration.Init, metadata.Name);

                        DependentsTracker.Remove(metadata.Name);
                        DeclaredVariables[metadata.Name] = metadata;

                        if (TryMatch(declaration, metadata))
                            return AstWalker.STOP;
                    }
                    break;
            }

            return AstWalker.NORMAL;
        });
    }


    public string Source { get; }
    public string? LifeParamName { get; private set; } = null;

    public Dictionary<string, VariableMetadata> DeclaredVariables { get; } = [];
    public Dictionary<string, HashSet<VariableMetadata>> PrototypeAliasAssignments { get; } = [];
    public Dictionary<string, HashSet<VariableMetadata>> RelatedMemberAssignments { get; } = [];
    public Dictionary<string, HashSet<string>> DependentsTracker { get; } = [];

    public IEnumerable<ExtractionState> ExtractionStates => extractionStates.Where(state => state.Node is not null);


    bool NeedsDependencyAnalysis(
        Node node) =>
        node switch
        {
            FunctionExpression or
            ArrowFunctionExpression or
            ArrayExpression or
            LogicalExpression or
            CallExpression or
            NewExpression or
            MemberExpression or
            BinaryExpression or
            ConditionalExpression or
            ObjectExpression or
            SequenceExpression or
            Identifier => true,

            _ => false,
        };

    HashSet<string> FindDependencies(
        Node node,
        string identifierName)
    {
        List<Scope> scopeStack = [];
        scopeStack.Add(new([], "block"));

        bool IsInScope(
            string name)
        {
            for (int i = scopeStack.Count - 1; i >= 0; i--)
            {
                if (scopeStack[i].Names.Contains(name))
                    return true;
            }
            return false;
        }


        string? rootIdentifierName = node switch
        {
            IFunction fn when fn.Id is Identifier id => id.Name,
            IClass cls when cls.Id is Identifier id => id.Name,
            VariableDeclarator vd when vd.Id is Identifier id => id.Name,
            _ => null
        };

        void CollectBindingIdentifiers(
            Node? pattern,
            HashSet<string> target)
        {
            if (pattern is null)
                return;

            switch (pattern)
            {
                case Identifier id:
                    target.Add(id.Name);
                    break;

                case ObjectPattern objPattern:
                    foreach (Node prop in objPattern.Properties)
                    {
                        if (prop is RestElement rest)
                            CollectBindingIdentifiers(rest.Argument, target);
                        else if (prop is Property property)
                            CollectBindingIdentifiers(property.Value, target);
                    }
                    break;

                case ArrayPattern arrayPtrn:
                    foreach (Node? element in arrayPtrn.Elements)
                        CollectBindingIdentifiers(element, target);
                    break;

                case RestElement restElmt:
                    CollectBindingIdentifiers(restElmt.Argument, target);
                    break;

                case AssignmentPattern assignmentPtrn:
                    CollectBindingIdentifiers(assignmentPtrn.Left, target);
                    break;
            }
        }

        void CollectParams(
            IFunction function,
            HashSet<string> target)
        {
            foreach (Node param in function.Params)
                CollectBindingIdentifiers(param, target);
        }


        HashSet<string> dependencies = [];

        AstWalker.Walk(node,
            (currentNode, parent, ancestors) =>
            {
                switch (currentNode)
                {
                    case IFunction function:
                        string? functionName = function.Id?.Name;

                        if (function is FunctionDeclaration && functionName is not null)
                            scopeStack[scopeStack.Count - 1].Names.Add(functionName);

                        Scope functionScope = new([], "function");
                        if (function is FunctionExpression && functionName is not null)
                            functionScope.Names.Add(functionName);

                        CollectParams(function, functionScope.Names);
                        scopeStack.Add(functionScope);
                        break;

                    case BlockStatement blockStmt:
                        scopeStack.Add(new([], "block"));
                        break;

                    case CatchClause catchClse:
                        HashSet<string> set = [];

                        CollectBindingIdentifiers(catchClse.Param, set);
                        scopeStack.Add(new(set, "block"));
                        break;

                    case VariableDeclaration variableDecl:
                        Scope currentScope = scopeStack[scopeStack.Count - 1];

                        foreach (VariableDeclarator declaration in variableDecl.Declarations)
                            CollectBindingIdentifiers(declaration.Id, currentScope.Names);
                        break;

                    case ClassDeclaration classDecl:
                        if (classDecl.Id is Identifier classId)
                            scopeStack[scopeStack.Count - 1].Names.Add(classId.Name);
                        break;

                    case LabeledStatement labeledStmt:
                        scopeStack[scopeStack.Count - 1].Names.Add(labeledStmt.Label.Name);
                        break;

                    case Identifier identifier:
                        if (identifier.Name == rootIdentifierName)
                            return AstWalker.NORMAL;

                        if (parent is Property prop && prop.Key == identifier && !prop.Computed)
                            return AstWalker.NORMAL;

                        if (parent is MemberExpression memberExpr && memberExpr.Property == identifier && !memberExpr.Computed)
                        {
                            if (memberExpr.Object is ThisExpression)
                                return AstWalker.NORMAL;

                            string? full = memberExpr.MemberToString(Source);
                            if (full is null)
                                return AstWalker.NORMAL;

                            if (DeclaredVariables.TryGetValue(full, out VariableMetadata declaredVariable))
                            {
                                declaredVariable.Dependents.Add(identifierName);
                                dependencies.Add(full);
                            }
                            else if (memberExpr.Object is Identifier objectId)
                            {
                                if ((DeclaredVariables.TryGetValue(objectId.Name, out VariableMetadata? declaredBaseVariable) || objectId.Name == LifeParamName) &&
                                    !IsInScope(objectId.Name) &&
                                    !BuiltIns.Contains(objectId.Name)
                                    )
                                {
                                    declaredBaseVariable?.Dependents.Add(identifierName);
                                    dependencies.Add(full);

                                    if (DependentsTracker.TryGetValue(full, out HashSet<string> existingTracker))
                                        existingTracker.Add(identifierName);
                                    else
                                        DependentsTracker[full] = [identifierName];
                                }
                            }
                            return AstWalker.NORMAL;
                        }

                        if (IsInScope(identifier.Name) || BuiltIns.Contains(identifier.Name))
                            return AstWalker.NORMAL;

                        dependencies.Add(identifier.Name);

                        if (DeclaredVariables.TryGetValue(identifier.Name, out VariableMetadata DeclaredVariableshit))
                            DeclaredVariableshit.Dependents.Add(identifierName);
                        else
                        {
                            if (DependentsTracker.TryGetValue(identifier.Name, out HashSet<string> existingTracker))
                                existingTracker.Add(identifierName);
                            else
                                DependentsTracker[identifier.Name] = [identifierName];
                        }
                        break;
                }
                return AstWalker.NORMAL;
            },

            (currentNode, parent, ancestors) =>
            {
                switch (currentNode)
                {
                    case IFunction:
                    case BlockStatement:
                    case CatchClause:
                        if (scopeStack.Count > 1)
                            scopeStack.RemoveAt(scopeStack.Count - 1);
                        break;
                }
                return AstWalker.NORMAL;
            });

        return dependencies;
    }

    bool AreDependenciesResolved(
        HashSet<string> dependencies,
        HashSet<string> seen)
    {
        if (dependencies.Count < 1)
            return true;

        foreach (string dependency in dependencies)
        {
            if (BuiltIns.Contains(dependency))
                continue;

            if (dependency == LifeParamName)
                continue;

            if (seen.Contains(dependency))
                continue;

            if (!DeclaredVariables.TryGetValue(dependency, out VariableMetadata? depMeta))
                return false;

            seen.Add(dependency);

            if (!AreDependenciesResolved(depMeta.Dependencies, seen))
                return false;
        }

        return true;
    }


    bool TryMatch(
        Node node,
        VariableMetadata? metadata)
    {
        if (extractionStates.Length < 1)
            return false;

        bool isMatched = false;
        Node? result = null;

        foreach (ExtractionState state in extractionStates)
        {
            if (state.Node is null)
            {
                if (node is VariableDeclarator variableDeclarator && variableDeclarator.Init is null)
                    continue;

                result = state.Extractor.TryMatch(node);
                if (result is null)
                    continue;

                state.Node = node;
            }
            else if (state.Node != node)
            {
                RefreshExtractionState(state);

                if (ShouldStopTraversal())
                    return true;

                continue;
            }

            isMatched = true;

            if (metadata is not null)
            {
                state.Metadata = metadata;
                state.Dependencies = metadata.Dependencies;
                state.Dependents = metadata.Dependents;
                if (result is not null)
                    state.MatchContext = result;
            }

            RefreshExtractionState(state);
        }

        if (!isMatched)
            return false;

        return ShouldStopTraversal();
    }

    bool ShouldStopTraversal()
    {
        if (extractionStates.Length < 1)
            return false;

        bool hasStoppingTarget = false;
        foreach (ExtractionState state in extractionStates)
        {
            hasStoppingTarget = true;

            if (state.Node is null || !state.IsReady)
                return false;
        }

        return hasStoppingTarget;
    }

    void RefreshExtractionState(
        ExtractionState state)
    {
        if (state.Node is null)
        {
            state.IsReady = false;
            return;
        }

        if (state.Extractor.CollectDependencies == false)
        {
            state.IsReady = true;
            return;
        }

        if (state.Metadata is null)
        {
            state.IsReady = false;
            return;
        }

        state.IsReady = AreDependenciesResolved(state.Dependencies, []);
    }
}