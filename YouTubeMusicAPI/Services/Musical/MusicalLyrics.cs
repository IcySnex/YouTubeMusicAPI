using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Musical;

/// <summary>
/// Represents lyrics for a song/video on YouTube Music.
/// </summary>
/// <param name="source">The source of the lyrics from which they come.</param>
/// <param name="backgroundImage">The background image associated with the lyrics.</param>
public abstract class MusicalLyrics(
    string source,
    string backgroundImage)
{
    /// <summary>
    /// Represents non-synced lyrics for a song/video on YouTube Music.
    /// </summary>
    /// <param name="source">The source of the lyrics from which they come.</param>
    /// <param name="backgroundImage">The background image associated with the lyrics.</param>
    /// <param name="text">The full lyrics text.</param>
    /// <remarks>
    /// Creates a new instance of the <see cref="Plain"/> class.
    /// </remarks>
    public class Plain(
        string source,
        string backgroundImage,
        string text) : MusicalLyrics(source, backgroundImage)
    {
        /// <summary>
        /// Parses a <see cref="JElement"/> into a <see cref="Plain"/>.
        /// </summary>
        /// <param name="element">The <see cref="JElement"/> "lyricsData".</param>
        internal static new Plain Parse(
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

    /// <summary>
    /// Represents lyrics with synchronized timing information for a song/video on YouTube Music.
    /// </summary>
    /// <remarks>
    /// Creates a new instance of the <see cref="Synced"/> class.
    /// </remarks>
    /// <param name="source">The source of the lyrics from which they come.</param>
    /// <param name="backgroundImage">The background image associated with the lyrics.</param>
    /// <param name="lines">The list of synchronized lines in the lyrics.</param>
    public class Synced(
        string source,
        string backgroundImage,
        IReadOnlyList<Synced.Line> lines) : MusicalLyrics(source, backgroundImage)
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
        /// Parses a <see cref="JElement"/> into a <see cref="Synced"/>.
        /// </summary>
        /// <param name="element">The <see cref="JElement"/> "lyricsData".</param>
        internal static new Synced Parse(
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


    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="MusicalLyrics"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "lyricsData".</param>
    internal static MusicalLyrics Parse(
        JElement element)
    {
        bool isPlain = element
            .Get("staticLayout")
            .AsBool()
            .Or(false);

        return isPlain ? Plain.Parse(element) : Synced.Parse(element);
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