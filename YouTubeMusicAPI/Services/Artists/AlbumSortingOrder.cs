namespace YouTubeMusicAPI.Services.Artists;

/// <summary>
/// Represents the sorting order when browsing albums by artist on Youtube Music
/// </summary>
public enum AlbumSortingOrder
{
    /// <summary>
    /// Default order
    /// </summary>
    Default,
    /// <summary>
    /// Order by recency
    /// </summary>
    Recency,
    /// <summary>
    /// Order by popularity
    /// </summary>
    Popularity,
    /// <summary>
    /// Alphabetical ordering
    /// </summary>
    AlphabeticalOrder
}