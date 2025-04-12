namespace YouTubeMusicAPI.Pagination;

/// <summary>
/// A single page of results items along with a continuation token for pagination
/// </summary>
/// <typeparam name="T">The type of items</typeparam>
/// <param name="items">The list of items on the current page</param>
/// <param name="continuationToken">The continuation token used to fetch the next page (null if there are no more pages)</param>
public class Page<T>(
    IReadOnlyList<T> items,
    string? continuationToken)
{
    /// <summary>
    /// The list of items on the current page
    /// </summary>
    public IReadOnlyList<T> Items { get; } = items;

    /// <summary>
    /// The continuation token used to fetch the next page
    /// null if there are no more pages
    /// </summary>
    public string? ContinuationToken { get; } = continuationToken;
}