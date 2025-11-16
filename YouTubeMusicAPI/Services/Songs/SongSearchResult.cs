using YouTubeMusicAPI.Common;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Services.Search;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Songs;

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
    /// Parses a <see cref="JElement"/> into a <see cref="SongSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static SongSearchResult Parse(
        JElement element)
    {
        JElement menu = element
            .SelectMenu();

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
            .Is("Song")
                ? 2
                : 0;


        string name = flexColumns
            .GetAt(0)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .SelectRunTextAt("text", 0)
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

        bool isAlbumUnknown = descriptionRuns
            .GetAt(descriptionStartIndex + artists.Length * 2)
            .Get("navigationEndpoint")
            .IsUndefined;
        YouTubeMusicEntity album = isAlbumUnknown
            ? menu
                .SelectAlbumUnknown()
            : descriptionRuns
                .GetAt(descriptionStartIndex + artists.Length * 2)
                .SelectAlbum();

        TimeSpan duration = descriptionRuns
            .GetAt(descriptionStartIndex + artists.Length * 2 + (isAlbumUnknown ? 0 : 2))
            .Get("text")
            .AsString()
            .ToTimeSpan()
            .Or(TimeSpan.Zero);

        bool isExplicit = element
            .SelectIsExplicit();

        string playsInfo = flexColumns
            .GetAt(2)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .SelectRunTextAt("text", 0)
            .Or("N/A plays");

        bool isCreditsAvailable = menu
            .SelectIsCreditsAvailable();

        Radio? radio = menu
            .SelectRadio();

        return new(name, id, thumbnails, artists, album, duration, isExplicit, playsInfo, isCreditsAvailable, radio);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="SongSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicCardShelfRenderer".</param>
    internal static SongSearchResult ParseTopResult(
        JElement element)
    {
        JElement menu = element
            .SelectMenu();

        JElement descriptionRuns = element
            .Get("subtitle")
            .Get("runs");


        string name = element
            .SelectRunTextAt("title", 0)
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

        YouTubeMusicEntity album = menu
            .SelectAlbumUnknown();

        TimeSpan duration = descriptionRuns
            .GetAt(artists.Length * 2 + 2)
            .Get("text")
            .AsString()
            .ToTimeSpan()
            .Or(TimeSpan.Zero);

        bool isExplicit = element
            .SelectIsExplicit("subtitleBadges");

        string playsInfo = "N/A plays";

        bool isCreditsAvailable = menu
            .SelectIsCreditsAvailable();

        Radio? radio = menu
            .SelectRadio();

        return new(name, id, thumbnails, artists, album, duration, isExplicit, playsInfo, isCreditsAvailable, radio);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="SongSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static SongSearchResult ParseSuggestion(
        JElement element)
    {
        JElement menu = element
            .SelectMenu();

        JElement flexColumns = element
            .Get("flexColumns");

        JElement descriptionRuns = flexColumns
            .GetAt(1)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .Get("text")
            .Get("runs");

        JElement albumRun = flexColumns
            .GetAt(2)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .Get("text")
            .Get("runs")
            .GetAt(0);


        string name = flexColumns
            .GetAt(0)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .SelectRunTextAt("text", 0)
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
            .SelectArtists(2);

        bool isAlbumUnknown = albumRun
            .Get("navigationEndpoint")
            .IsUndefined;
        YouTubeMusicEntity album = isAlbumUnknown
            ? menu
                .SelectAlbumUnknown()
            : albumRun
                .SelectAlbum();

        TimeSpan duration = TimeSpan.Zero;

        bool isExplicit = element
            .SelectIsExplicit();

        string playsInfo = descriptionRuns
            .GetAt(artists.Length * 2 + 2)
            .Get("text")
            .AsString()
            .Or("N/A plays");

        bool isCreditsAvailable = menu
            .SelectIsCreditsAvailable();

        Radio? radio = menu
            .SelectRadio();

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
    /// <remarks>
    /// May be <see cref="TimeSpan.Zero"/> if the YouTube decides to play annoying games (in search suggestions and library searches)."/>
    /// </remarks>
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