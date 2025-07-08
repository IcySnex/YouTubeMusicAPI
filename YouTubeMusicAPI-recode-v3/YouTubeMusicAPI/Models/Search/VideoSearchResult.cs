using System.Text.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a video search result on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="VideoSearchResult"/>.
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
    Radio? radio) : SearchResult(name, id, thumbnails)
{
    /// <summary>
    /// Parses the JSON element into a <see cref="VideoSearchResult"/>.
    /// </summary>
    /// <param name="element">The JSON element containing "musicResponsiveListItemRenderer".</param>
    internal static VideoSearchResult Parse(
        JsonElement element)
    {
        JsonElement item = element
            .GetProperty("musicResponsiveListItemRenderer");

        JsonElement flexColumns = item
            .GetProperty("flexColumns");

        JsonElement titleRun = flexColumns
            .GetElementAt(0)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs")
            .GetElementAt(0);

        JsonElement descriptionRuns = flexColumns
            .GetElementAt(1)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs");


        string name = titleRun
            .GetProperty("text")
            .GetStringOrEmpty();

        string id = titleRun
            .SelectNavigationWatchId();

        Thumbnail[] thumbnails = item
            .GetProperty("thumbnail")
            .SelectThumbnails();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists();

        TimeSpan duration = descriptionRuns
            .GetElementAt(artists.Length * 2 + 2)
            .GetProperty("text")
            .GetStringOrEmpty()
            .ToTimeSpan();

        string viewsInfo = descriptionRuns
            .GetElementAt(artists.Length * 2)
            .GetProperty("text")
            .GetStringOrEmpty();

        Radio? radio = item
            .GetProperty("menu")
            .GetProperty("menuRenderer")
            .GetProperty("items")
            .SelectRadioOrNull();

        return new(name, id, thumbnails, artists, duration, viewsInfo, radio);
    }


    /// <summary>
    /// The artists of this video.
    /// </summary>
    public YouTubeMusicEntity[] Artists { get; } = artists;

    /// <summary>
    /// The duration of this video.
    /// </summary>
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