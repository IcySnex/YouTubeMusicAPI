namespace YouTubeMusicAPI.Types;

/// <summary>
/// Represents the kind of a YouTube Music item
/// </summary>
public enum YouTubeMusicItemKind
{
    /// <summary>
    /// Kind is unknown
    /// </summary>
    Unknown,
    /// <summary>
    /// Kind is a song
    /// </summary>
    Songs,
    /// <summary>
    /// Kind is a video
    /// </summary>
    Videos,
    /// <summary>
    /// Kind is an album
    /// </summary>
    Albums,
    /// <summary>
    /// Kind is a community playlist
    /// </summary>
    CommunityPlaylists,
    /// <summary>
    /// Kind is an artist
    /// </summary>
    Artists,
    /// <summary>
    /// Kind is an podcast
    /// </summary>
    Podcasts,
    /// <summary>
    /// Kind is a podcast episode
    /// </summary>
    Episodes,
    /// <summary>
    /// Kind is a profile
    /// </summary>
    Profiles
}