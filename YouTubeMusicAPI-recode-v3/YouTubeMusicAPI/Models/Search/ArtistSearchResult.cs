using System.Text.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents an artist search result on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="ArtistSearchResult"/>.
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
    Radio? radio) : SearchResult(name, id, thumbnails)
{
    /// <summary>
    /// Parses the JSON element into an <see cref="ArtistSearchResult"/>.
    /// </summary>
    /// <param name="element">The JSON element containing "musicResponsiveListItemRenderer".</param>
    internal static ArtistSearchResult Parse(
        JsonElement element)
    {
        JsonElement item = element
            .GetProperty("musicResponsiveListItemRenderer");

        JsonElement flexColumns = item
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

        string id = item
            .SelectNavigationBrowseId();

        Thumbnail[] thumbnails = item
            .GetProperty("thumbnail")
            .SelectThumbnails();

        string audienceInfo = (descriptionRuns
            .GetElementAtOrNull(descriptionStartIndex + 2)
            ?.GetPropertyOrNull("text")
            ?.GetString())
            .Or("N/A subscribers");

        Radio? radio = item
            .SelectMenuItems()
            .SelectRadioOrNull();

        return new(name, id, thumbnails, audienceInfo, radio);
    }

    /// <summary>
    /// Parses the JSON item into an <see cref="ArtistSearchResult"/>.
    /// </summary>
    /// <param name="item">The JSON item "musicCardShelfRenderer".</param>
    internal static ArtistSearchResult ParseTopResult(
        JsonElement item)
    {
        string name = item
            .GetProperty("title")
            .GetProperty("runs")
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = item
            .SelectTapBrowseId();

        Thumbnail[] thumbnails = item
            .GetProperty("thumbnail")
            .SelectThumbnails();

        string audienceInfo = (item
            .GetProperty("subtitle")
            .GetProperty("runs")
            .GetElementAtOrNull(2)
            ?.GetPropertyOrNull("text")
            ?.GetString())
            .Or("N/A subscribers");

        Radio? radio = item
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