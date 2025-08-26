namespace YouTubeMusicAPI.Services.Search;

/// <summary>
/// Represents the category of content to search for on YouTube Music.
/// </summary>
public enum SearchCategory
{
    /// <summary>
    /// Search for songs.
    /// </summary>
    Songs,

    /// <summary>
    /// Search for videos.
    /// </summary>
    Videos,

    /// <summary>
    /// Search for community-created playlists.
    /// </summary>
    CommunityPlaylists,

    /// <summary>
    /// Search for featured playlists created by YouTube Music.
    /// </summary>
    FeaturedPlaylists,

    /// <summary>
    /// Search for albums.
    /// </summary>
    Albums,

    /// <summary>
    /// Search for artists.
    /// </summary>
    Artists,

    /// <summary>
    /// Search for profiles.
    /// </summary>
    Profiles,

    /// <summary>
    /// Search for podcasts.
    /// </summary>
    Podcasts,

    /// <summary>
    /// Search for episodes.
    /// </summary>
    Episodes
}