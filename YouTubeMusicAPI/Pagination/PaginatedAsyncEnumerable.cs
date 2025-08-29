namespace YouTubeMusicAPI.Pagination;

/// <summary>
/// Provides an asynchronous way to iterate through paginated items.
/// </summary>
/// <typeparam name="T">The type of the items.</typeparam>
public class PaginatedAsyncEnumerable<T> : IAsyncEnumerable<T>
{
    readonly FetchPageDelegate<T> fetchDelegate;
    readonly Stack<string?> previousContinuationTokens = new();

    string? nextContinuationToken = null;

    /// <summary>
    /// Creates a new instance of the <see cref="PaginatedAsyncEnumerable{T}"/> class.
    /// </summary>
    /// <param name="fetchDelegate">Delegate to fetch a page of items.</param>
    /// <param name="firstPage">The pre-fetched first page of items.</param>
    internal PaginatedAsyncEnumerable(
        FetchPageDelegate<T> fetchDelegate,
        Page<T>? firstPage = null)
    {
        this.fetchDelegate = fetchDelegate;

        if (firstPage is not null)
        {
            previousContinuationTokens.Push(null);

            CurrentPage = firstPage.Items;
            CurrentIndex = 0;

            nextContinuationToken = firstPage.ContinuationToken;
        }
    }


    /// <summary>
    /// The current page of items.
    /// </summary>
    public IReadOnlyList<T>? CurrentPage { get; private set; } = null;

    /// <summary>
    /// The current index of fetched pages.
    /// </summary>
    public int CurrentIndex { get; private set; } = -1;


    /// <summary>
    /// Whether there are more pages to fetch.
    /// </summary>
    public bool HasMore => CurrentIndex == -1 || nextContinuationToken is not null;

    /// <summary>
    /// Whether there are previous pages to fetch.
    /// </summary>
    public bool HasPrevious => CurrentIndex > 0;


    /// <summary>
    /// Fetches the next page of items.
    /// </summary>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>Whether the next page was fetched or skipped as <see cref="HasMore"/> is false.</returns>
    public async Task<bool> FetchNextPageAsync(
        CancellationToken cancellationToken = default)
    {
        if (!HasMore)
            return false;

        Page<T> page = await fetchDelegate(nextContinuationToken, cancellationToken);

        previousContinuationTokens.Push(nextContinuationToken);
        nextContinuationToken = page.ContinuationToken;

        CurrentPage = page.Items;
        CurrentIndex++;

        return true;
    }

    /// <summary>
    /// Fetches the previous page of items.
    /// </summary>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>Whether the next page was fetched or skipped as <see cref="HasPrevious"/> is false.</returns>
    public async Task<bool> FetchPreviousPageAsync(
        CancellationToken cancellationToken = default)
    {
        if (!HasPrevious)
            return false;

        previousContinuationTokens.Pop();
        string? previousContinuationToken = previousContinuationTokens.Peek();

        Page<T> page = await fetchDelegate(previousContinuationToken, cancellationToken);

        nextContinuationToken = page.ContinuationToken;

        CurrentPage = page.Items;
        CurrentIndex--;

        return true;
    }


    /// <summary>
    /// Resets the paginator to the first page.
    /// </summary>
    public void Reset()
    {
        nextContinuationToken = null;
        previousContinuationTokens.Clear();

        CurrentPage = null;
        CurrentIndex = -1;
    }

    /// <summary>
    /// Fetches the items of all pages in the specified range.
    /// </summary>
    /// <remarks>
    /// This may run indefinitely if <c>limit</c> is null.
    /// </remarks>
    /// <param name="offset">The number of items to skip. Pages will still be fetched but not added to the result.</param>
    /// <param name="limit">The maximum items to fetch. Leave this <see langword="null"/> to fetch all items.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>A list containing the items within the range.</returns>
    public async Task<IReadOnlyList<T>> FetchItemsAsync(
        int offset = 0,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        List<T> result = [];
        int skipped = 0;
        int taken = 0;

        bool Consume(IReadOnlyList<T> page)
        {
            foreach (T item in page)
            {
                if (skipped < offset)
                {
                    skipped++;
                    continue;
                }

                result.Add(item);
                taken++;

                if (limit is not null && taken >= limit.Value)
                    return true;
            }

            return false;
        }

        if (CurrentIndex != 0 || CurrentPage is null)
            Reset();
        else if (Consume(CurrentPage))
            return result;

        while (await FetchNextPageAsync(cancellationToken) && CurrentPage is not null)
            if (Consume(CurrentPage))
                return result;

        return result;
    }

    /// <summary>
    /// Returns an enumerator that iterates asynchronously through all items.
    /// </summary>
    /// <param name="cancellationToken">The token to cancel the asynchronous iteration.</param>
    /// <returns>An <see cref="IAsyncEnumerator{T}"/> to iterate asynchronously through all items.</returns>
    public async IAsyncEnumerator<T> GetAsyncEnumerator(
        CancellationToken cancellationToken = default)
    {
        if (CurrentIndex != 0 || CurrentPage is null)
            Reset();
        else
            foreach (T item in CurrentPage)
                yield return item;

        while (await FetchNextPageAsync(cancellationToken) && CurrentPage is not null)
            foreach (T item in CurrentPage)
                yield return item;
    }
}