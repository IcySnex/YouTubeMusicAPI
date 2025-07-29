namespace YouTubeMusicAPI.Pagination;

/// <summary>
/// Represents a single page of items.
/// </summary>
/// <typeparam name="T">The type of the items.</typeparam>
/// <param name="items">The list containing the items.</param>
/// <param name="continuationToken">The continuation token to fetch the next page of items. This is <see langword="null"/> if there are no more pages to fetch.</param>
public class Page<T>(
    IReadOnlyList<T> items,
    string? continuationToken)
{
    /// <summary>
    /// The list containing the items.
    /// </summary>
    public IReadOnlyList<T> Items { get; } = items;

    /// <summary>
    /// The continuation token to fetch the next page of items.
    /// </summary>
    /// <remarks>
    /// This is <see langword="null"/> if there are no more pages to fetch.
    /// </remarks>
    public string? ContinuationToken { get; } = continuationToken;
}