namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents search suggestions for a query on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="SearchSuggestions"/> class.
/// </remarks>
/// <param name="search">The suggested search queries, including autocomplete, corrections, or related.</param>
/// <param name="history">The past search queries from the user's history.</param>
/// <param name="results">The direct search results that are most related to the search query.</param>
public class SearchSuggestions(
    IReadOnlyList<string> search,
    IReadOnlyList<string> history,
    IReadOnlyList<SearchResult> results)
{
    /// <summary>
    /// The suggested search queries, including autocomplete, corrections, or related.
    /// </summary>
    public IReadOnlyList<string> Search { get; } = search;

    /// <summary>
    /// The past search queries from the user's history.
    /// </summary>
    public IReadOnlyList<string> History { get; } = history;

    /// <summary>
    /// The direct search results that are most related to the search query.
    /// </summary>
    public IReadOnlyList<SearchResult> Results { get; } = results;
}