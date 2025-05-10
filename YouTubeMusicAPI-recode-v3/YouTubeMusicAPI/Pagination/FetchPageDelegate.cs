namespace YouTubeMusicAPI.Pagination;

/// <summary>
/// Delegate to fetch a page of items.
/// </summary>
/// <typeparam name="T">The type of the items.</typeparam>
/// <param name="continuationToken">The continuation token to fetch the next page of the items. Leave this <see langword="null"/> to fetch the first page.</param>
/// <param name="cancellationToken">The token to cancel this task.</param>
/// <returns>A page containing the items and the next continuation token.</returns>
public delegate Task<Page<T>> FetchPageDelegate<T>(
    string? continuationToken,
    CancellationToken cancellationToken = default);