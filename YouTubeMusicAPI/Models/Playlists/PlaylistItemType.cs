namespace YouTubeMusicAPI.Models.Playlists;

/// <summary>
/// Represents the type of a playlist item on YouTube Music.
/// </summary>
public enum PlaylistItemType
{
    /// <summary>
    /// The item is a song.
    /// </summary>
    Song,

    /// <summary>
    /// The item is a video.
    /// </summary>
    Video,

    /// <summary>
    /// The item is unavailable.
    /// </summary>
    Unavailable
}