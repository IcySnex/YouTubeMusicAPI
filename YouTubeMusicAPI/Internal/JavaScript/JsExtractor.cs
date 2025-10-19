using Acornima.Ast;

namespace YouTubeMusicAPI.Internal.JavaScript;

internal class JsExtractor(
    JsAnalyzer analyzer,
    bool isStrictMode) // aka disallowSideEffectInitializers
{
    readonly JsAnalyzer analyzer = analyzer;
    readonly bool isStrictMode = isStrictMode;


    bool AreSafeArgs(
        IEnumerable<Expression?> args,
        bool isStrictMode = true)
    {
        foreach (Node? arg in args)
        {
            if (arg is null)
                return false;

            if (arg is SpreadElement)
                return false;

            if (!IsSafeInitializer(arg, isStrictMode))
                return false;
        }

        return true;
    }

    bool IsSafeInitializer(
        Node? node,
        bool isStrictMode = true)
    {
        if (node is null)
            return true;

        switch (node)
        {
            case TemplateLiteral templateLiteral:
                return templateLiteral.Expressions.All(expr => IsSafeInitializer(expr, isStrictMode));

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

                    return IsSafeInitializer(element, isStrictMode);
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
                    return AreSafeArgs(callExpr.Arguments, isStrictMode);
                }
                else if (callExpr.Callee is MemberExpression callExprCalleeMemberExpr)
                {
                    if (!IsSafeInitializer(callExprCalleeMemberExpr.Object, isStrictMode))
                        return false;

                    if (isStrictMode)
                    {
                        string propertyName = callExprCalleeMemberExpr.Property is Identifier propertyId ? propertyId.Name : "";
                        if (callExprCalleeMemberExpr.Computed || !JsAnalyzer.BuiltIns.Contains(propertyName))
                            return false;
                    }

                    return AreSafeArgs(callExpr.Arguments, isStrictMode);
                }

                return false;

            case NewExpression newExpr:
                if (newExpr.Callee is Identifier newExprCalleeId &&
                    (
                        JsAnalyzer.BuiltIns.Contains(newExprCalleeId.Name) ||
                        !isStrictMode
                    ))
                    return AreSafeArgs(newExpr.Arguments, isStrictMode);

                return false;

            case UnaryExpression unaryExpr:
                return IsSafeInitializer(unaryExpr.Argument, isStrictMode);

            case FunctionExpression:
            case ArrowFunctionExpression:
            case Identifier:
                return true;

            case MemberExpression memberExpr:
                if (!isStrictMode)
                {
                    if (memberExpr.Computed && !IsSafeInitializer(memberExpr.Property, isStrictMode))
                        return false;

                    return IsSafeInitializer(memberExpr.Object, isStrictMode);
                }

                return false;

            case BinaryExpression binaryExpr:
                return IsSafeInitializer(binaryExpr.Left, isStrictMode) &&
                    IsSafeInitializer(binaryExpr.Right, isStrictMode);

            case ConditionalExpression conditionalExpr:
                if (!isStrictMode)
                    return IsSafeInitializer(conditionalExpr.Test, isStrictMode) &&
                        IsSafeInitializer(conditionalExpr.Consequent, isStrictMode) &&
                        IsSafeInitializer(conditionalExpr.Alternate, isStrictMode);

                return false;

            case SequenceExpression sequenceExpr:
                if (!isStrictMode)
                    return sequenceExpr.Expressions.All(expr => IsSafeInitializer(expr, isStrictMode));

                return false;

            case AssignmentExpression assignmentExpr:
                if (assignmentExpr.Left is MemberExpression assigmentExprLeftMemberExpr && assigmentExprLeftMemberExpr.Computed)
                {
                    if (assigmentExprLeftMemberExpr.Object is Identifier identifier &&
                        analyzer.DeclaredVariables.TryGetValue(identifier.Name, out VariableMetadata metadata) &&
                        metadata.Node is VariableDeclarator variableDecl &&
                        variableDecl.Init is not null)
                        return IsSafeInitializer(assignmentExpr.Right, isStrictMode);
                }
                else if (assignmentExpr.Left is Identifier identifier)
                {
                    if (analyzer.DeclaredVariables.ContainsKey(identifier.Name))
                        return IsSafeInitializer(assignmentExpr.Right, isStrictMode);
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


    object RenderNode(
        Node node,
        bool isPreDeclared)
    {
        bool canDissalow = true;


    }
}