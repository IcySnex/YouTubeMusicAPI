namespace YouTubeMusicAPI.Services.Search;

/// <summary>
/// Represents a search page for all kind of results on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="SearchPage"/> class.
/// </remarks>
/// <param name="results">The list containing the search results.</param>
/// <param name="topResult">The most relevant search result.</param>
/// <param name="telatedTopResults">The list containing the related top results.</param>
/// <param name="availableCategories">The available categories for this search.</param>
public class SearchPage(
    IReadOnlyList<SearchResult> results,
    SearchResult? topResult,
    IReadOnlyList<SearchResult> telatedTopResults,
    IReadOnlyList<SearchCategory> availableCategories)
{
    /// <summary>
    /// The list containing the items.
    /// </summary>
    public IReadOnlyList<SearchResult> Results { get; } = results;

    /// <summary>
    /// The most relevant search result.
    /// </summary>
    public SearchResult? TopResult { get; } = topResult;

    /// <summary>
    /// The list containing the related top results.
    /// </summary>
    public IReadOnlyList<SearchResult> RelatedTopResults { get; } = telatedTopResults;

    /// <summary>
    /// The available categories for this search.
    /// </summary>
    public IReadOnlyList<SearchCategory> AvailableCategories { get; } = availableCategories;
}