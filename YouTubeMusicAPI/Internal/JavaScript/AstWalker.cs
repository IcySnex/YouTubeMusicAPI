using Acornima.Ast;

namespace YouTubeMusicAPI.Internal.JavaScript;

internal static class AstWalker
{
    public const int CONTINUE = 0;
    public const int STOP = 1;
    public const int SKIP = 2;


    public delegate int AstVisitor(
        Node node,
        Node? parent,
        IReadOnlyList<Node> ancestors);

    public static void Walk(
        Node root,
        AstVisitor onEnter,
        AstVisitor? onLeave = null)
    {
        if (root is null)
            return;

        Stack<(Node Node, Node? Parent, bool Exit)> stack = [];
        stack.Push((root, null, false));

        List<Node> ancestors = [];
        bool shouldStop = false;

        while (!shouldStop && stack.Count > 0)
        {
            (Node node, Node? parent, bool exit) = stack.Pop();

            if (exit)
            {
                if (ancestors.Count > 0)
                    ancestors.RemoveAt(ancestors.Count - 1);

                if (onLeave is not null && onLeave(node, parent, ancestors) == STOP)
                    shouldStop = true;
                continue;
            }

            if (node is null)
                continue;

            int enterResult = onEnter(node, parent, ancestors);
            if (enterResult == STOP)
            {
                shouldStop = true;
                continue;
            }

            if (enterResult == SKIP)
                continue;

            stack.Push((node, parent, true));
            ancestors.Add(node);

            List<Node> children = [.. node.ChildNodes];
            for (int i = children.Count - 1; i >= 0; i--)
            {
                Node? child = children[i];

                if (child is not null)
                    stack.Push((child, node, false));
            }
        }
    }
}