using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Lyrics;

/// <summary>
/// Represents lyrics for a media item on YouTube Music.
/// </summary>
/// <param name="source">The source of the lyrics from which they come.</param>
/// <param name="backgroundImage">The background image associated with the lyrics.</param>
public abstract class SongVideoLyrics(
    string source,
    string backgroundImage)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="SongVideoLyrics"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "lyricsData".</param>
    internal static SongVideoLyrics Parse(
        JElement element)
    {
        bool isPlain = element
            .Get("staticLayout")
            .AsBool()
            .Or(false);

        return isPlain ? PlainLyrics.Parse(element) : SyncedLyrics.Parse(element);
    }


    /// <summary>
    /// The source of the lyrics from which they come.
    /// </summary>
    public string Source { get; } = source;

    /// <summary>
    /// The background image associated with the lyrics.
    /// </summary>
    public string BackgroundImage { get; } = backgroundImage;
}