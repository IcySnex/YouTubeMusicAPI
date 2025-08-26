using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents an artist search result on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="ArtistSearchResult"/> class.
/// </remarks>
/// <param name="name">The name of this artist.</param>
/// <param name="id">The ID of this artist.</param>
/// <param name="thumbnails">The thumbnails of this artist.</param>
/// <param name="audienceInfo">The information about the audience of this artist (e.g. subscribers, monthly listeners).</param>
/// <param name="radio">The radio associated with this artist, if available.</param>
public class ArtistSearchResult(
    string name,
    string id,
    Thumbnail[] thumbnails,
    string audienceInfo,
    Radio? radio) : SearchResult(name, id, id, thumbnails)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into an <see cref="ArtistSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static ArtistSearchResult Parse(
        JElement element)
    {
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
            .If("Artist", 2, 0);


        string name = flexColumns
            .GetAt(0)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .SelectRunTextAt("text", 0)
            .OrThrow();

        string id = element
            .SelectNavigationBrowseId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        string audienceInfo = descriptionRuns
            .GetAt(descriptionStartIndex)
            .Get("text")
            .AsString()
            .Or("N/A subscribers");

        Radio? radio = element
            .SelectMenu()
            .SelectRadio();

        return new(name, id, thumbnails, audienceInfo, radio);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="ArtistSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicCardShelfRenderer".</param>
    internal static ArtistSearchResult ParseTopResult(
        JElement element)
    {
        string name = element
            .SelectRunTextAt("title", 0)
            .OrThrow();

        string id = element
            .SelectTapBrowseId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        string audienceInfo = element
            .SelectRunTextAt("subtitle", 2)
            .Or("N/A subscribers");

        Radio? radio = element
            .SelectMenu()
            .SelectRadio();

        return new(name, id, thumbnails, audienceInfo, radio);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into an <see cref="ArtistSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static ArtistSearchResult ParseSuggestion(
        JElement element)
    {
        string name = element
            .Get("flexColumns")
            .GetAt(0)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .SelectRunTextAt("text", 0)
            .OrThrow();

        string id = element
            .SelectNavigationBrowseId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        string audienceInfo = "N/A subscribers";

        Radio? radio = element
            .SelectMenu()
            .SelectRadio();

        return new(name, id, thumbnails, audienceInfo, radio);
    }


    /// <summary>
    /// The browse ID of this artist.
    /// </summary>
    public override string BrowseId { get; } = id;


    /// <summary>
    /// The information about the audience of this artist (e.g. subscribers, monthly listeners).
    /// </summary>
    public string AudienceInfo { get; } = audienceInfo;

    /// <summary>
    /// The radio associated with this artist, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}