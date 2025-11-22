using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Albums;

/// <summary>
/// Represents an item of an album on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="AlbumItem"/> class.
/// </remarks>
/// <param name="name">The name of this album item.</param>
/// <param name="id">The ID of this album item.</param>
/// <param name="artists">The artists of this album item.</param>
/// <param name="duration">The duration of this album item.</param>
/// <param name="isExplicit">Whether this album item is explicit or not.</param>
/// <param name="index">The index of this album item.</param>
/// <param name="playsInfo">The information about the numbers of plays this album item has.</param>
/// <param name="isCreditsAvailable">Whether credits are available to fetch for this album item.</param>
/// <param name="radio">The radio associated with this album item, if available.</param>
public class AlbumItem(
    string name,
    string id,
    YouTubeMusicEntity[] artists,
    TimeSpan duration,
    bool isExplicit,
    int index,
    string playsInfo,
    bool isCreditsAvailable,
    Radio? radio) : YouTubeMusicEntity(name, id, null)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="AlbumItem"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    /// <param name="artists">The artists of this album.</param>
    internal static AlbumItem Parse(
        JElement element,
        YouTubeMusicEntity[] artists)
    {
        JElement menu = element
            .SelectMenu();

        JElement flexColumns = element
            .Get("flexColumns");

        JElement artistRuns = flexColumns
            .GetAt(1)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .Get("text")
            .Get("runs");


        string name = flexColumns
            .GetAt(0)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .SelectRunTextAt("text", 0)
            .OrThrow();

        string id = element
            .Get("overlay")
            .SelectOverlayVideoId()
            .OrThrow();

        artists = artistRuns
            .IsUndefined
                ? artists
                : artistRuns
                    .SelectArtists();

        TimeSpan duration = element
            .Get("fixedColumns")
            .GetAt(0)
            .Get("musicResponsiveListItemFixedColumnRenderer")
            .SelectRunTextAt("text", 0)
            .ToTimeSpan()
            .Or(TimeSpan.Zero);

        bool isExplicit = element
            .SelectIsExplicit();

        int index = element
            .SelectRunTextAt("index", 0)
            .ToInt32()
            .OrThrow();

        string playsInfo = flexColumns
            .GetAt(2)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .SelectRunTextAt("text", 0)
            .Or("N/A plays");

        bool isCreditsAvailable = menu
            .SelectIsCreditsAvailable();

        Radio? radio = menu
            .SelectRadio();

        return new(name, id, artists, duration, isExplicit, index, playsInfo, isCreditsAvailable, radio);
    }


    /// <summary>
    /// The ID of this album item.
    /// </summary>
    public override string Id { get; } = id;


    /// <summary>
    /// The artists of this album item.
    /// </summary>
    public YouTubeMusicEntity[] Artists { get; } = artists;

    /// <summary>
    /// The duration of this album item.
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// Whether this album item is explicit or not.
    /// </summary>
    public bool IsExplicit { get; } = isExplicit;

    /// <summary>
    /// The index of this album item.
    /// </summary>
    public int Index { get; } = index;

    /// <summary>
    /// The information about the numbers of plays this album item has.
    /// </summary>
    public string PlaysInfo { get; } = playsInfo;

    /// <summary>
    /// Whether credits are available to fetch for this album item.
    /// </summary>
    public bool IsCreditsAvailable { get; } = isCreditsAvailable;

    /// <summary>
    /// The radio associated with this album item, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}