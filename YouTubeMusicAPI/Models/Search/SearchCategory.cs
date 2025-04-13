namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents the category of a YouTube Music search result
/// </summary>
public enum SearchCategory
{
    /// <summary>
    /// Search result is a song
    /// </summary>
    Songs,
    /// <summary>
    /// Search result is a video
    /// </summary>
    Videos,
    /// <summary>
    /// Search result is an album
    /// </summary>
    Albums,
    /// <summary>
    /// Search result is a community playlist
    /// </summary>
    CommunityPlaylists,
    /// <summary>
    /// Search result is an artist
    /// </summary>
    Artists,
    /// <summary>
    /// Search result is an podcast
    /// </summary>
    Podcasts,
    /// <summary>
    /// Search result is a podcast episode
    /// </summary>
    Episodes,
    /// <summary>
    /// Search result is a profile
    /// </summary>
    Profiles
}