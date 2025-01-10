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
    /// <returns>The community playlist info</returns>
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
}