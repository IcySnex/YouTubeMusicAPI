using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Artists;

/// <summary>
/// Represents a related artist on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="RelatedArtist"/> class.
/// </remarks>
/// <param name="name">The name of this related artist.</param>
/// <param name="id">The ID of this related artist.</param>
/// <param name="thumbnails">The thumbnails of this related artist.</param>
/// <param name="audienceInfo">The information about the audience of this related artist (e.g. subscribers, monthly listeners).</param>
/// <param name="radio">The radio associated with this related artist, if available.</param>
public class RelatedArtist(
    string name,
    string id,
    Thumbnail[] thumbnails,
    string audienceInfo,
    Radio? radio) : YouTubeMusicEntity(name, id, id)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="RelatedArtist"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicTwoRowItemRenderer".</param>
    internal static RelatedArtist Parse(
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
    /// The ID of this related artist.
    /// </summary>
    public override string Id { get; } = id;

    /// <summary>
    /// The browse ID of this related artist.
    /// </summary>
    public override string BrowseId { get; } = id;


    /// <summary>
    /// The thumbnails of this related artist.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The information about the audience of this related artist (e.g. subscribers, monthly listeners).
    /// </summary>
    public string AudienceInfo { get; } = audienceInfo;

    /// <summary>
    /// The radio associated with this related artist, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}