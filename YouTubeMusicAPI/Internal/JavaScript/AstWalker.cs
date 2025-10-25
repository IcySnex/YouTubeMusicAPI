using Acornima.Ast;
using System.Reflection;

namespace YouTubeMusicAPI.Internal.JavaScript;

internal static class AstWalker
{
    public const int NORMAL = 0;
    public const int STOP = 1;
    public const int SKIP = 2;

    static readonly Dictionary<Type, Func<object, object?>[]> AstMembersCache = [];


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
            //
            //    if (child is not null)
            //        stack.Push((child, node, false));
            //}

            foreach (Func<object, object?> accessor in GetAstMembers(node.GetType()))
            {
                object? value = accessor(node);
                if (value is null)
                    continue;

                if (value is IReadOnlyList<Node?> nodesList)
                {
                    for (int i = nodesList.Count - 1; i >= 0; i--)
                    {
                        Node? childNode = nodesList[i];

                        if (childNode is not null)
                            stack.Push((childNode, node, false));
                    }
                }
                else if (value is Node childNode)
                    stack.Push((childNode, node, false));
            }

        }
    }


    static Func<object, object?>[] GetAstMembers(
        Type type)
    {
        if (AstMembersCache.TryGetValue(type, out Func<object, object?>[]? cached))
            return cached;

        MemberInfo[] members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public);
        List<Func<object, object?>> accessors = new(members.Length);

        foreach (MemberInfo member in members)
        {
            if (member.MemberType == MemberTypes.Property)
            {
                PropertyInfo property = (PropertyInfo)member;
                if (!property.CanRead || ShouldIgnoreMember(property.Name))
                    continue;

                accessors.Add(property.GetValue);
            }
            else if (member.MemberType == MemberTypes.Field)
            {
                FieldInfo field = (FieldInfo)member;

                if (ShouldIgnoreMember(field.Name))
                    continue;

                accessors.Add(field.GetValue);
            }
        }

        Func<object, object?>[] result = [.. accessors];
        AstMembersCache[type] = result;
        return result;
    }

    static bool ShouldIgnoreMember(
        string name) => name switch
        {
            "Start" or "End" or "Location" or "LocationRef" or
            "Range" or "RangeRef" or "Type" or "TypeText" or
            "UserData" or "ChildNodes" => true,
            _ => false
        };
}