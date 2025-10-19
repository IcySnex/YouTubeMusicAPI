using Acornima.Ast;

namespace YouTubeMusicAPI.Internal.JavaScript;

internal class ExtractionState(
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