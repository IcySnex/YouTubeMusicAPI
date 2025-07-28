using System.Text.Json;
using YouTubeMusicAPI.Models;

namespace YouTubeMusicAPI.Utils;

/// <summary>
/// Contains extension methods to select specific data from JSON elements.
/// </summary>
internal static class Selectors
{
    /// <summary>
    /// Selects the navigation browse endpoint ID from a JSON element.
    /// </summary>
    /// <param name="element">The element containing "navigationEndpoint.browseEndpoint.browseId".</param>
    /// <returns></returns>
    public static string SelectNavigationBrowseId(
        this JsonElement element) =>
        element
            .GetProperty("navigationEndpoint")
            .GetProperty("browseEndpoint")
            .GetProperty("browseId")
            .GetString()
            .OrThrow();
    /// <summary>
    /// Selects the navigation browse endpoint ID from a JSON element or null if not found.
    /// </summary>
    /// <param name="element">The element containing "navigationEndpoint.browseEndpoint.browseId".</param>
    /// <returns></returns>
    public static string? SelectNavigationBrowseIdOrNull(
        this JsonElement element) =>
        element
            .GetPropertyOrNull("navigationEndpoint")
            ?.GetPropertyOrNull("browseEndpoint")
            ?.GetPropertyOrNull("browseId")
            ?.GetString();

    /// <summary>
    /// Selects the onTap browse endpoint ID from a JSON element.
    /// </summary>
    /// <param name="element">The element containing "onTap.browseEndpoint.browseId".</param>
    /// <returns></returns>
    public static string SelectTapBrowseId(
        this JsonElement element) =>
        element
            .GetProperty("onTap")
            .GetProperty("browseEndpoint")
            .GetProperty("browseId")
            .GetString()
            .OrThrow();

    /// <summary>
    /// Selects the navigation watch endpoint ID from a JSON element.
    /// </summary>
    /// <param name="element">The element containing "navigationEndpoint.watchEndpoint.videoId".</param>
    /// <returns></returns>
    public static string SelectNavigationVideoId(
        this JsonElement element) =>
        element
            .GetProperty("navigationEndpoint")
            .GetProperty("watchEndpoint")
            .GetProperty("videoId")
            .GetString()
            .OrThrow();

    /// <summary>
    /// Selects the overlay navigation playlist endpoint ID from a JSON element.
    /// </summary>
    /// <param name="element">The element containing "musicItemThumbnailOverlayRenderer.content.musicPlayButtonRenderer.playNavigationEndpoint.watchPlaylistEndpoint.playlistId".</param>
    /// <returns></returns>
    public static string SelectOverlayNavigationPlaylistId(
        this JsonElement element) =>
        element
            .GetProperty("musicItemThumbnailOverlayRenderer")
            .GetProperty("content")
            .GetProperty("musicPlayButtonRenderer")
            .GetProperty("playNavigationEndpoint")
            .GetProperty("watchPlaylistEndpoint")
            .GetProperty("playlistId")
            .GetString()
            .OrThrow();

    /// <summary>
    /// Selects the overlay navigation video endpoint ID from a JSON element.
    /// </summary>
    /// <param name="element">The element containing "musicItemThumbnailOverlayRenderer.content.musicPlayButtonRenderer.playNavigationEndpoint.watchEndpoint.videoId".</param>
    /// <returns></returns>
    public static string SelectOverlayNavigationVideoId(
        this JsonElement element) =>
        element
            .GetProperty("musicItemThumbnailOverlayRenderer")
            .GetProperty("content")
            .GetProperty("musicPlayButtonRenderer")
            .GetProperty("playNavigationEndpoint")
            .GetProperty("watchEndpoint")
            .GetProperty("videoId")
            .GetString()
            .OrThrow();


    /// <summary>
    /// Selects the menu items from a JSON element.
    /// </summary>
    /// <param name="element">The element containing "menu.menuRenderer.items".</param>
    /// <returns></returns>
    public static JsonElement SelectMenuItems(
        this JsonElement element) =>
        element
            .GetProperty("menu")
            .GetProperty("menuRenderer")
            .GetProperty("items");


    /// <summary>
    /// Selects the thumbnails from a JSON element.
    /// </summary>
    /// <param name="element">The elemnt containing "musicThumbnailRenderer.thumbnail.thumbnails".</param>
    /// <returns>An array containing the thumbnails.</returns>
    public static Thumbnail[] SelectThumbnails(
        this JsonElement element)
    {
        JsonElement thumbnails = element
            .GetProperty("thumbnail")
            .GetProperty("thumbnails");

        List<Thumbnail> result = [];
        foreach (JsonElement item in thumbnails.EnumerateArray())
        {
            Thumbnail thumbnail = Thumbnail.Parse(item);
            result.Add(thumbnail);
        }

        return [.. result];
    }

    /// <summary>
    /// Selects a radio from a JSON element.
    /// </summary>
    /// <param name="element">The array element.</param>
    /// <returns>An radio, if found.</returns>
    public static Radio? SelectRadioOrNull(
        this JsonElement element)
    {
        foreach (JsonElement item in element.EnumerateArray())
        {
            JsonElement? menu = item
                .GetPropertyOrNull("menuNavigationItemRenderer");

            if (menu is null)
                continue;

            string type = menu.Value
                .GetProperty("text")
                .GetProperty("runs")
                .GetElementAt(0)
                .GetProperty("text")
                .GetString()
                .OrThrow();

            if (type != "Start radio")
                continue;


            JsonElement navigationEndpoint = menu.Value
                .GetProperty("navigationEndpoint");

            JsonElement? watchEndpoint =
                navigationEndpoint
                    .GetPropertyOrNull("watchEndpoint")
                ?? navigationEndpoint
                    .GetPropertyOrNull("watchPlaylistEndpoint");

            if (watchEndpoint is null)
                return null;


            string? playlistId = watchEndpoint
                ?.GetPropertyOrNull("playlistId")
                ?.GetString();

            if (playlistId is null)
                return null;

            string? songVideoId = watchEndpoint
                ?.GetPropertyOrNull("videoId")
                ?.GetString();

            return new(playlistId, songVideoId);
        }

        return null;
    }

    /// <summary>
    /// Selects a playlist ID from a JSON element.
    /// </summary>
    /// <param name="element">The array element.</param>
    /// <returns>A playlist ID, if found.</returns>
    public static string? SelectPlaylistIdOrNull(
        this JsonElement element)
    {
        foreach (JsonElement item in element.EnumerateArray())
        {
            JsonElement? menu = item
                .GetPropertyOrNull("menuNavigationItemRenderer");

            if (menu is null)
                continue;

            string type = menu.Value
                .GetProperty("text")
                .GetProperty("runs")
                .GetElementAt(0)
                .GetProperty("text")
                .GetString()
                .OrThrow();

            if (type != "Shuffle play")
                continue;


            JsonElement? watchEndpoint = menu.Value
                .GetProperty("navigationEndpoint")
                .GetPropertyOrNull("watchPlaylistEndpoint");

            if (watchEndpoint is null)
                return null;


            string? playlistId = watchEndpoint
                ?.GetPropertyOrNull("playlistId")
                ?.GetString();

            return playlistId;
        }

        return null;
    }

    /// <summary>
    /// Selects whether the given JSON element contains an explicit badge.
    /// </summary>
    /// <param name="element">The element containing "musicInlineBadgeRenderer.icon.iconType".</param>
    /// <returns>A boolean indicating wether a explicit badge is contained.</returns>
    public static bool SelectContainsExplicitBadge(
        this JsonElement? element)
    {
        if (element.HasValue)
            foreach (JsonElement badge in element.Value.EnumerateArray())
            {
                string? iconType = badge
                    .GetPropertyOrNull("musicInlineBadgeRenderer")
                    ?.GetPropertyOrNull("icon")
                    ?.GetPropertyOrNull("iconType")
                    ?.GetString();

                if (iconType == "MUSIC_EXPLICIT_BADGE")
                    return true;
            }

        return false;
    }


    /// <summary>
    /// Selects an artist from a JSON element
    /// </summary>
    /// <param name="element">The element containing "text" and "navigationEndpoint.browseEndpoint.browseId".</param>
    /// <returns>An artist.</returns>
    public static YouTubeMusicEntity SelectArtist(
        this JsonElement element)
    {
        string text = element
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string? id = element
            .SelectNavigationBrowseIdOrNull();

        return new(text, id, id);
    }

    /// <summary>
    /// Selects the artists from a JSON element.
    /// </summary>
    /// <param name="element">The array element.</param>
    /// <param name="startIndex">The index to start from.</param>
    /// <returns>An array containing the artists.</returns>
    public static YouTubeMusicEntity[] SelectArtists(
        this JsonElement element,
        int startIndex = 0)
    {
        int index = 0;

        List<YouTubeMusicEntity> result = [];
        foreach (JsonElement run in element.EnumerateArray())
        {
            if (index++ < startIndex)
                continue;

            string text = run
                .GetProperty("text")
                .GetString()
                .OrThrow()
                .Trim();

            switch (text)
            {
                case "•":
                    return [.. result];
                case "&":
                case ",":
                    continue;
            }

            string? id = run
                .SelectNavigationBrowseIdOrNull();

            result.Add(new(text, id, id));
        }

        return [.. result];
    }


    /// <summary>
    /// Selects an album from a JSON element
    /// </summary>
    /// <param name="element">The element containing "text" and "navigationEndpoint.browseEndpoint.browseId".</param>
    /// <returns>An album.</returns>
    public static YouTubeMusicEntity SelectAlbum(
        this JsonElement element)
    {
        string text = element
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string? id = element
            .SelectNavigationBrowseIdOrNull();

        return new(text, null, id);
    }

    /// <summary>
    /// Selects an unknown album (only id) from a JSON element.
    /// </summary>
    /// <param name="element">The array element.</param>
    /// <returns>An album with the name 'N/A'.</returns>
    public static YouTubeMusicEntity SelectAlbumUnknown(
        this JsonElement element)
    {
        foreach (JsonElement item in element.EnumerateArray())
        {
            JsonElement? menu = item
                .GetPropertyOrNull("menuNavigationItemRenderer");

            if (menu is null)
                continue;

            string type = menu.Value
                .GetProperty("text")
                .GetProperty("runs")
                .GetElementAt(0)
                .GetProperty("text")
                .GetString()
                .OrThrow();

            if (type != "Go to album")
                continue;


            string name = "N/A";

            string? browseId = menu.Value
                .GetPropertyOrNull("navigationEndpoint")
                ?.GetPropertyOrNull("browseEndpoint")
                ?.GetPropertyOrNull("browseId")
                ?.GetString();

            return new(name, null, browseId);
        }

        return new("N/A", null, null);
    }


    /// <summary>
    /// Selects a podcast from a JSON element
    /// </summary>
    /// <param name="element">The element containing "text" and "navigationEndpoint.browseEndpoint.browseId".</param>
    /// <returns>A podcast.</returns>
    public static YouTubeMusicEntity SelectPodcast(
        this JsonElement element)
    {
        string text = element
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string? id = element
            .SelectNavigationBrowseIdOrNull();

        return new(text, id?.Substring(4), id);
    }


    /// <summary>
    /// Selects weither credits are available to fetch for a song from a JSON element.
    /// </summary>
    /// <param name="element">The array element.</param>
    /// <returns>A boolean weither credits are available.</returns>
    public static bool SelectIsCreditsAvailable(
        this JsonElement element)
    {
        foreach (JsonElement item in element.EnumerateArray())
        {
            JsonElement? menu = item
                .GetPropertyOrNull("menuNavigationItemRenderer");

            if (menu is null)
                continue;

            string type = menu.Value
                .GetProperty("text")
                .GetProperty("runs")
                .GetElementAt(0)
                .GetProperty("text")
                .GetString()
                .OrThrow();

            if (type != "View song credits")
                continue;


            string? browseId = menu.Value
                .GetPropertyOrNull("navigationEndpoint")
                ?.GetPropertyOrNull("browseEndpoint")
                ?.GetPropertyOrNull("browseId")
                ?.GetString();

            return browseId is not null;
        }

        return false;
    }
}