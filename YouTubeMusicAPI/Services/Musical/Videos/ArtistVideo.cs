using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Musical.Videos;

/// <summary>
/// Represents an artist video on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="ArtistVideo"/> class.
/// </remarks>
/// <param name="name">The name of this artist video.</param>
/// <param name="id">The ID of this artist video.</param>
/// <param name="thumbnails">The thumbnails of this artist video.</param>
/// <param name="artists">The artists of this artist video.</param>
/// <param name="viewsInfo">The information about the number of views this artist video has.</param>
/// <param name="radio">The radio associated with this artist video, if available.</param>
public class ArtistVideo(
    string name,
    string id,
    Thumbnail[] thumbnails,
    YouTubeMusicEntity[] artists,
    string viewsInfo,
    Radio? radio) : YouTubeMusicEntity(name, id, null)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="ArtistVideo"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicTwoRowItemRenderer".</param>
    internal static ArtistVideo Parse(
        JElement element)
    {
        JElement subtitleRuns = element
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
            .Get("thumbnailRenderer")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity[] artists = subtitleRuns
            .SelectArtists();

        string viewsInfo = subtitleRuns
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
    /// The ID of this artist video.
    /// </summary>
    public override string Id { get; } = id;


    /// <summary>
    /// The thumbnails of this artist video.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The artists of this artist video.
    /// </summary>
    public YouTubeMusicEntity[] Artists { get; } = artists;

    /// <summary>
    /// The information about the number of views this artist video has.
    /// </summary>
    public string ViewsInfo { get; } = viewsInfo;

    /// <summary>
    /// The radio associated with this artist video, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}