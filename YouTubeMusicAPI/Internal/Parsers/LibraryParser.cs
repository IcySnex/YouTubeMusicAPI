using Newtonsoft.Json.Linq;
using System.Globalization;
using YouTubeMusicAPI.Models.Library;
using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Internal.Parsers;

/// <summary>
/// Contains methods to parse library items from json tokens
/// </summary>
internal class LibraryParser
{
    /// <summary>
    /// Parses community playlist info data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>The community playlist</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static LibraryCommunityPlaylist GetCommunityPlaylist(
        JObject jsonToken)
    {
        JToken[] runs = jsonToken.SelectObject<JToken[]>("musicTwoRowItemRenderer.subtitle.runs");

        string? createdId = jsonToken.SelectObjectOptional<string>("musicTwoRowItemRenderer.subtitle.runs[0].navigationEndpoint.browseEndpoint.browseId");

        return new(
            name: jsonToken.SelectObject<string>("musicTwoRowItemRenderer.title.runs[0].text"),
            id: jsonToken.SelectObject<string>("musicTwoRowItemRenderer.thumbnailOverlay.musicItemThumbnailOverlayRenderer.content.musicPlayButtonRenderer.playNavigationEndpoint.watchPlaylistEndpoint.playlistId"),
            creator: new(createdId is null ? "YouTube Music" : jsonToken.SelectObject<string>("musicTwoRowItemRenderer.subtitle.runs[0].text"), createdId, YouTubeMusicItemKind.Profiles),
            songCount: int.Parse(jsonToken.SelectObject<string>($"musicTwoRowItemRenderer.subtitle.runs[{runs.Length - 1}].text").Split(' ')[0], NumberStyles.AllowThousands, CultureInfo.InvariantCulture),
            radio: jsonToken.SelectRadio("musicTwoRowItemRenderer.menu.menuRenderer.items[1].menuNavigationItemRenderer.navigationEndpoint.watchPlaylistEndpoint.playlistId", null),
            thumbnails: jsonToken.SelectThumbnails("musicTwoRowItemRenderer.thumbnailRenderer.musicThumbnailRenderer.thumbnail.thumbnails"));
    }

    /// <summary>
    /// Parses song info data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>The song</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static LibrarySong GetSong(
        JObject jsonToken)
    {
        return new(
            name: jsonToken.SelectObject<string>("musicResponsiveListItemRenderer.flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text"),
            id: jsonToken.SelectObject<string>("musicResponsiveListItemRenderer.flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].navigationEndpoint.watchEndpoint.videoId"),
            artists: jsonToken.SelectArtists("musicResponsiveListItemRenderer.flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs"),
            album: jsonToken.SelectYouTubeMusicItem("musicResponsiveListItemRenderer.flexColumns[2].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text", "musicResponsiveListItemRenderer.flexColumns[2].musicResponsiveListItemFlexColumnRenderer.text.runs[0].navigationEndpoint.browseEndpoint.browseId", YouTubeMusicItemKind.Albums),
            duration: jsonToken.SelectObject<string>($"musicResponsiveListItemRenderer.fixedColumns[0].musicResponsiveListItemFixedColumnRenderer.text.runs[0].text").ToTimeSpan(),
            isExplicit: jsonToken.SelectIsExplicit("musicResponsiveListItemRenderer.badges"),
            radio: jsonToken.SelectRadio("musicResponsiveListItemRenderer.menu.menuRenderer.items[0].menuNavigationItemRenderer.navigationEndpoint.watchEndpoint.playlistId", "musicResponsiveListItemRenderer.menu.menuRenderer.items[0].menuNavigationItemRenderer.navigationEndpoint.watchEndpoint.videoId"),
            thumbnails: jsonToken.SelectThumbnails("musicResponsiveListItemRenderer.thumbnail.musicThumbnailRenderer.thumbnail.thumbnails"));
    }
}