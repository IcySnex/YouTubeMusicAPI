namespace YouTubeMusicAPI.Common;

/// <summary>
/// Delegate for fetching a page of results using an optional continuation token
/// </summary>
/// <typeparam name="T">The type of items</typeparam>
/// <param name="continuationToken">The token to fetch the next page (null for the first page)</param>
/// <param name="cancellationToken">The token to cancel this action</param>
/// <returns>The page corresponding to the continuation token</returns>
public delegate Task<Page<T>> FetchPageDelegate<T>(
    string? continuationToken,
    CancellationToken cancellationToken = default);