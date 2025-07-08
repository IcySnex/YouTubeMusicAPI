using System.Text.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a song search result on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="SongSearchResult"/>.
/// </remarks>
/// <param name="name">The name of this song.</param>
/// <param name="id">The ID of this song.</param>
/// <param name="thumbnails">The thumbnails of this song.</param>
/// <param name="artists">The artists of this song.</param>
/// <param name="album">The album of this song.</param>
/// <param name="duration">The duration of this song.</param>
/// <param name="isExplicit">Whether this song is explicit or not.</param>
/// <param name="playsInfo">The information about the numbers of plays this song has.</param>
/// <param name="radio">The radio associated with this song, if available.</param>
public class SongSearchResult(
    string name,
    string id,
    Thumbnail[] thumbnails,
    YouTubeMusicEntity[] artists,
    YouTubeMusicEntity album,
    TimeSpan duration,
    bool isExplicit,
    string playsInfo,
    Radio? radio) : SearchResult(name, id, thumbnails)
{
    /// <summary>
    /// Parses the JSON element into a <see cref="SongSearchResult"/>.
    /// </summary>
    /// <param name="element">The JSON element containing "musicResponsiveListItemRenderer".</param>
    internal static SongSearchResult Parse(
        JsonElement element)
    {
        JsonElement item = element
            .GetProperty("musicResponsiveListItemRenderer");

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
            .GetStringOrEmpty();

        string id = item
            .SelectOverlayNavigationVideoId();

        Thumbnail[] thumbnails = item
            .GetProperty("thumbnail")
            .SelectThumbnails();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists();

        YouTubeMusicEntity album = descriptionRuns
            .GetElementAt(artists.Length * 2)
            .SelectYouTubeMusicEntity();

        TimeSpan duration = descriptionRuns
            .GetElementAt(artists.Length * 2 + 2)
            .GetProperty("text")
            .GetStringOrEmpty()
            .ToTimeSpan();

        bool isExplicit = item
            .GetPropertyOrNull("badges")
            .SelectContainsExplicitBadge();

        string playsInfo = flexColumns
            .GetElementAt(2)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs")
            .GetElementAt(0)
            .GetProperty("text")
            .GetStringOrEmpty();

        Radio? radio = item
            .GetProperty("menu")
            .GetProperty("menuRenderer")
            .GetProperty("items")
            .SelectRadioOrNull();

        return new(name, id, thumbnails, artists, album, duration, isExplicit, playsInfo, radio);
    }


    /// <summary>
    /// The artists of this song.
    /// </summary>
    public YouTubeMusicEntity[] Artists { get; } = artists;

    /// <summary>
    /// The album of this song.
    /// </summary>
    public YouTubeMusicEntity Album { get; } = album;

    /// <summary>
    /// The duration of this song.
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// Whether this song is explicit or not.
    /// </summary>
    public bool IsExplicit { get; } = isExplicit;

    /// <summary>
    /// The information about the numbers of plays this song has.
    /// </summary>
    public string PlaysInfo { get; } = playsInfo;

    /// <summary>
    /// The radio associated with this song, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}