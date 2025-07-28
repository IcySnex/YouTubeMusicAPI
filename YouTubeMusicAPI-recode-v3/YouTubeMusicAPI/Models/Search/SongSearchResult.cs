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
/// <param name="isCreditsAvailable">Whether credits are available to fetch for this song.</param>
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
    bool isCreditsAvailable,
    Radio? radio) : SearchResult(name, id, null, thumbnails)
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

        JsonElement menuItems = item
            .SelectMenuItems();

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
            .If("Song", 2, 0);


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

        bool hasKnownAlbum = (descriptionRuns
            .GetElementAtOrNull(descriptionStartIndex + artists.Length * 2)
            ?.GetPropertyOrNull("navigationEndpoint"))
            .If(null, false, true);

        YouTubeMusicEntity album = hasKnownAlbum
            ? descriptionRuns
                .GetElementAt(descriptionStartIndex + artists.Length * 2)
                .SelectAlbum()
            : menuItems
                .SelectAlbumUnknown();

        TimeSpan duration = descriptionRuns
            .GetElementAt(descriptionStartIndex + artists.Length * 2 + (hasKnownAlbum ? 2 : 0))
            .GetProperty("text")
            .GetString()
            .ToTimeSpan()
            .OrThrow();

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
            .GetString()
            .OrThrow();

        bool isCreditsAvailable = menuItems
            .SelectIsCreditsAvailable();

        Radio? radio = menuItems
            .SelectRadioOrNull();

        return new(name, id, thumbnails, artists, album, duration, isExplicit, playsInfo, isCreditsAvailable, radio);
    }

    /// <summary>
    /// Parses the JSON item into an <see cref="SongSearchResult"/>.
    /// </summary>
    /// <param name="item">The JSON item "musicCardShelfRenderer".</param>
    internal static SongSearchResult ParseTopResult(
        JsonElement item)
    {
        JsonElement menuItems = item
            .SelectMenuItems();

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

        YouTubeMusicEntity album = menuItems
            .SelectAlbumUnknown();

        TimeSpan duration = descriptionRuns
            .GetElementAt(artists.Length * 2 + 2)
            .GetProperty("text")
            .GetString()
            .ToTimeSpan()
            .OrThrow();

        bool isExplicit = item
            .GetPropertyOrNull("subtitleBadges")
            .SelectContainsExplicitBadge();

        string playsInfo = "N/A plays";

        bool isCreditsAvailable = menuItems
            .SelectIsCreditsAvailable();

        Radio? radio = menuItems
            .SelectRadioOrNull();

        return new(name, id, thumbnails, artists, album, duration, isExplicit, playsInfo, isCreditsAvailable, radio);
    }

    /// <summary>
    /// Parses the JSON element into a <see cref="SongSearchResult"/>.
    /// </summary>
    /// <param name="item">The JSON item "musicResponsiveListItemRenderer".</param>
    internal static SongSearchResult ParseSuggestion(
        JsonElement item)
    {
        JsonElement menuItems = item
            .SelectMenuItems();

        JsonElement flexColumns = item
            .GetProperty("flexColumns");

        JsonElement descriptionRuns = flexColumns
            .GetElementAt(1)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs");

        JsonElement? albumRun = flexColumns
            .GetElementAtOrNull(2)
            ?.GetPropertyOrNull("musicResponsiveListItemFlexColumnRenderer")
            ?.GetPropertyOrNull("text")
            ?.GetPropertyOrNull("runs")
            ?.GetElementAtOrNull(0);


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
            .SelectArtists(2);

        bool hasKnownAlbum = (albumRun
            ?.GetPropertyOrNull("navigationEndpoint"))
            .If(null, false, true);

        YouTubeMusicEntity album = hasKnownAlbum
            ? albumRun!.Value
                .SelectAlbum()
            : menuItems
                .SelectAlbumUnknown();

        TimeSpan duration = TimeSpan.Zero; // grrrrr YT, why DO YOU NOT PROVIDE A DURATION GAWD DAMN??=?=!" i will not make this nullabel just because of this bullshit!!!!

        bool isExplicit = item
            .GetPropertyOrNull("badges")
            .SelectContainsExplicitBadge();

        string playsInfo = descriptionRuns
            .GetElementAt(artists.Length * 2 + 2)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        bool isCreditsAvailable = menuItems
            .SelectIsCreditsAvailable();

        Radio? radio = menuItems
            .SelectRadioOrNull();

        return new(name, id, thumbnails, artists, album, duration, isExplicit, playsInfo, isCreditsAvailable, radio);
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
    /// Whether credits are available to fetch for this song.
    /// </summary>
    public bool IsCreditsAvailable { get; } = isCreditsAvailable;

    /// <summary>
    /// The radio associated with this song, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}