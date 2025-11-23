using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Artists;

/// <summary>
/// Represents an artist correlation on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="ArtistCorrelation"/> class.
/// </remarks>
/// <param name="name">The name of this artist correlation.</param>
/// <param name="id">The ID of this artist correlation.</param>
/// <param name="thumbnails">The thumbnails of this artist correlation.</param>
/// <param name="audienceInfo">The information about the audience of this artist correlation (e.g. subscribers, monthly listeners).</param>
/// <param name="radio">The radio associated with this artist correlation, if available.</param>
public class ArtistCorrelation(
    string name,
    string id,
    Thumbnail[] thumbnails,
    string audienceInfo,
    Radio? radio) : YouTubeMusicEntity(name, id, id)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="ArtistCorrelation"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicTwoRowItemRenderer".</param>
    internal static ArtistCorrelation Parse(
        JElement element)
    {
        string name = element
            .SelectRunTextAt("title", 0)
            .OrThrow();

        string id = element
            .SelectNavigationBrowseId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnailRenderer")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        string audienceInfo = element
            .SelectRunTextAt("subtitle", 0)
            .Or("N/A subscribers");

        Radio? radio = element
            .SelectMenu()
            .SelectRadio();

        return new(name, id, thumbnails, audienceInfo, radio);
    }


    /// <summary>
    /// The ID of this artist correlation.
    /// </summary>
    public override string Id { get; } = id;

    /// <summary>
    /// The browse ID of this artist correlation.
    /// </summary>
    public override string BrowseId { get; } = id;


    /// <summary>
    /// The thumbnails of this artist correlation.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The information about the audience of this artist correlation (e.g. subscribers, monthly listeners).
    /// </summary>
    public string AudienceInfo { get; } = audienceInfo;

    /// <summary>
    /// The radio associated with this artist correlation, if available.
    /// </summary>
    public Radio? radio { get; } = radio;
}