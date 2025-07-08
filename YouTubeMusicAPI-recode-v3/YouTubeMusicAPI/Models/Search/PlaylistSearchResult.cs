using System.Text.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a playlist search result on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="PlaylistSearchResult"/>.
/// </remarks>
/// <param name="name">The name of this playlist.</param>
/// <param name="id">The ID of this playlist.</param>
/// <param name="thumbnails">The thumbnails of this playlist.</param>
/// <param name="browseId">The browse ID of this playlist.</param>
/// <param name="creator">The artists of this playlist.</param>
/// <param name="viewsInfo">The information about the number of views this playlist has.</param>
/// <param name="radio">The radio associated with this playlist, if available.</param>
public class PlaylistSearchResult(
    string name,
    string id,
    Thumbnail[] thumbnails,
    string browseId,
    YouTubeMusicEntity creator,
    string viewsInfo,
    Radio? radio) : SearchResult(name, id, thumbnails)
{
    /// <summary>
    /// Parses the JSON element into a <see cref="PlaylistSearchResult"/>.
    /// </summary>
    /// <param name="element">The JSON element containing "musicResponsiveListItemRenderer".</param>
    internal static PlaylistSearchResult Parse(
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


        string name = flexColumns
            .GetElementAt(0)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs")
            .GetElementAt(0)
            .GetProperty("text")
            .GetStringOrEmpty();

        string id = item
            .SelectOverlayNavigationPlaylistId();

        Thumbnail[] thumbnails = item
            .GetProperty("thumbnail")
            .SelectThumbnails();

        string browseId = item
            .SelectNavigationBrowseId();

        YouTubeMusicEntity creator = descriptionRuns
            .GetElementAt(0)
            .SelectYouTubeMusicEntity();

        string viewsInfo = descriptionRuns
            .GetElementAt(2)
            .GetProperty("text")
            .GetStringOrEmpty();

        Radio? radio = item
            .GetProperty("menu")
            .GetProperty("menuRenderer")
            .GetProperty("items")
            .SelectRadioOrNull();

        return new(name, id, thumbnails, browseId, creator, viewsInfo, radio);
    }


    /// <summary>
    /// The browse ID of this playlist.
    /// </summary>
    public string BrowseId { get; } = browseId;

    /// <summary>
    /// The creator of this playlist.
    /// </summary>
    public YouTubeMusicEntity Creator { get; } = creator;

    /// <summary>
    /// The information about the number of views this playlist has.
    /// </summary>
    public string ViewsInfo { get; } = viewsInfo;

    /// <summary>
    /// The radio associated with this playlist, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}