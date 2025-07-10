using YouTubeMusicAPI.Pagination;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a search page for all kind of results on YouTube Music.
/// </summary>
/// <param name="items">The list containing the items.</param>
/// <param name="topResult">The top result.</param>
/// <param name="telatedTopResults">The list containing the related top results.</param>
public class SearchPage(
    IReadOnlyList<SearchResult> items,
    SearchResult? topResult,
    IReadOnlyList<SearchResult> telatedTopResults) : Page<SearchResult>(items, null)
{
    /// <summary>
    /// The top result.
    /// </summary>
    public SearchResult? TopResult { get; } = topResult;

    /// <summary>
    /// The list containing the related top results.
    /// </summary>
    public IReadOnlyList<SearchResult> RelatedTopResults { get; } = telatedTopResults;
}