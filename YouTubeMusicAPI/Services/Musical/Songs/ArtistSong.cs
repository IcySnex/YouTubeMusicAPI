using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Musical.Songs;

/// <summary>
/// Represents an artist song on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="ArtistSong"/> class.
/// </remarks>
/// <param name="name">The name of this artist song.</param>
/// <param name="id">The ID of this artist song.</param>
/// <param name="thumbnails">The thumbnails of this artist song.</param>
/// <param name="artists">The artists of this artist song.</param>
/// <param name="album">The album of this artist song.</param>
/// <param name="isExplicit">Whether this artist song is explicit or not.</param>
/// <param name="playsInfo">The information about the numbers of plays this artist song has.</param>
/// <param name="isCreditsAvailable">Whether credits are available to fetch for this artist song.</param>
/// <param name="radio">The radio associated with this artist song, if available.</param>
public class ArtistSong(
    string name,
    string id,
    Thumbnail[] thumbnails,
    YouTubeMusicEntity[] artists,
    YouTubeMusicEntity album,
    bool isExplicit,
    string playsInfo,
    bool isCreditsAvailable,
    Radio? radio) : YouTubeMusicEntity(name, id, null)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="ArtistSong"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static ArtistSong Parse(
        JElement element)
    {
        JElement menu = element
            .SelectMenu();

        JElement flexColumns = element
            .Get("flexColumns");

        JElement albumRun = flexColumns
            .GetAt(3)
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

        YouTubeMusicEntity[] artists = flexColumns
            .GetAt(1)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .Get("text")
            .Get("runs")
            .SelectArtists();

        YouTubeMusicEntity album = albumRun
            .SelectNavigationBrowseId()
            .IsNotNull(out string? albumBrowseId)
                ? new YouTubeMusicEntity(
                    albumRun
                        .Get("text")
                        .AsString()
                        .OrThrow(),
                    null,
                    albumBrowseId)
                : menu
                    .SelectAlbumUnknown();

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

        return new(name, id, thumbnails, artists, album, isExplicit, playsInfo, isCreditsAvailable, radio);
    }


    /// <summary>
    /// The ID of this artist song.
    /// </summary>
    public override string Id { get; } = id;


    /// <summary>
    /// The thumbnails of this artist song.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The artists of this artist song.
    /// </summary>
    public YouTubeMusicEntity[] Artists { get; } = artists;

    /// <summary>
    /// The album of this artist song.
    /// </summary>
    public YouTubeMusicEntity Album { get; } = album;

    /// <summary>
    /// Whether this artist song is explicit or not.
    /// </summary>
    public bool IsExplicit { get; } = isExplicit;

    /// <summary>
    /// The information about the numbers of plays this artist song has.
    /// </summary>
    public string PlaysInfo { get; } = playsInfo;

    /// <summary>
    /// Whether credits are available to fetch for this artist song.
    /// </summary>
    public bool IsCreditsAvailable { get; } = isCreditsAvailable;

    /// <summary>
    /// The radio associated with this artist song, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}
