using Acornima.Ast;
using System.Reflection;

namespace YouTubeMusicAPI.Internal.JavaScript;

internal static class AstWalker
{
    public const int NORMAL = 0;
    public const int STOP = 1;
    public const int SKIP = 2;

    static readonly Dictionary<Type, List<MemberInfo>> AstMembersCache = [];


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


            //List<Node> children = [.. node.ChildNodes];       // only the lord knows why this doesnt work
            //for (int i = children.Count - 1; i >= 0; i--)     // now i gotta do this expensive ahh reflection >:(
            //{
            //    Node? child = children[i];

            //    if (child is not null)
            //        stack.Push((child, node, false));
            //}

            foreach (MemberInfo member in GetAstMembers(node.GetType())) // todo: optimize this shi
            {
                object? value = member switch
                {
                    PropertyInfo pi => pi.GetValue(node),
                    FieldInfo fi => fi.GetValue(node),
                    _ => null
                };

                if (value is IEnumerable<Node> nodeEnumerable)
                {
                    foreach (Node childNode in nodeEnumerable.Reverse())
                        if (childNode is not null)
                            stack.Push((childNode, node, false));
                }
                else if (value is Node childNode)
                    stack.Push((childNode, node, false));
            }

        }
    }


    static List<MemberInfo> GetAstMembers(
        Type type)
    {
        if (AstMembersCache.TryGetValue(type, out List<MemberInfo>? cached))
            return cached;

        List<MemberInfo> members = [.. type
            .GetMembers(BindingFlags.Instance | BindingFlags.Public)
            .Where(m => m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field)
            .Where(p => p.Name != "Start" &&
                        p.Name != "End" &&
                        p.Name != "Location" &&
                        p.Name != "LocationRef" &&
                        p.Name != "Range" &&
                        p.Name != "RangeRef" &&
                        p.Name != "Type" &&
                        p.Name != "TypeText" &&
                        p.Name != "UserData" &&
                        p.Name != "ChildNodes")];

        AstMembersCache[type] = members;
        return members;
    }
}