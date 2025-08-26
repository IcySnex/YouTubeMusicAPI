using YouTubeMusicAPI.Common;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Services.Search;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Playlists;

/// <summary>
/// Represents a community playlist search result on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="CommunityPlaylistSearchResult"/> class.
/// </remarks>
/// <param name="name">The name of this community playlist.</param>
/// <param name="id">The ID of this community playlist.</param>
/// <param name="thumbnails">The thumbnails of this community playlist.</param>
/// <param name="browseId">The browse ID of this community playlist.</param>
/// <param name="creator">The artists of this community playlist.</param>
/// <param name="viewsInfo">The information about the number of views this community playlist has.</param>
/// <param name="radio">The radio associated with this community playlist, if available.</param>
public class CommunityPlaylistSearchResult(
    string name,
    string id,
    string browseId,
    Thumbnail[] thumbnails,
    YouTubeMusicEntity creator,
    string viewsInfo,
    Radio? radio) : SearchResult(name, id, browseId, thumbnails)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="CommunityPlaylistSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static CommunityPlaylistSearchResult Parse(
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
            .If("Playlist", 2, 0);


        string name = flexColumns
            .GetAt(0)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .SelectRunTextAt("text", 0)
            .OrThrow();

        string id = element
            .Get("overlay")
            .SelectOverlayPlaylistId()
            .OrThrow();

        string browseId = element
            .SelectNavigationBrowseId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity creator = descriptionRuns
            .GetAt(descriptionStartIndex)
            .SelectArtist();

        string viewsInfo = descriptionRuns
            .GetAt(descriptionStartIndex + 2)
            .Get("text")
            .AsString()
            .Or("N/A views");

        Radio? radio = element
            .SelectMenu()
            .SelectRadio();

        return new(name, id, browseId, thumbnails, creator, viewsInfo, radio);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="CommunityPlaylistSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicCardShelfRenderer".</param>
    internal static CommunityPlaylistSearchResult ParseTopResult(
        JElement element)
    {
        JElement descriptionRuns = element
            .Get("subtitle")
            .Get("runs");


        string name = element
            .SelectRunTextAt("title", 0)
            .OrThrow();

        string id = element
            .Get("thumbnailOverlay")
            .SelectOverlayPlaylistId()
            .OrThrow();

        string browseId = element
            .SelectTapBrowseId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity creator = descriptionRuns
            .GetAt(2)
            .SelectArtist();

        string viewsInfo = "N/A views";

        Radio? radio = element
            .SelectMenu()
            .SelectRadio();

        return new(name, id, browseId, thumbnails, creator, viewsInfo, radio);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="CommunityPlaylistSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static CommunityPlaylistSearchResult ParseSuggestion(
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
            .If("Playlist", 2, 0);


        string name = flexColumns
            .GetAt(0)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .SelectRunTextAt("text", 0)
            .OrThrow();

        string id = element
            .Get("overlay")
            .SelectOverlayPlaylistId()
            .OrThrow();

        string browseId = element
            .SelectNavigationBrowseId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity creator = descriptionRuns
            .GetAt(descriptionStartIndex)
            .SelectArtist();

        string viewsInfo = descriptionRuns
            .GetAt(descriptionStartIndex + 2)
            .Get("text")
            .AsString()
            .Or("N/A views");

        Radio? radio = element
            .SelectMenu()
            .SelectRadio();

        return new(name, id, browseId, thumbnails, creator, viewsInfo, radio);
    }


    /// <summary>
    /// The browse ID of this community playlist.
    /// </summary>
    public override string BrowseId { get; } = browseId;


    /// <summary>
    /// The creator of this community playlist.
    /// </summary>
    public YouTubeMusicEntity Creator { get; } = creator;

    /// <summary>
    /// The information about the number of views this community playlist has.
    /// </summary>
    public string ViewsInfo { get; } = viewsInfo;

    /// <summary>
    /// The radio associated with this community playlist, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}