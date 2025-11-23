using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Musical;

/// <summary>
/// Represents a musical item of a playlist on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="PlaylistMusicalItem"/> class.
/// </remarks>
/// <param name="name">The name of this playlist musical item.</param>
/// <param name="id">The ID of this playlist musical item.</param>
/// <param name="thumbnails">The thumbnails of this playlist musical item.</param>
/// <param name="artists">The artists of this playlist musical item.</param>
/// <param name="album">The album of this playlist musical item.</param>
/// <param name="duration">The duration of this playlist musical item.</param>
/// <param name="isExplicit">Whether this playlist musical item is explicit or not.</param>
/// <param name="isCreditsAvailable">Whether credits are available to fetch for this playlist musical item.</param>
/// <param name="type">The type of this playlist musical item.</param>
/// <param name="radio">The radio associated with this playlist musical item, if available.</param>
public class PlaylistMusicalItem(
    string name,
    string? id,
    Thumbnail[] thumbnails,
    YouTubeMusicEntity[] artists,
    YouTubeMusicEntity? album,
    TimeSpan duration,
    bool isExplicit,
    bool isCreditsAvailable,
    MusicalItemType type,
    Radio? radio) : YouTubeMusicEntity(name, id, null)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="PlaylistMusicalItem"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static PlaylistMusicalItem Parse(
        JElement element)
    {
        JElement menu = element
            .SelectMenu();

        JElement flexColumns = element
            .Get("flexColumns");

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

        string? id = element
            .Get("overlay")
            .SelectOverlayVideoId();

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

        YouTubeMusicEntity? album = albumRun
            .SelectNavigationBrowseId()
            .IsNotNull(out string? albumBrowseId)
                ? new YouTubeMusicEntity(
                    albumRun
                        .Get("text")
                        .AsString()
                        .OrThrow(),
                    null,
                    albumBrowseId)
                : null; // ahh my beloved fluent syntax.. :(

        TimeSpan duration = element
            .Get("fixedColumns")
            .GetAt(0)
            .Get("musicResponsiveListItemFixedColumnRenderer")
            .SelectRunTextAt("text", 0)
            .ToTimeSpan()
            .Or(TimeSpan.Zero);

        bool isExplicit = element
            .SelectIsExplicit();

        bool isCreditsAvailable = menu
            .SelectIsCreditsAvailable();

        MusicalItemType type = menu
            .SelectPlaylistItemType();

        Radio? radio = menu
            .SelectRadio();

        return new(name, id, thumbnails, artists, album, duration, isExplicit, isCreditsAvailable, type, radio);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="PlaylistMusicalItem"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "playlistPanelVideoRenderer".</param>
    internal static PlaylistMusicalItem ParseRadio(
        JElement element)
    {
        JElement menu = element
            .SelectMenu();

        JElement descriptionRuns = element
            .Get("longBylineText")
            .Get("runs");


        string name = element
            .SelectRunTextAt("title", 0)
            .OrThrow();

        string id = element
            .SelectNavigationVideoId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .SelectThumbnails();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists();

        YouTubeMusicEntity? album = descriptionRuns
            .GetAt(artists.Length * 2)
            .SelectNavigationBrowseId()
            .IsNotNull(out string? albumBrowseId)
                ? new YouTubeMusicEntity(
                    descriptionRuns
                        .GetAt(artists.Length * 2)
                        .Get("text")
                        .AsString()
                        .OrThrow(),
                    null,
                    albumBrowseId)
                : null; // ahh my beloved fluent syntax.. :(

        TimeSpan duration = element
            .SelectRunTextAt("lengthText", 0)
            .ToTimeSpan()
            .Or(TimeSpan.Zero);

        bool isExplicit = element
            .SelectIsExplicit();

        bool isCreditsAvailable = menu
            .SelectIsCreditsAvailable();

        MusicalItemType type = menu
            .SelectPlaylistItemType();

        Radio? radio = menu
            .SelectRadio();

        return new(name, id, thumbnails, artists, album, duration, isExplicit, isCreditsAvailable, type, radio);
    }


    /// <summary>
    /// The ID of this playlist musical item.
    /// </summary>
    /// <remarks>
    /// Only available if <see cref="Type"/> is <see cref="MusicalItemType.Song"/> or <see cref="MusicalItemType.Video"/>, else <see langword="null"/>.
    /// </remarks>
    public override string? Id { get; } = id;


    /// <summary>
    /// The thumbnails of this playlist musical item.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The artists of this playlist musical item.
    /// </summary>
    public YouTubeMusicEntity[] Artists { get; } = artists;

    /// <summary>
    /// The album of this playlist musical item.
    /// </summary>
    /// <remarks>
    /// Only available if <see cref="Type"/> is <see cref="MusicalItemType.Song"/>, else <see langword="null"/>.
    /// </remarks>
    public YouTubeMusicEntity? Album { get; } = album;

    /// <summary>
    /// The duration of this playlist musical item.
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// Whether this playlist musical item is explicit or not.
    /// </summary>
    public bool IsExplicit { get; } = isExplicit;

    /// <summary>
    /// Whether credits are available to fetch for this playlist musical item.
    /// </summary>
    public bool IsCreditsAvailable { get; } = isCreditsAvailable;

    /// <summary>
    /// The type of this playlist musical item.
    /// </summary>
    public MusicalItemType Type { get; } = type;

    /// <summary>
    /// The radio associated with this playlist musical item, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}