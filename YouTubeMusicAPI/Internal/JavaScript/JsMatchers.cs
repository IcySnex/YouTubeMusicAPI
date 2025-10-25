using Acornima;
using Acornima.Ast;

namespace YouTubeMusicAPI.Internal.JavaScript;

internal static class JsMatchers
{
    public delegate Node? Delegate(
        Node node);


    public static Node? Signature(
        Node node)
    {
        if (node is not VariableDeclarator variableDecl ||
            variableDecl.Init is not FunctionExpression functionExpr ||
            functionExpr.Body is not BlockStatement blockStmt)
            return null;

        foreach (Statement statement in blockStmt.Body)
        {
            if (statement is not ExpressionStatement expressionStmt ||
                expressionStmt.Expression is not LogicalExpression logicalExpr ||
                logicalExpr.Operator != Operator.LogicalAnd ||
                logicalExpr.Left is not Identifier ||
                logicalExpr.Right is not SequenceExpression sequenceExpr ||
                sequenceExpr.Expressions.FirstOrDefault() is not AssignmentExpression assignmentExpr ||
                assignmentExpr.Operator != Operator.Assignment ||
                assignmentExpr.Left is not Identifier ||
                assignmentExpr.Right is not CallExpression callExpr ||
                callExpr.Callee is not Identifier ||
                callExpr.Arguments.FirstOrDefault(exp => exp is CallExpression) is not CallExpression innerCallExpr ||
                innerCallExpr.Callee is not Identifier identifier ||
                identifier.Name != "decodeURIComponent" ||
                innerCallExpr.Arguments.FirstOrDefault() is not Identifier)
                continue;

            return callExpr;
        }

        return null;
    }

    public static Node? NSignature(
        Node node)
    {
        if (node is not VariableDeclarator variableDecr ||
            variableDecr.Init is not ArrayExpression arrayExpr ||
            arrayExpr.Elements.FirstOrDefault() is not Identifier identifier)
            return null;

        return identifier;
    }

    public static Node? SignatureTimestamp(
        Node node)
    {
        if (node is not ExpressionStatement expressionStmt ||
            expressionStmt.Expression is not AssignmentExpression assignmentExpr ||
            assignmentExpr.Right is not FunctionExpression functionExpr ||
            functionExpr.Body is not BlockStatement blockStmt)
            return null;

        foreach (Statement statement in blockStmt.Body)
        {
            if (statement is not ExpressionStatement innerExpressionStmt ||
                innerExpressionStmt.Expression is not AssignmentExpression innerAsiignmentExpr ||
                innerAsiignmentExpr.Right is not ObjectExpression objectExpr)
                continue;

            foreach (Node objectProp in objectExpr.Properties)
            {
                if (objectProp is not Property prop ||
                    prop.Key is not Identifier identifier ||
                    identifier.Name != "signatureTimestamp" ||
                    prop.Value is not Literal literal ||
                    literal.Value is null)
                    continue;

                return literal;
            }
        }

        return null;
    }
}