namespace YouTubeMusicAPI.Models.MediaItems;

/// <summary>
/// Represents lyrics for a media item on YouTube Music.
/// </summary>
/// <param name="source">The source of the lyrics from which they come.</param>
/// <param name="backgroundImage">The background image associated with the lyrics.</param>
public abstract class Lyrics(
    string source,
    string backgroundImage)
{
    /// <summary>
    /// The source of the lyrics from which they come.
    /// </summary>
    public string Source { get; } = source;

    /// <summary>
    /// The background image associated with the lyrics.
    /// </summary>
    public string BackgroundImage { get; } = backgroundImage;
}