using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Playlists;

/// <summary>
/// Represents an item of a playlist on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="PlaylistItem"/> class.
/// </remarks>
/// <param name="name">The name of this playlist item.</param>
/// <param name="id">The ID of this playlist item.</param>
/// <param name="thumbnails">The thumbnails of this playlist item.</param>
/// <param name="artists">The artists of this playlist item.</param>
/// <param name="album">The album of this playlist item.</param>
/// <param name="duration">The duration of this playlist item.</param>
/// <param name="isExplicit">Whether this playlist item is explicit or not.</param>
/// <param name="isCreditsAvailable">Whether credits are available to fetch for this playlist item.</param>
/// <param name="type">The type of this playlist item.</param>
/// <param name="radio">The radio associated with this playlist item, if available.</param>
public class PlaylistItem(
    string name,
    string? id,
    Thumbnail[] thumbnails,
    YouTubeMusicEntity[] artists,
    YouTubeMusicEntity? album,
    TimeSpan duration,
    bool isExplicit,
    bool isCreditsAvailable,
    PlaylistItemType type,
    Radio? radio) : YouTubeMusicEntity(name, id, null)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="PlaylistItem"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static PlaylistItem Parse(
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

        PlaylistItemType type = menu
            .SelectPlaylistItemType();

        Radio? radio = menu
            .SelectRadio();

        return new(name, id, thumbnails, artists, album, duration, isExplicit, isCreditsAvailable, type, radio);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="PlaylistItem"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "playlistPanelVideoRenderer".</param>
    internal static PlaylistItem ParseRadio(
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

        PlaylistItemType type = menu
            .SelectPlaylistItemType();

        Radio? radio = menu
            .SelectRadio();

        return new(name, id, thumbnails, artists, album, duration, isExplicit, isCreditsAvailable, type, radio);
    }


    /// <summary>
    /// The ID of this playlist item.
    /// </summary>
    /// <remarks>
    /// Only available if <see cref="Type"/> is <see cref="PlaylistItemType.Song"/> or <see cref="PlaylistItemType.Video"/>, else <see langword="null"/>.
    /// </remarks>
    public override string? Id { get; } = id;


    /// <summary>
    /// The thumbnails of this playlist item.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The artists of this playlist item.
    /// </summary>
    public YouTubeMusicEntity[] Artists { get; } = artists;

    /// <summary>
    /// The album of this playlist item.
    /// </summary>
    /// <remarks>
    /// Only available if <see cref="Type"/> is <see cref="PlaylistItemType.Song"/>, else <see langword="null"/>.
    /// </remarks>
    public YouTubeMusicEntity? Album { get; } = album;

    /// <summary>
    /// The duration of this playlist item.
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// Whether this playlist item is explicit or not.
    /// </summary>
    public bool IsExplicit { get; } = isExplicit;

    /// <summary>
    /// Whether credits are available to fetch for this playlist item.
    /// </summary>
    public bool IsCreditsAvailable { get; } = isCreditsAvailable;

    /// <summary>
    /// The type of this playlist item.
    /// </summary>
    public PlaylistItemType Type { get; } = type;

    /// <summary>
    /// The radio associated with this playlist item, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}