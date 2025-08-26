using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Videos;

/// <summary>
/// Represents a related video on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="RelatedVideo"/> class.
/// </remarks>
/// <param name="name">The name of this related video.</param>
/// <param name="id">The ID of this related video.</param>
/// <param name="thumbnails">The thumbnails of this related video.</param>
/// <param name="artists">The artists of this related video.</param>
/// <param name="viewsInfo">The information about the number of views this related video has.</param>
/// <param name="radio">The radio associated with this related video, if available.</param>
public class RelatedVideo(
    string name,
    string id,
    Thumbnail[] thumbnails,
    YouTubeMusicEntity[] artists,
    string viewsInfo,
    Radio? radio) : YouTubeMusicEntity(name, id, null)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="RelatedVideo"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static RelatedVideo Parse(
        JElement element)
    {
        JElement flexColumns = element
            .Get("flexColumns");

        JElement descriptionRuns = flexColumns
            .GetAt(1)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .Get("text")
            .Get("runs");


        string name = flexColumns
            .GetAt(0)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .Get("text")
            .Get("runs")
            .GetAt(0)
            .Get("text")
            .AsString()
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
            .SelectArtists();

        string viewsInfo = descriptionRuns
            .GetAt(artists.Length * 2)
            .Get("text")
            .AsString()
            .Or("N/A views");

        Radio? radio = element
            .SelectMenu()
            .SelectRadio();

        return new(name, id, thumbnails, artists, viewsInfo, radio);
    }


    /// <summary>
    /// The ID of this related video.
    /// </summary>
    public override string Id { get; } = id;


    /// <summary>
    /// The thumbnails of this related video.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The artists of this related video.
    /// </summary>
    public YouTubeMusicEntity[] Artists { get; } = artists;

    /// <summary>
    /// The information about the number of views this related video has.
    /// </summary>
    public string ViewsInfo { get; } = viewsInfo;

    /// <summary>
    /// The radio associated with this related video, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}