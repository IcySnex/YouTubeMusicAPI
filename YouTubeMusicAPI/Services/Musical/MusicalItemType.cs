namespace YouTubeMusicAPI.Services.Musical;

/// <summary>
/// Represents the type of a musical item on YouTube Music.
/// </summary>
public enum MusicalItemType
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