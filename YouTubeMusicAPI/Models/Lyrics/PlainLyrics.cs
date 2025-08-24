using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Lyrics;

/// <summary>
/// Represents non-synced lyrics for a media item on YouTube Music.
/// </summary>
/// <param name="source">The source of the lyrics from which they come.</param>
/// <param name="backgroundImage">The background image associated with the lyrics.</param>
/// <param name="text">The full lyrics text.</param>
/// <remarks>
/// Creates a new instance of the <see cref="PlainLyrics"/> class.
/// </remarks>
public class PlainLyrics(
    string source,
    string backgroundImage,
    string text) : Lyrics(source, backgroundImage)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="PlainLyrics"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "lyricsData".</param>
    internal new static PlainLyrics Parse(
        JElement element)
    {
        string source = element
            .Get("sourceMessage")
            .AsString()
            .OrThrow()
            .Replace("Source: ", "");

        string backgroundImage = element
            .Get("backgroundImage")
            .Get("sources")
            .GetAt(0)
            .Get("url")
            .AsString()
            .OrThrow();

        string text = element
            .Get("timedLyricsData")
            .AsArray()
            .OrThrow()
            .Select(line => line
                .Get("lyricLine")
                .AsString()
                .OrThrow())
            .Join("\n");

        return new(source, backgroundImage, text);
    }


    /// <summary>
    /// The full lyrics text.
    /// </summary>
    public string Text { get; } = text;
}