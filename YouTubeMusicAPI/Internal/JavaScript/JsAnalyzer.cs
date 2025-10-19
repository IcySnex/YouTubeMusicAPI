using Acornima;
using Acornima.Ast;

namespace YouTubeMusicAPI.Internal.JavaScript;

internal class JsAnalyzer
{
    class ExtractionState(
        (string Name, JsMatchers.Delegate TryMatch, bool CollectDependencies) extractor,
        Node? node,
        VariableMetadata? metadata,
        HashSet<string> dependencies,
        HashSet<string> dependents,
        Node? matchContext,
        bool isReady)
    {
        public (string Name, JsMatchers.Delegate TryMatch, bool CollectDependencies) Extractor { get; set; } = extractor;

        public Node? Node { get; set; } = node;

        public VariableMetadata? Metadata { get; set; } = metadata;

        public HashSet<string> Dependencies { get; set; } = dependencies;

        public HashSet<string> Dependents { get; set; } = dependents;

        public Node? MatchContext { get; set; } = matchContext;

        public bool IsReady { get; set; } = isReady;
    }

    class VariableMetadata(
        string name,
        Node? node,
        HashSet<string> dependencies,
        HashSet<string> dependents,
        bool isPredeclared)
    {
        public string Name { get; set; } = name;

        public Node? Node { get; set; } = node;

        public HashSet<string> Dependencies { get; set; } = dependencies;

        public HashSet<string> Dependents { get; set; } = dependents;

        public bool IsPredeclared { get; set; } = isPredeclared;
    }

    class Scope(
        HashSet<string> names,
        string type)
    {
        public HashSet<string> Names { get; set; } = names;

        public string Type { get; set; } = type;
    }


    static readonly HashSet<string> JsBuiltIns =
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


    readonly string source;
    readonly ExtractionState[] extractionStates;

    readonly Script ast;

    readonly Dictionary<string, VariableMetadata> declaredVariables = [];
    readonly Dictionary<string, HashSet<string>> dependentsTracker = [];

    string? lifeParamName = null;

    public JsAnalyzer(
        string source,
        (string FucntionName, JsMatchers.Delegate TryMatch, bool CollectDependencies)[] extractors)
    {
        this.source = source;

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

            if (lifeParamName is null && firstParam is Identifier identifier)
                lifeParamName = identifier.Name;

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
                            dependentsTracker.TryGetValue(declerationId.Name, out HashSet<string> dependents) ? dependents : [],
                            [],
                            false);

                        Expression? init = declaration.Init;

                        if (init is null && variableDecl.Kind == VariableDeclarationKind.Var)
                            metadata.IsPredeclared = true;
                        else if (init is not null && NeedsDependencyAnalysis(init))
                            metadata.Dependents = FindDependencies(init, metadata.Name);

                        dependentsTracker.Remove(metadata.Name);
                        declaredVariables.Add(metadata.Name, metadata);

                        if (TryMatch(declaration, metadata))
                            return;
                    }
                    break;

                case ExpressionStatement expressionStmt:
                    if (expressionStmt.Expression is not AssignmentExpression assignmentExpr)
                        continue;

                    if (assignmentExpr.Left is Identifier leftId)
                    {
                        if (!declaredVariables.TryGetValue(leftId.Name, out VariableMetadata existingVariable))
                            continue;

                        if (existingVariable.Node is VariableDeclarator variableDecl)                               // holy shit, luan wtf did u do here?? this a hack frfr
                            existingVariable.Node = new VariableDeclarator(variableDecl.Id, assignmentExpr.Right);  // why u manually overwriting the init; breaks the entire node code lookup

                        if (NeedsDependencyAnalysis(assignmentExpr.Right))
                            existingVariable.Dependencies = FindDependencies(assignmentExpr.Right, leftId.Name);

                        if (existingVariable.Node is not null && TryMatch(existingVariable.Node, existingVariable))
                            return;
                    }
                    else if (assignmentExpr.Left is MemberExpression memberExpr)
                    {
                        string? memberName = memberExpr.MemberToString(source);

                        if (memberName is null || declaredVariables.ContainsKey(memberName))
                            continue;

                        VariableMetadata metadata = new(
                            memberName,
                            currentNode,
                            FindDependencies(assignmentExpr.Right, memberName),
                            dependentsTracker.TryGetValue(memberName, out HashSet<string> dependents) ? dependents : [],
                            false);

                        string? baseName = memberExpr.MemberBaseName(source);
                        if (baseName is not null && baseName != memberName && !baseName.StartsWith("this."))
                            metadata.Dependencies.Add(baseName.Replace(".prototype", ""));

                        dependentsTracker.Remove(memberName);
                        declaredVariables.Add(memberName, metadata);

                        if (TryMatch(currentNode, metadata))
                            return;
                    }
                    break;
            }
        }
    }


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
        Stack<Scope> scopeStack = [];
        scopeStack.Push(new([], "block"));

        bool IsInScope(
            string name)
        {
            foreach (Scope scope in scopeStack)
            {
                if (scope.Names.Contains(name))
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
                            scopeStack.Peek().Names.Add(functionName);

                        Scope functionScope = new([], "function");
                        if (function is FunctionExpression &&  functionName is not null)
                            functionScope.Names.Add(functionName);

                        CollectParams(function, functionScope.Names);
                        scopeStack.Push(functionScope);
                        break;

                    case BlockStatement blockStmt:
                        scopeStack.Push(new([], "block"));
                        break;

                    case CatchClause catchClse:
                        HashSet<string> set = [];

                        CollectBindingIdentifiers(catchClse.Param, set);
                        scopeStack.Push(new(set, "block"));
                        break;

                    case VariableDeclaration variableDecl:
                        Scope currentScope = scopeStack.Peek();

                        foreach (var declaration in variableDecl.Declarations)
                            CollectBindingIdentifiers(declaration.Id, currentScope.Names);
                        break;

                    case ClassDeclaration classDecl:
                        if (classDecl.Id is Identifier classId)
                            scopeStack.Peek().Names.Add(classId.Name);
                        break;

                    case LabeledStatement labeledStmt:
                        scopeStack.Peek().Names.Add(labeledStmt.Label.Name);
                        break;

                    case Identifier identifier:
                        if (identifier.Name == rootIdentifierName)
                            return AstWalker.CONTINUE;

                        if (parent is Property prop && prop.Key == identifier && !prop.Computed)
                            return AstWalker.CONTINUE;

                        if (parent is MemberExpression memberExpr && memberExpr.Property == identifier && !memberExpr.Computed)
                        {
                            if (memberExpr.Object is ThisExpression)
                                return AstWalker.CONTINUE;

                            string? full = memberExpr.MemberToString(source);
                            if (full is null)
                                return AstWalker.CONTINUE;

                            if (declaredVariables.TryGetValue(full, out VariableMetadata declaredVariable))
                            {
                                declaredVariable.Dependents.Add(identifierName);
                                dependencies.Add(full);
                            }
                            else if (memberExpr.Object is Identifier objectId)
                            {
                                if ((
                                        declaredVariables.TryGetValue(objectId.Name, out VariableMetadata? declaredBaseVariable) ||
                                        objectId.Name == lifeParamName
                                    ) &&
                                    !IsInScope(objectId.Name) &&
                                    !JsBuiltIns.Contains(objectId.Name)
                                    )
                                {
                                    declaredBaseVariable?.Dependents.Add(identifierName);
                                    dependencies.Add(full);

                                    if (dependentsTracker.TryGetValue(full, out HashSet<string> existingTracker))
                                        existingTracker.Add(identifierName);
                                    else
                                        dependentsTracker.Add(full, [ identifierName ]);
                                }
                            }
                            return AstWalker.CONTINUE;
                        }

                        if (IsInScope(identifier.Name) || JsBuiltIns.Contains(identifier.Name))
                            return AstWalker.CONTINUE;

                        dependencies.Add(identifier.Name);

                        if (declaredVariables.TryGetValue(identifierName, out VariableMetadata declaredVariableShit))
                            declaredVariableShit.Dependents.Add(identifierName);
                        else
                        {
                            if (dependentsTracker.TryGetValue(identifier.Name, out HashSet<string> existingTracker))
                                existingTracker.Add(identifierName);
                            else
                                dependentsTracker.Add(identifier.Name, [ identifierName ]);
                        }

                        break;
                }
                return AstWalker.CONTINUE;
            },

            (currentNode, parent, ancestors) =>
            {
                switch (currentNode)
                {
                    case IFunction:
                    case BlockStatement:
                    case CatchClause:
                        if (scopeStack.Count > 1)
                            scopeStack.Pop();
                        break;
                }
                return AstWalker.CONTINUE;
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
            if (JsBuiltIns.Contains(dependency))
                continue;

            if (dependency == lifeParamName)
                continue;

            if (seen.Contains(dependency))
                continue;

            if (!declaredVariables.TryGetValue(dependency, out VariableMetadata? depMeta))
                return false;

            seen.Add(dependency);

            if (!AreDependenciesResolved(depMeta.Dependents, seen))
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

                state.Node = result;
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
                    state.MatchContext = node;
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