using Acornima;
using Acornima.Ast;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace YouTubeMusicAPI.Internal.JavaScript;

internal class JsExtractor(
    JsAnalyzer analyzer,
    int maxDepth = int.MaxValue,
    bool forceVarPredeclaration = false,
    bool isStrictMode = true, // aka disallowSideEffectInitializers
    string[]? skipEmitFor = null) // aka disallowSideEffectInitializers
{
    public class Result(
        string output,
        IReadOnlyList<string> exported,
        IReadOnlyDictionary<string, object?> exportedRawValues)
    {
        public string Output { get; } = output;

        public IReadOnlyList<string> Exported { get; } = exported;

        public IReadOnlyDictionary<string, object?> ExportedRawValues { get; } = exportedRawValues;
    }


    readonly JsAnalyzer analyzer = analyzer;
    readonly int MaxDepth = maxDepth;
    readonly bool forceVarPredeclaration = forceVarPredeclaration;
    readonly bool isStrictMode = isStrictMode;
    readonly string[] skipEmitFor = skipEmitFor ?? [];


    bool AreSafeArgs(
        IEnumerable<Expression?> args)
    {
        foreach (Node? arg in args)
        {
            if (arg is null)
                return false;

            if (arg is SpreadElement)
                return false;

            if (!IsSafeInitializer(arg))
                return false;
        }

        return true;
    }

    bool IsSafeInitializer(
        Node? node)
    {
        if (node is null)
            return true;

        switch (node)
        {
            case TemplateLiteral templateLiteral:
                return templateLiteral.Expressions.All(expr => IsSafeInitializer(expr));

            case Literal literal:
                return
                    literal.Value is string ||
                    literal.Value is double ||
                    literal.Value is bool ||
                    literal.Value is null ||
                    literal is RegExpLiteral;

            case ArrayExpression arrayExpr:
                return arrayExpr.Elements.All(element =>
                {
                    if (element is SpreadElement)
                        return false;

                    return IsSafeInitializer(element);
                });

            case ObjectExpression objectExpr:
                return objectExpr.Properties.All(prop =>
                {
                    if (prop is not Property property)
                        return false;

                    if (property.Computed ||
                        property.Kind != PropertyKind.Init)
                        return false;

                    if (property.Value is null)
                        return false;

                    return property.Value is FunctionExpression ||
                        property.Value is ArrowFunctionExpression ||
                        property.Value is Literal;
                });

            case CallExpression callExpr:
                if (callExpr.Callee is Identifier callExprCalleeId && JsAnalyzer.BuiltIns.Contains(callExprCalleeId.Name))
                {
                    return AreSafeArgs(callExpr.Arguments);
                }
                else if (callExpr.Callee is MemberExpression callExprCalleeMemberExpr)
                {
                    if (!IsSafeInitializer(callExprCalleeMemberExpr.Object))
                        return false;

                    if (isStrictMode)
                    {
                        string propertyName = callExprCalleeMemberExpr.Property is Identifier propertyId ? propertyId.Name : "";
                        if (callExprCalleeMemberExpr.Computed || !JsAnalyzer.BuiltIns.Contains(propertyName))
                            return false;
                    }

                    return AreSafeArgs(callExpr.Arguments);
                }

                return false;

            case NewExpression newExpr:
                if (newExpr.Callee is Identifier newExprCalleeId &&
                    (JsAnalyzer.BuiltIns.Contains(newExprCalleeId.Name) || !isStrictMode))
                    return AreSafeArgs(newExpr.Arguments);

                return false;

            case UnaryExpression unaryExpr:
                return IsSafeInitializer(unaryExpr.Argument);

            case FunctionExpression:
            case ArrowFunctionExpression:
            case Identifier:
                return true;

            case MemberExpression memberExpr:
                if (!isStrictMode)
                {
                    if (memberExpr.Computed && !IsSafeInitializer(memberExpr.Property))
                        return false;

                    return IsSafeInitializer(memberExpr.Object);
                }

                return false;

            case BinaryExpression binaryExpr:
                return IsSafeInitializer(binaryExpr.Left) &&
                    IsSafeInitializer(binaryExpr.Right);

            case ConditionalExpression conditionalExpr:
                if (!isStrictMode)
                    return IsSafeInitializer(conditionalExpr.Test) &&
                        IsSafeInitializer(conditionalExpr.Consequent) &&
                        IsSafeInitializer(conditionalExpr.Alternate);

                return false;

            case SequenceExpression sequenceExpr:
                if (!isStrictMode)
                    return sequenceExpr.Expressions.All(expr => IsSafeInitializer(expr));

                return false;

            case AssignmentExpression assignmentExpr:
                if (assignmentExpr.Left is MemberExpression assigmentExprLeftMemberExpr && !assigmentExprLeftMemberExpr.Computed)
                {
                    if (assigmentExprLeftMemberExpr.Object is Identifier identifier &&
                        analyzer.DeclaredVariables.TryGetValue(identifier.Name, out VariableMetadata metadata) &&
                        metadata.Node is VariableDeclarator variableDecl &&
                        variableDecl.Init is not null)
                        return IsSafeInitializer(assignmentExpr.Right);
                }
                else if (assignmentExpr.Left is Identifier identifier)
                {
                    if (analyzer.DeclaredVariables.ContainsKey(identifier.Name))
                        return IsSafeInitializer(assignmentExpr.Right);
                }

                return false;

            default:
                return false;
        }
    }

    string GetInitializerFallback(
        Expression? init) =>
        init switch
        {
            ObjectExpression or
            NewExpression or
            MemberExpression or
            LogicalExpression => "{}",

            ArrayExpression => "[]",

            _ => "undefined",
        };


    string RenderNode(
        Node? node,
        bool isPreDeclared)
    {
        AssignmentExpression? assignmentTarget = node is AssignmentExpression assignmentExpr
            ? assignmentExpr
            : node is ExpressionStatement expressionStmt && expressionStmt.Expression is AssignmentExpression innerAssignmentExpr
                ? innerAssignmentExpr
                : null;

        Expression? init = assignmentTarget is not null && assignmentTarget.Operator == Operator.Assignment
            ? assignmentTarget.Right
            : node is VariableDeclarator innerVariableDecl
                ? innerVariableDecl.Init
                : null;
        bool forceRemove = init is not null && !IsSafeInitializer(init);
        string initializerFallback = GetInitializerFallback(init);

        string initSource = initializerFallback;

        if (!forceRemove && init is not null)
        {
            if (isPreDeclared || init is not Identifier identifier || analyzer.DeclaredVariables.ContainsKey(identifier.Name))
            {
                if (assignmentTarget?.Left is MemberExpression memberExpr && init is not null)
                {
                    if (memberExpr.Object is Identifier &&
                        init is not FunctionExpression &&
                        init is not ArrowFunctionExpression &&
                        init is not LogicalExpression)
                        return $"   // Skipped {memberExpr.MemberToString(analyzer.Source)} assignment.";
                }

                initSource = init?.ExtractNodeSource(analyzer.Source) is string s
                    ? Regex.Replace(s, @";\s*$", "")
                    : "kk"; // wtf is kk ??
            }
        }

        if (!forceRemove && init is SequenceExpression && !initSource.StartsWith("("))
            initSource = $"({initSource})";

        string idName = node is VariableDeclarator variableDecl && variableDecl.Id is Identifier id
            ? id.Name
            : assignmentTarget?.Left is Identifier leftId
                ? leftId.Name
                : assignmentTarget?.Left is Identifier assigmentTargetLeftIdentifier
                    ? assigmentTargetLeftIdentifier.Name
                    : assignmentTarget is AssignmentExpression actualHolyShitAssignmentExpr
                        ? (actualHolyShitAssignmentExpr.Left is MemberExpression imGogingCrazyMemberExpr
                            ? imGogingCrazyMemberExpr.MemberToString(analyzer.Source) ?? "unknown"
                            : "unknown")
                        : "unknown";

        string assignmentExpression = $"{idName} = {initSource};";

        if (node is VariableDeclarator thirdsTimesTheCharmVariableDecl && thirdsTimesTheCharmVariableDecl.Init is not null && !isPreDeclared)
            return $"   var {assignmentExpression}";

        return $"   {assignmentExpression}";
    }


    public Result BuildScript()
    {
        IEnumerable<ExtractionState> extractions = analyzer.ExtractionStates;
        HashSet<string> seen = [.. extractions.Select(state => state.Metadata?.Name ?? "")];

        List<string> snippsets = [];
        HashSet<string> predeclaredVarSet = [];
        Dictionary<string, Node> exported = [];
        Dictionary<string, object?> exportedRawValues = [];

        void RegisterPredeclaredVar(
            string? name)
        {
            if (name is null || name.Contains('.'))
                return;

            predeclaredVarSet.Add(name);
        }

        void Visit(
            VariableMetadata? metadata,
            int depth = 0)
        {
            if (metadata is null || depth > MaxDepth)
                return;

            foreach (string dependency in metadata.Dependencies)
            {
                if (seen.Contains(dependency))
                    continue;

                seen.Add(dependency);

                if (!analyzer.DeclaredVariables.TryGetValue(dependency, out VariableMetadata? dependencyMetadata))
                    continue;

                bool shouldPredeclare = forceVarPredeclaration || dependencyMetadata.IsPredeclared;
                if (shouldPredeclare)
                    RegisterPredeclaredVar(dependencyMetadata.Name);

                if (!dependency.Contains('.'))
                    Visit(dependencyMetadata, depth + 1);

                snippsets.Add(RenderNode(dependencyMetadata.Node, shouldPredeclare));
            }
        }

        foreach (ExtractionState extraction in extractions)
        {
            string name = extraction.Extractor.Name;
            bool shouldSkip = skipEmitFor.Contains(name);

            if (extraction.Metadata is not null)
            {
                if (!shouldSkip)
                    snippsets.Add($"    //#region --- start [{name}] ---");

                bool shouldPredeclare = (forceVarPredeclaration || extraction.Metadata.IsPredeclared) && !shouldSkip;
                if (shouldPredeclare)
                    RegisterPredeclaredVar(extraction.Metadata.Name);

                if (extraction.Extractor.CollectDependencies && !shouldSkip)
                    Visit(extraction.Metadata);

                if (extraction.MatchContext is not null)
                {
                    exported.Add(name, extraction.MatchContext);

                    string? rawValue = null;

                    if (extraction.MatchContext is Property property)
                        rawValue = property.Value.ExtractNodeSource(analyzer.Source);
                    else if (extraction.MatchContext is Identifier identifier)
                        rawValue = identifier.Name;
                    else
                        rawValue = extraction.MatchContext.ExtractNodeSource(analyzer.Source);

                    exportedRawValues[name] = rawValue;
                }

                if (!shouldSkip)
                    snippsets.Add($"    //#endregion --- end [{name}] ---\n");
            }
        }

        StringBuilder output = new();

        // safety I GUESS
        output.AppendLine("const window = Object.assign({}, globalThis);");
        output.AppendLine("const document = {};");
        output.AppendLine("const self = window;\n");

        output.AppendLine($"const exportedVars = (function({analyzer.LifeParamName}) {{");
        if (predeclaredVarSet.Count > 0)
            output.AppendLine($"    var {string.Join(", ", predeclaredVarSet)};\n");

        output.AppendLine(string.Join("\n", snippsets));

        List<string> exportedVars = [];
        foreach (KeyValuePair<string, Node> export in exported)
        {
            Node? currentFunctionNode = null;

            if (export.Value is Identifier identifier)
            {
                if (analyzer.DeclaredVariables.TryGetValue(identifier.Name, out VariableMetadata? metadata) &&
                    metadata.Node is VariableDeclarator variableDecl &&
                    variableDecl.Init is FunctionExpression)
                    currentFunctionNode = metadata.Node;
            }
            else if (export.Value is CallExpression)
            {
                currentFunctionNode = export.Value;
            }

            string? wrapper = currentFunctionNode?.CreateWrapperFunction(export.Key, analyzer);
            if (wrapper is not null)
            {
                output.AppendLine($"{wrapper}\n");
                exportedVars.Add(export.Key);
            }
        }

        // export raw values: ALWAYS TRUE WUHWUHWUHWUHW (? im going crazy)
        string rawJson = JsonConvert.SerializeObject(exportedRawValues, Formatting.Indented);
        string[] rawJsonLines = rawJson.Split('\n');

        string formattedRawJson = rawJsonLines.Length > 0
            ? $"{rawJsonLines[0]}\n{string.Join("\n", rawJsonLines.Skip(1).Select(line => $"    {line}"))}"
            : rawJson;

        output.AppendLine($"    const rawValues = {formattedRawJson};\n");
        exportedVars.Add("rawValues");

        output.AppendLine($"    return {{ {string.Join(", ", exportedVars)} }};");
        output.AppendLine("})({});\n");

        return new(
            output.ToString(),
            exportedVars,
            exportedRawValues);
    }
}