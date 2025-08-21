using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a video search result on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="VideoSearchResult"/> class.
/// </remarks>
/// <param name="name">The name of this video.</param>
/// <param name="id">The ID of this video.</param>
/// <param name="thumbnails">The thumbnails of this video.</param>
/// <param name="artists">The artists of this video.</param>
/// <param name="duration">The duration of this video.</param>
/// <param name="viewsInfo">The information about the number of views this video has.</param>
/// <param name="radio">The radio associated with this video, if available.</param>
public class VideoSearchResult(
    string name,
    string id,
    Thumbnail[] thumbnails,
    YouTubeMusicEntity[] artists,
    TimeSpan duration,
    string viewsInfo,
    Radio? radio) : SearchResult(name, id, null, thumbnails)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="VideoSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static VideoSearchResult Parse(
        JElement element)
    {
        JElement flexColumns = element
            .Get("flexColumns");

        JElement descriptionRuns = flexColumns
            .GetAt(1)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .Get("text")
            .Get("runs");

        int descriptionStartIndex = descriptionRuns
            .GetAt(0)
            .Get("text")
            .AsString()
            .OrThrow()
            .If("Video", 2, 0);


        string name = flexColumns
            .GetAt(0)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .Get("text")
            .Get("runs")
            .GetAt(0)
            .Get("text")
            .AsString()
            .OrThrow();

        string id = element
            .Get("overlay")
            .SelectOverlayVideoId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists(descriptionStartIndex);

        TimeSpan duration = descriptionRuns
            .GetAt(descriptionStartIndex + artists.Length * 2 + 2)
            .Get("text")
            .AsString()
            .ToTimeSpan()
            .Or(TimeSpan.Zero);

        string viewsInfo = descriptionRuns
            .GetAt(descriptionStartIndex + artists.Length * 2)
            .Get("text")
            .AsString()
            .Or("N/A views");

        Radio? radio = element
            .SelectMenu()
            .SelectRadio();

        return new(name, id, thumbnails, artists, duration, viewsInfo, radio);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="VideoSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicCardShelfRenderer".</param>
    internal static VideoSearchResult ParseTopResult(
        JElement element)
    {
        JElement descriptionRuns = element
            .Get("subtitle")
            .Get("runs");


        string name = element
            .Get("title")
            .Get("runs")
            .GetAt(0)
            .Get("text")
            .AsString()
            .OrThrow();

        string id = element
            .Get("thumbnailOverlay")
            .SelectOverlayVideoId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists(2);

        TimeSpan duration = descriptionRuns
            .GetAt(artists.Length * 2 + 4)
            .Get("text")
            .AsString()
            .ToTimeSpan()
            .Or(TimeSpan.Zero);

        string viewsInfo = descriptionRuns
            .GetAt(artists.Length * 2 + 2)
            .Get("text")
            .AsString()
            .Or("N/A views");

        Radio? radio = element
            .SelectMenu()
            .SelectRadio();

        return new(name, id, thumbnails, artists, duration, viewsInfo, radio);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="VideoSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static VideoSearchResult ParseSuggestion(
        JElement element)
    {
        JElement flexColumns = element
            .Get("flexColumns");

        JElement descriptionRuns = flexColumns
            .GetAt(1)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .Get("text")
            .Get("runs");


        string name = flexColumns
            .GetAt(0)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .Get("text")
            .Get("runs")
            .GetAt(0)
            .Get("text")
            .AsString()
            .OrThrow();

        string id = element
            .Get("overlay")
            .SelectOverlayVideoId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        bool hasKnownArtist = descriptionRuns
            .ArrayLength
            .If(3, false, true);

        YouTubeMusicEntity[] artists = hasKnownArtist
            .If(true, // fr im gonna crash out bruh, why WHY DO YOU SOMETIMES NOT RETURN AN ARTIST??
                descriptionRuns
                    .SelectArtists(2),
                [new("N/A", null, null)]);

        TimeSpan duration = TimeSpan.Zero;

        string viewsInfo = descriptionRuns
            .GetAt(artists.Length * 2 + (hasKnownArtist ? 2 : 0))
            .Get("text")
            .AsString()
            .Or("N/A views");

        Radio? radio = element
            .SelectMenu()
            .SelectRadio();

        return new(name, id, thumbnails, artists, duration, viewsInfo, radio);
    }


    /// <summary>
    /// The artists of this video.
    /// </summary>
    public YouTubeMusicEntity[] Artists { get; } = artists;

    /// <summary>
    /// The duration of this video.
    /// </summary>
    /// <remarks>
    /// May be <see cref="TimeSpan.Zero"/> if the YouTube decides to play annoying games (in search suggestions and library searches)."/>
    /// </remarks>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// The information about the number of views this video has.
    /// </summary>
    public string ViewsInfo { get; } = viewsInfo;

    /// <summary>
    /// The radio associated with this video, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}