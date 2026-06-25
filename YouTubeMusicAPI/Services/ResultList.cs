namespace YouTubeMusicAPI.Services;

/// <summary>
/// A wrapper for list results such as playlists, albums, singles, songs, etc
/// </summary>
/// <typeparam name="T">The result type</typeparam>
public class ResultList<T>
{
    /// <summary>
    /// The browse id
    /// </summary>
    public string? BrowseId { get; init; }
    /// <summary>
    /// The search params
    /// </summary>
    public string? Params { get; init; }
    /// <summary>
    /// The results, could be songs, albums, etc
    /// </summary>
    public IReadOnlyList<T> Results { get; init; } = [];
}