using System.Text.Json;
using YouTubeMusicAPI.Models;

namespace YouTubeMusicAPI.Utils;

/// <summary>
/// Contains extension methods to select specific data from JSON elements.
/// </summary>
internal static class Selectors
{
    public static string SelectRunText(
        this JsonElement runs,
        int index) =>
        runs
            .GetPropertyAt(index)
            .GetProperty("text")
            .GetString()
            .OrThrow();

    public static string SelectRunTextAt(
        this JsonElement element,
        string propertyName,
        int index) =>
        element
            .GetProperty(propertyName)
            .GetProperty("runs")
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
        string propertyName = "thumbnail") => [];
        //element
        //    .GetProperty(propertyName)
        //    .GetProperty("thumbnails")
        //    .EnumerateArray()
        //    .Select(Thumbnail.Parse)
        //    .ToArray();

    public static Radio? SelectRadioOrNull(
        this JsonElement element)
    {
        JsonElement? watchEndpoint = element
            .EnumerateArray()
            .FirstOrDefault(item =>
                item.TryGetProperty("menuNavigationItemRenderer", out JsonElement menu) &&
                menu.SelectRunTextAt("text", 0) == "Start radio")
            .AsNullable()
            ?.GetProperty("menuNavigationItemRenderer")
            .GetPropertyOrNull("navigationEndpoint")
            ?.Coalesce(
                item => item.GetPropertyOrNull("watchEndpoint"),
                item => item.GetPropertyOrNull("watchPlaylistEndpoint"));

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

    public static string? SelectPlaylistIdOrNull(
        this JsonElement element) =>
        element
            .EnumerateArray()
            .FirstOrDefault(item =>
                item.TryGetProperty("menuNavigationItemRenderer", out JsonElement menu) &&
                menu.SelectRunTextAt("text", 0) == "Shuffle play")
            .AsNullable()
            ?.GetProperty("menuNavigationItemRenderer")
            .GetPropertyOrNull("navigationEndpoint")
            ?.GetPropertyOrNull("watchPlaylistEndpoint")
            ?.GetPropertyOrNull("playlistId")
            ?.GetString();

    public static bool SelectContainsExplicitBadge(
        this JsonElement element,
        string propertyName = "badges") =>
        (
            element
                .GetPropertyOrNull(propertyName)
                ?.EnumerateArray()
                .Any(item =>
                    item.GetPropertyOrNull("musicInlineBadgeRenderer")
                        ?.GetPropertyOrNull("icon")
                        ?.GetPropertyOrNull("iconType")
                        ?.GetString() == "MUSIC_EXPLICIT_BADGE")
        )
        .Or(false);


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
        int startIndex = 0) =>
        element
            .EnumerateArray()
            .Skip(startIndex)
            .TakeWhile(item => item
                .GetProperty("text")
                .GetString()
                .OrThrow()
                .Trim() != "•")
            .Where(item => item
                .GetProperty("text")
                .GetString()
                .OrThrow()
                .Trim() is string text && text != "&" && text != ",")
            .Select(SelectArtist)
            .ToArray();


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
        string? browseId = element
            .EnumerateArray()
            .FirstOrDefault(item =>
                item.TryGetProperty("menuNavigationItemRenderer", out JsonElement menu) &&
                menu.SelectRunTextAt("text", 0) == "Go to album")
            .AsNullable()
            ?.GetProperty("menuNavigationItemRenderer")
            .GetPropertyOrNull("navigationEndpoint")
            ?.GetPropertyOrNull("browseEndpoint")
            ?.GetPropertyOrNull("browseId")
            ?.GetString();

        return new("N/A", null, browseId);
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
        this JsonElement element) =>
        element
            .EnumerateArray()
            .FirstOrDefault(item =>
                item.TryGetProperty("menuNavigationItemRenderer", out JsonElement menu) &&
                menu.SelectRunTextAt("text", 0) == "View song credits")
            .AsNullable()
            ?.GetPropertyOrNull("navigationEndpoint")
            ?.GetPropertyOrNull("browseEndpoint")
            ?.GetPropertyOrNull("browseId")
            ?.GetString() is not null;


    public static bool SelectIsPodcastEvenThoItShouldnt(
        this JsonElement element,
        string categoryTitle) =>
        element
            .GetProperty("menu")
            .GetProperty("menuRenderer")
            .TryGetProperty("topLevelButtons", out JsonElement _) && categoryTitle != "Episodes";
}