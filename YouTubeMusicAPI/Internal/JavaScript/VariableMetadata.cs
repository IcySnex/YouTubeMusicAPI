using Acornima.Ast;

namespace YouTubeMusicAPI.Internal.JavaScript;

internal class VariableMetadata(
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