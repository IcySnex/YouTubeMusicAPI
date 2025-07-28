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
    Radio? radio) : SearchResult(name, id, null, thumbnails)
{
    /// <summary>
    /// Parses the JSON element into a <see cref="VideoSearchResult"/>.
    /// </summary>
    /// <param name="item">The JSON item "musicResponsiveListItemRenderer".</param>
    internal static VideoSearchResult Parse(
        JsonElement item)
    {
        JsonElement flexColumns = item
            .GetProperty("flexColumns");

        JsonElement descriptionRuns = flexColumns
            .GetElementAt(1)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs");

        int descriptionStartIndex = descriptionRuns
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow()
            .If("Video", 2, 0);


        string name = flexColumns
            .GetElementAt(0)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs")
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = item
            .GetProperty("overlay")
            .SelectOverlayNavigationVideoId();

        Thumbnail[] thumbnails = item
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists(descriptionStartIndex);

        TimeSpan duration = descriptionRuns
            .GetElementAt(descriptionStartIndex + artists.Length * 2 + 2)
            .GetProperty("text")
            .GetString()
            .ToTimeSpan()
            .OrThrow();

        string viewsInfo = descriptionRuns
            .GetElementAt(descriptionStartIndex + artists.Length * 2)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        Radio? radio = item
            .SelectMenuItems()
            .SelectRadioOrNull();

        return new(name, id, thumbnails, artists, duration, viewsInfo, radio);
    }

    /// <summary>
    /// Parses the JSON item into an <see cref="VideoSearchResult"/>.
    /// </summary>
    /// <param name="item">The JSON item "musicCardShelfRenderer".</param>
    internal static VideoSearchResult ParseTopResult(
        JsonElement item)
    {
        JsonElement descriptionRuns = item
            .GetProperty("subtitle")
            .GetProperty("runs");


        string name = item
            .GetProperty("title")
            .GetProperty("runs")
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = item
            .GetProperty("thumbnailOverlay")
            .SelectOverlayNavigationVideoId();

        Thumbnail[] thumbnails = item
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists(2);

        TimeSpan duration = descriptionRuns
            .GetElementAt(artists.Length * 2 + 4)
            .GetProperty("text")
            .GetString()
            .ToTimeSpan()
            .OrThrow();

        string viewsInfo = descriptionRuns
            .GetElementAt(artists.Length * 2 + 2)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        Radio? radio = item
            .SelectMenuItems()
            .SelectRadioOrNull();

        return new(name, id, thumbnails, artists, duration, viewsInfo, radio);
    }

    /// <summary>
    /// Parses the JSON element into a <see cref="VideoSearchResult"/>.
    /// </summary>
    /// <param name="item">The JSON item "musicResponsiveListItemRenderer".</param>
    internal static VideoSearchResult ParseSuggestion(
        JsonElement item)
    {
        JsonElement flexColumns = item
            .GetProperty("flexColumns");

        JsonElement descriptionRuns = flexColumns
            .GetElementAt(1)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs");


        string name = flexColumns
            .GetElementAt(0)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs")
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = item
            .GetProperty("overlay")
            .SelectOverlayNavigationVideoId();

        Thumbnail[] thumbnails = item
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        bool hasKnownArtist = descriptionRuns
            .GetArrayLength()
            .If(3, false, true);

        YouTubeMusicEntity[] artists = hasKnownArtist
            ? descriptionRuns
                .SelectArtists(2)
            : [new("N/A", null, null)]; // fr im gonna crash out bruh, why WHY DO YOU SOMETIMES NOT RETURN AN ARTIST??

        TimeSpan duration = TimeSpan.Zero; // grrrrr YT, why DO YOU NOT PROVIDE A DURATION GAWD DAMN??=?=!" i will not make this nullable just because of this bullshit!!!!

        string viewsInfo = descriptionRuns
            .GetElementAt(artists.Length * 2 + (hasKnownArtist ? 2 : 0))
            .GetProperty("text")
            .GetString()
            .OrThrow();

        Radio? radio = item
            .SelectMenuItems()
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