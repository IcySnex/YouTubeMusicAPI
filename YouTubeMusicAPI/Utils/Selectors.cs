using System.Text.Json;
using YouTubeMusicAPI.Models;

namespace YouTubeMusicAPI.Utils;

/// <summary>
/// Contains extension methods to select specific data from JSON elements.
/// </summary>
internal static class Selectors
{
    public static JsonElement SelectRuns(
        this JsonElement element,
        string propertyName) =>
        element
            .GetProperty(propertyName)
            .GetProperty("runs");

    public static string SelectRunText(
        this JsonElement runs,
        int index) =>
        runs
            .GetElementAt(index)
            .GetProperty("text")
            .GetString()
            .OrThrow();

    public static string SelectRunTextAt(
        this JsonElement element,
        string propertyName,
        int index = 0) =>
        element
            .SelectRuns(propertyName)
            .SelectRunText(index);


    public static string SelectNavigationBrowseId(
        this JsonElement element) =>
        element
            .GetProperty("navigationEndpoint")
            .GetProperty("browseEndpoint")
            .GetProperty("browseId")
            .GetString()
            .OrThrow();
    public static string? SelectNavigationBrowseIdOrNull(
        this JsonElement element) =>
        element
            .GetPropertyOrNull("navigationEndpoint")
            ?.GetPropertyOrNull("browseEndpoint")
            ?.GetPropertyOrNull("browseId")
            ?.GetString();

    public static string SelectTapBrowseId(
        this JsonElement element) =>
        element
            .GetProperty("onTap")
            .GetProperty("browseEndpoint")
            .GetProperty("browseId")
            .GetString()
            .OrThrow();

    public static string SelectNavigationVideoId(
        this JsonElement element) =>
        element
            .GetProperty("navigationEndpoint")
            .GetProperty("watchEndpoint")
            .GetProperty("videoId")
            .GetString()
            .OrThrow();

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


    public static JsonElement SelectMenuItems(
        this JsonElement element) =>
        element
            .GetProperty("menu")
            .GetProperty("menuRenderer")
            .GetProperty("items");


    public static Thumbnail[] SelectThumbnails(
        this JsonElement element,
        string propertyName = "thumbnail")
    {
        JsonElement thumbnails = element
            .GetProperty(propertyName)
            .GetProperty("thumbnails");

        List<Thumbnail> result = [];
        foreach (JsonElement item in thumbnails.EnumerateArray())
        {
            Thumbnail thumbnail = Thumbnail.Parse(item);
            result.Add(thumbnail);
        }

        return [.. result];
    }

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


    public static bool SelectIsPodcastEvenThoItShouldnt(
        this JsonElement element,
        string categoryTitle)
    {
        JsonElement menuItems = element
            .GetProperty("menu")
            .GetProperty("menuRenderer");

        return menuItems.TryGetProperty("topLevelButtons", out JsonElement items) && categoryTitle != "Episodes";
    }
}