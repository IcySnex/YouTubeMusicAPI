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

        foreach (Node currentNode in lifeBody.Body)
        {
            switch (currentNode)
            {
                case VariableDeclaration variableDecl:
                    foreach (VariableDeclarator declaration in variableDecl.Declarations)
                    {
                        if (declaration.Id is not Identifier declerationId)
                            continue;

                        VariableMetadata metadata = new(
                            declerationId.Name,
                            declaration,
                            DependentsTracker.TryGetValue(declerationId.Name, out HashSet<string> dependents) ? dependents : [],
                            [],
                            false);

                        Expression? init = declaration.Init;

                        if (init is null && variableDecl.Kind == VariableDeclarationKind.Var)
                            metadata.IsPredeclared = true;
                        else if (init is not null && NeedsDependencyAnalysis(init))
                            metadata.Dependencies = FindDependencies(init, metadata.Name);

                        DependentsTracker.Remove(metadata.Name);
                        DeclaredVariables.Add(metadata.Name, metadata);

                        if (TryMatch(declaration, metadata))
                            return;
                    }
                    break;

                case ExpressionStatement expressionStmt:
                    if (expressionStmt.Expression is not AssignmentExpression assignmentExpr)
                        continue;

                    if (assignmentExpr.Left is Identifier leftId)
                    {
                        if (!DeclaredVariables.TryGetValue(leftId.Name, out VariableMetadata existingVariable))
                            continue;

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

                        if (NeedsDependencyAnalysis(assignmentExpr.Right))
                            existingVariable.Dependencies = FindDependencies(assignmentExpr.Right, leftId.Name);

                        if (existingVariable.Node is not null && TryMatch(existingVariable.Node, existingVariable))
                            return;
                    }
                    else if (assignmentExpr.Left is MemberExpression memberExpr)
                    {
                        string? memberName = memberExpr.MemberToString(Source);

                        if (memberName is null || DeclaredVariables.ContainsKey(memberName))
                            continue;

                        VariableMetadata metadata = new(
                            memberName,
                            currentNode,
                            FindDependencies(assignmentExpr.Right, memberName),
                            DependentsTracker.TryGetValue(memberName, out HashSet<string> dependents) ? dependents : [],
                            false);

                        string? baseName = memberExpr.MemberBaseName(Source);
                        if (baseName is not null && baseName != memberName && !baseName.StartsWith("this."))
                            metadata.Dependencies.Add(baseName.Replace(".prototype", ""));

                        DependentsTracker.Remove(memberName);
                        DeclaredVariables.Add(memberName, metadata);

                        if (TryMatch(currentNode, metadata))
                            return;
                    }
                    break;
            }
        }
    }


    public string Source { get; }
    public string? LifeParamName { get; private set; } = null;

    public Dictionary<string, VariableMetadata> DeclaredVariables { get; } = [];
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
                        else if(prop is Property property)
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
                        if (function is FunctionExpression &&  functionName is not null)
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
                                if ((
                                        DeclaredVariables.TryGetValue(objectId.Name, out VariableMetadata? declaredBaseVariable) ||
                                        objectId.Name == LifeParamName
                                    ) &&
                                    !IsInScope(objectId.Name) &&
                                    !BuiltIns.Contains(objectId.Name)
                                    )
                                {
                                    declaredBaseVariable?.Dependents.Add(identifierName);
                                    dependencies.Add(full);

                                    if (DependentsTracker.TryGetValue(full, out HashSet<string> existingTracker))
                                        existingTracker.Add(identifierName);
                                    else
                                        DependentsTracker[full] = [ identifierName ];
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
                            {
                                DependentsTracker[identifier.Name] = [identifierName];
                            }
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