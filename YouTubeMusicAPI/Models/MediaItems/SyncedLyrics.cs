using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.MediaItems;

/// <summary>
/// Represents lyrics with synchronized timing information for a media item on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="SyncedLyrics"/> class.
/// </remarks>
/// <param name="source">The source of the lyrics from which they come.</param>
/// <param name="backgroundImage">The background image associated with the lyrics.</param>
/// <param name="lines">The list of synchronized lines in the lyrics.</param>
public class SyncedLyrics(
    string source,
    string backgroundImage,
    IReadOnlyList<SyncedLyrics.Line> lines) : Lyrics(source, backgroundImage)
{
    /// <summary>
    /// Represents a single line of synchronized lyrics.
    /// </summary>
    /// <remarks>
    /// Creates a new instance of the <see cref="Line"/> class.
    /// </remarks>
    /// <param name="text">The text of the line.</param>
    /// <param name="startsAt">The start time of the line.</param>
    /// <param name="endsAt">The end time of the line.</param>
    public class Line(
        string text,
        TimeSpan startsAt,
        TimeSpan endsAt)
    {
        /// <summary>
        /// Parses a <see cref="JElement"/> into a <see cref="Line"/>.
        /// </summary>
        /// <param name="element">The <see cref="JElement"/> "timedLyricsData".</param>
        internal static Line Parse(
            JElement element)
        {
            JElement cueRange = element
                .Get("cueRange");


            string text = element
                .Get("lyricLine")
                .AsString()
                .OrThrow();

            TimeSpan startsAt = cueRange
                .Get("startTimeMilliseconds")
                .AsString()
                .ToInt64()
                .ToTimeSpan()
                .OrThrow();

            TimeSpan endsAt = cueRange
                .Get("endTimeMilliseconds")
                .AsString()
                .ToInt64()
                .ToTimeSpan()
                .OrThrow();

            return new(text, startsAt, endsAt);
        }


        /// <summary>
        /// The text of the line.
        /// </summary>
        public string Text { get; } = text;

        /// <summary>
        /// The start time of the line.
        /// </summary>
        public TimeSpan StartsAt { get; } = startsAt;

        /// <summary>
        /// The end time of the line.
        /// </summary>
        public TimeSpan EndsAt { get; } = endsAt;
    }


    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="SyncedLyrics"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "lyricsData".</param>
    internal static SyncedLyrics Parse(
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

        List<Line> lines = element
            .Get("timedLyricsData")
            .AsArray()
            .OrThrow()
            .Select(Line.Parse)
            .ToList();

        return new(source, backgroundImage, lines);
    }


    /// <summary>
    /// The list of synchronized lines in the lyrics.
    /// </summary>
    public IReadOnlyList<Line> Lines { get; } = lines;
}