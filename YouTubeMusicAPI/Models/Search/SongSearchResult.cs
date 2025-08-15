using System.Text.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a song search result on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="SongSearchResult"/> class.
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
    /// Parses a <see cref="JsonElement"/> into a <see cref="SongSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> "musicResponsiveListItemRenderer".</param>
    internal static SongSearchResult Parse(
        JsonElement element)
    {
        JsonElement menuItems = element
            .SelectMenuItems();

        JsonElement flexColumns = element
            .GetProperty("flexColumns");

        JsonElement descriptionRuns = flexColumns
            .GetPropertyAt(1)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs");

        int descriptionStartIndex = descriptionRuns
            .GetPropertyAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow()
            .If("Song", 2, 0);


        string name = flexColumns
            .GetPropertyAt(0)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs")
            .GetPropertyAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = element
            .GetProperty("overlay")
            .SelectOverlayNavigationVideoId();

        Thumbnail[] thumbnails = element
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists(descriptionStartIndex);

        bool hasKnownAlbum = (descriptionRuns
            .GetPropertyAtOrNull(descriptionStartIndex + artists.Length * 2)
            ?.GetPropertyOrNull("navigationEndpoint"))
            .If(null, false, true);

        YouTubeMusicEntity album = hasKnownAlbum
            ? descriptionRuns
                .GetPropertyAt(descriptionStartIndex + artists.Length * 2)
                .SelectAlbum()
            : menuItems
                .SelectAlbumUnknown();

        TimeSpan duration = descriptionRuns
            .GetPropertyAt(descriptionStartIndex + artists.Length * 2 + (hasKnownAlbum ? 2 : 0))
            .GetProperty("text")
            .GetString()
            .ToTimeSpan()
            .OrThrow();

        bool isExplicit = element
            .GetPropertyOrNull("badges")
            .SelectContainsExplicitBadge();

        string playsInfo = flexColumns
            .GetPropertyAt(2)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs")
            .GetPropertyAt(0)
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
    /// Parses a <see cref="JsonElement"/> into a <see cref="SongSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> "musicCardShelfRenderer".</param>
    internal static SongSearchResult ParseTopResult(
        JsonElement element)
    {
        JsonElement menuItems = element
            .SelectMenuItems();

        JsonElement descriptionRuns = element
            .GetProperty("subtitle")
            .GetProperty("runs");


        string name = element
            .GetProperty("title")
            .GetProperty("runs")
            .GetPropertyAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = element
            .GetProperty("thumbnailOverlay")
            .SelectOverlayNavigationVideoId();

        Thumbnail[] thumbnails = element
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists(2);

        YouTubeMusicEntity album = menuItems
            .SelectAlbumUnknown();

        TimeSpan duration = descriptionRuns
            .GetPropertyAt(artists.Length * 2 + 2)
            .GetProperty("text")
            .GetString()
            .ToTimeSpan()
            .OrThrow();

        bool isExplicit = element
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
    /// Parses a <see cref="JsonElement"/> into a <see cref="SongSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> "musicResponsiveListItemRenderer".</param>
    internal static SongSearchResult ParseSuggestion(
        JsonElement element)
    {
        JsonElement menuItems = element
            .SelectMenuItems();

        JsonElement flexColumns = element
            .GetProperty("flexColumns");

        JsonElement descriptionRuns = flexColumns
            .GetPropertyAt(1)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs");

        JsonElement? albumRun = flexColumns
            .GetPropertyAtOrNull(2)
            ?.GetPropertyOrNull("musicResponsiveListItemFlexColumnRenderer")
            ?.GetPropertyOrNull("text")
            ?.GetPropertyOrNull("runs")
            ?.GetPropertyAtOrNull(0);


        string name = flexColumns
            .GetPropertyAt(0)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs")
            .GetPropertyAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = element
            .GetProperty("overlay")
            .SelectOverlayNavigationVideoId();

        Thumbnail[] thumbnails = element
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

        bool isExplicit = element
            .GetPropertyOrNull("badges")
            .SelectContainsExplicitBadge();

        string playsInfo = descriptionRuns
            .GetPropertyAt(artists.Length * 2 + 2)
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