using System.Text.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a playlist search result on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="PlaylistSearchResult"/> class.
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
    string browseId,
    Thumbnail[] thumbnails,
    YouTubeMusicEntity creator,
    string viewsInfo,
    Radio? radio) : SearchResult(name, id, browseId, thumbnails)
{
    /// <summary>
    /// Parses a <see cref="JsonElement"/> into a <see cref="PlaylistSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> "musicResponsiveListItemRenderer".</param>
    internal static PlaylistSearchResult Parse(
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
            .If("Playlist", 2, 0);


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
            .GetProperty("overlay")
            .SelectOverlayNavigationPlaylistId();

        string browseId = element
            .SelectNavigationBrowseId();

        Thumbnail[] thumbnails = element
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity creator = descriptionRuns
            .GetElementAt(descriptionStartIndex)
            .SelectArtist();

        string viewsSongsInfo = (descriptionRuns
            .GetElementAtOrNull(descriptionStartIndex + 2)
            ?.GetPropertyOrNull("text")
            ?.GetString())
            .Or("N/A views");

        string viewsInfo = viewsSongsInfo.EndsWith(" songs") ? "N/A views" : viewsSongsInfo; // bruh automated playlists dont have a "view count"

        Radio? radio = element
            .SelectMenuItems()
            .SelectRadioOrNull();

        return new(name, id, browseId, thumbnails, creator, viewsInfo, radio);
    }

    /// <summary>
    /// Parses a <see cref="JsonElement"/> into a <see cref="PlaylistSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> "musicCardShelfRenderer".</param>
    internal static PlaylistSearchResult ParseTopResult(
        JsonElement element)
    {
        JsonElement descriptionRuns = element
            .GetProperty("subtitle")
            .GetProperty("runs");


        string name = element
            .GetProperty("title")
            .GetProperty("runs")
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = element
            .GetProperty("thumbnailOverlay")
            .SelectOverlayNavigationPlaylistId();

        string browseId = element
            .SelectTapBrowseId();

        Thumbnail[] thumbnails = element
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity creator = descriptionRuns
            .GetElementAt(2)
            .SelectArtist();

        string viewsInfo = "N/A views";

        Radio? radio = element
            .SelectMenuItems()
            .SelectRadioOrNull();

        return new(name, id, browseId, thumbnails, creator, viewsInfo, radio);
    }

    /// <summary>
    /// Parses a <see cref="JsonElement"/> into a <see cref="PlaylistSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> "musicResponsiveListItemRenderer".</param>
    internal static PlaylistSearchResult ParseSuggestion(
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
            .If("Playlist", 2, 0);


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
            .GetProperty("overlay")
            .SelectOverlayNavigationPlaylistId();

        string browseId = element
            .SelectNavigationBrowseId();

        Thumbnail[] thumbnails = element
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity creator = descriptionRuns
            .GetElementAt(descriptionStartIndex)
            .SelectArtist();

        string viewsInfo = (descriptionRuns
            .GetElementAtOrNull(descriptionStartIndex + 2)
            ?.GetPropertyOrNull("text")
            ?.GetString())
            .Or("N/A views");

        Radio? radio = element
            .SelectMenuItems()
            .SelectRadioOrNull();

        return new(name, id, browseId, thumbnails, creator, viewsInfo, radio);
    }


    /// <summary>
    /// The browse ID of this playlist.
    /// </summary>
    public override string BrowseId { get; } = browseId;


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