using Acornima.Ast;

namespace YouTubeMusicAPI.Internal.JavaScript;

internal static class JsMatchers
{
    public delegate Node? Delegate(
        Node node);


    public static Node? Signature(
        Node node)
    {
        if (node is VariableDeclarator variableDecl &&
            variableDecl.Id is Identifier &&
            variableDecl.Init is FunctionExpression functionExpr &&
            functionExpr.LooksLikeSignatureHelper())
            return node;

        return null;
    }

    public static Node? NSignature(
        Node node)
    {
        if (node is not VariableDeclarator variableDecl)
            return null;

        if (variableDecl.Id is Identifier identifier1 &&
            variableDecl.Init is FunctionExpression functionExpr &&
            functionExpr.LooksLikeSignatureHelper())
        {
            string? className = functionExpr.GetUrlHelperClassName();
            if (className is not null)
                return new Identifier(className)
                {
                    Location = identifier1.Location,
                    Range = identifier1.Range
                };
        }

        if (variableDecl.Init is ArrayExpression arrayExpr &&
            arrayExpr.Elements.Count > 0 &&
            arrayExpr.Elements[0] is Identifier firstArrayElement)
            return firstArrayElement;

        return null;
    }

    public static Node? SignatureTimestamp(
        Node node)
    {
        if (node is not VariableDeclarator variableDeclarator ||
            variableDeclarator.Init is not FunctionExpression functionExpr ||
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