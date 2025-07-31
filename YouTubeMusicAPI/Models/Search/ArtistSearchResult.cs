using System.Text.Json;
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
    /// Parses a <see cref="JsonElement"/> into an <see cref="ArtistSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> "musicResponsiveListItemRenderer".</param>
    internal static ArtistSearchResult Parse(
        JsonElement element)
    {
        JsonElement flexColumns = element
            .GetProperty("flexColumns");

        JsonElement descriptionRuns = flexColumns
            .GetElementAt(1)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs");

        int descriptionStartIndex = descriptionRuns
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow()
            .If("Artist", 2, 0);


        string name = flexColumns
            .GetElementAt(0)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs")
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = element
            .SelectNavigationBrowseId();

        Thumbnail[] thumbnails = element
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        string audienceInfo = (descriptionRuns
            .GetElementAtOrNull(descriptionStartIndex + 2)
            ?.GetPropertyOrNull("text")
            ?.GetString())
            .Or("N/A subscribers");

        Radio? radio = element
            .SelectMenuItems()
            .SelectRadioOrNull();

        return new(name, id, thumbnails, audienceInfo, radio);
    }

    /// <summary>
    /// Parses a <see cref="JsonElement"/> into a <see cref="ArtistSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> "musicCardShelfRenderer".</param>
    internal static ArtistSearchResult ParseTopResult(
        JsonElement element)
    {
        string name = element
            .GetProperty("title")
            .GetProperty("runs")
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = element
            .SelectTapBrowseId();

        Thumbnail[] thumbnails = element
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        string audienceInfo = (element
            .GetProperty("subtitle")
            .GetProperty("runs")
            .GetElementAtOrNull(2)
            ?.GetPropertyOrNull("text")
            ?.GetString())
            .Or("N/A subscribers");

        Radio? radio = element
            .SelectMenuItems()
            .SelectRadioOrNull();

        return new(name, id, thumbnails, audienceInfo, radio);
    }

    /// <summary>
    /// Parses a <see cref="JsonElement"/> into an <see cref="ArtistSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> "musicResponsiveListItemRenderer".</param>
    internal static ArtistSearchResult ParseSuggestion(
        JsonElement element)
    {
        string name = element
            .GetProperty("flexColumns")
            .GetElementAt(0)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs")
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = element
            .SelectNavigationBrowseId();

        Thumbnail[] thumbnails = element
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        string audienceInfo = "N/A subscribers";

        Radio? radio = element
            .SelectMenuItems()
            .SelectRadioOrNull();

        return new(name, id, thumbnails, audienceInfo, radio);
    }


    /// <summary>
    /// The information about the audience of this artist (e.g. subscribers, monthly listeners).
    /// </summary>
    public string AudienceInfo { get; } = audienceInfo;

    /// <summary>
    /// The radio associated with this artist, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}