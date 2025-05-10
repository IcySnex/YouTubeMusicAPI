namespace YouTubeMusicAPI.Pagination;

/// <summary>
/// Provides an asynchronous way to iterate through paginated items.
/// </summary>
/// <typeparam name="T">The type of the items.</typeparam>
/// <param name="fetchDelegate">Delegate to fetch a page of items.</param>
public class PaginatedAsyncEnumerable<T>(
    FetchPageDelegate<T> fetchDelegate) : IAsyncEnumerable<T>
{
    readonly FetchPageDelegate<T> fetchDelegate = fetchDelegate;
    readonly Stack<string?> previousContinuationTokens = new();
    string? nextContinuationToken = null;


    /// <summary>
    /// Weither there are previous pages to fetch.
    /// </summary>
    public bool HasPrevious => previousContinuationTokens.Count > 1;

    /// <summary>
    /// Weither there are more pages to fetch.
    /// </summary>
    public bool HasMore { get; private set; } = true;


    /// <summary>
    /// Fetches the previous page of items.
    /// </summary>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>A list containing the items.</returns>
    public async Task<IReadOnlyList<T>> FetchPreviousPageAsync(
        CancellationToken cancellationToken = default)
    {
        if (!HasPrevious)
            return [];

        previousContinuationTokens.Pop();
        string? previousContinuationToken = previousContinuationTokens.Pop();

        Page<T> page = await fetchDelegate(previousContinuationToken, cancellationToken);

        nextContinuationToken = page.ContinuationToken;
        HasMore = nextContinuationToken is not null;

        return page.Items;
    }

    /// <summary>
    /// Fetches the next page of items.
    /// </summary>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>A list containing the items.</returns>
    public async Task<IReadOnlyList<T>> FetchNextPageAsync(
        CancellationToken cancellationToken = default)
    {
        if (!HasMore)
            return [];

        Page<T> page = await fetchDelegate(nextContinuationToken, cancellationToken);

        previousContinuationTokens.Push(nextContinuationToken);

        nextContinuationToken = page.ContinuationToken;
        HasMore = nextContinuationToken is not null;

        return page.Items;
    }

    /// <summary>
    /// Fetches the items of all pages in the specified range.
    /// </summary>
    /// <param name="offset">The number of items to skip. Pages will still be fetched but not added to the result.</param>
    /// <param name="limit">The maximum items to fetch. Leave this <see langword="null"/> to fetch all items - may run indefinitely.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>A list containing the items within the range.</returns>
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
    /// Resets the paginator to the first page.
    /// </summary>
    public void Reset()
    {
        nextContinuationToken = null;
        HasMore = true;
    }


    /// <summary>
    /// Returns an enumerator that iterates asynchronously through the items.
    /// </summary>
    /// <param name="cancellationToken">The token to cancel the asynchronous iteration.</param>
    /// <returns>An enumerator that can be used to iterate asynchronously through the collection.</returns>
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