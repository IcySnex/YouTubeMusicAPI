namespace YouTubeMusicAPI.Common;

/// <summary>
/// Provides an asynchronous way to iterate through paginated results
/// </summary>
/// <typeparam name="T">The type of items</typeparam>
/// <param name="fetchDelegate">The delegate for fetching a page</param>
public class PaginatedAsyncEnumerable<T>(
    FetchPageDelegate<T> fetchDelegate) : IAsyncEnumerable<T>
{
    readonly FetchPageDelegate<T> fetchDelegate = fetchDelegate;
    string? continuationToken = null;


    /// <summary>
    /// Weither there are more pages to fetch
    /// </summary>
    public bool HasMore { get; private set; } = true;


    /// <summary>
    /// Fetches the next page of results
    /// </summary>
    /// <param name="cancellationToken">The token to cancel this action</param>
    /// <returns>A list containing the items of the next page</returns>
    public async Task<IReadOnlyList<T>> FetchNextPageAsync(
        CancellationToken cancellationToken = default)
    {
        if (!HasMore)
            return [];

        Page<T> page = await fetchDelegate(continuationToken, cancellationToken);

        continuationToken = page.ContinuationToken;
        HasMore = !string.IsNullOrEmpty(continuationToken);

        return page.Items;
    }

    /// <summary>
    /// Fetches the items of all pages in the specified range
    /// </summary>
    /// <param name="offset">The number of items to skip (pages will be fetched but not added to the result)</param>
    /// <param name="limit">The maximum items to fetch (null to get all)</param>
    /// <param name="cancellationToken">The token to cancel this action</param>
    /// <returns>A list of items within the range</returns>
    public async Task<IReadOnlyList<T>> FetchItemsAsync(
        int offset = 0,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        Reset();

        List<T> allItems = [];
        while (HasMore && !cancellationToken.IsCancellationRequested)
        {
            IReadOnlyList<T> items = await FetchNextPageAsync(cancellationToken);
            allItems.AddRange(items);

            if (limit.HasValue && allItems.Count >= limit.Value + offset)
                return [.. allItems.Skip(offset).Take(limit.Value)];
        }

        return [.. allItems.Skip(offset)];
    }


    /// <summary>
    /// Resets the paginator to the first page
    /// </summary>
    public void Reset()
    {
        continuationToken = null;
        HasMore = true;
    }


    /// <summary>
    /// Returns an enumerator that iterates asynchronously through the collection
    /// </summary>
    /// <param name="cancellationToken">A System.Threading.CancellationToken that may be used to cancel the asynchronous iteration</param>
    /// <returns>An enumerator that can be used to iterate asynchronously through the collection</returns>
    public async IAsyncEnumerator<T> GetAsyncEnumerator(
        CancellationToken cancellationToken = default)
    {
        Reset();

        while (HasMore && !cancellationToken.IsCancellationRequested)
        {
            IReadOnlyList<T> items = await FetchNextPageAsync(cancellationToken);

            foreach (T item in items)
                yield return item;
        }
    }
}