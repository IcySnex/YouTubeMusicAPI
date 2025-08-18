using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Json;

internal static class Selectors
{
    public static string? SelectRunTextAt(
        this JElement element,
        string key,
        int index) =>
        element
            .Get(key)
            .Get("runs")
            .GetAt(index)
            .Get("text")
            .AsString();

    public static JElement SelectMenu(
        this JElement element) =>
        element
            .Get("menu")
            .Get("menuRenderer")
            .Get("items");


    public static JElement SelectWatchEndpoint(
        this JElement element)
    {
        JElement watchEndpoint = element
            .Get("watchEndpoint");

        return watchEndpoint.IsUndefined
            .If(true,
                element
                    .Get("watchPlaylistEndpoint"),
                watchEndpoint);
    }


    public static string? SelectNavigationBrowseId(
        this JElement element) =>
        element
            .Get("navigationEndpoint")
            .Get("browseEndpoint")
            .Get("browseId")
            .AsString();

    public static string? SelectNavigationVideoId(
        this JElement element) =>
        element
            .Get("navigationEndpoint")
            .SelectWatchEndpoint()
            .Get("videoId")
            .AsString();

    public static string? SelectNavigationPlaylistId(
        this JElement element) =>
        element
            .Get("navigationEndpoint")
            .SelectWatchEndpoint()
            .Get("playlistId")
            .AsString();


    public static string? SelectOverlayVideoId(
        this JElement element) =>
        element
            .Get("musicItemThumbnailOverlayRenderer")
            .Get("content")
            .Get("musicPlayButtonRenderer")
            .Get("playNavigationEndpoint")
            .SelectWatchEndpoint()
            .Get("videoId")
            .AsString();

    public static string? SelectOverlayPlaylistId(
        this JElement element) =>
        element
            .Get("musicItemThumbnailOverlayRenderer")
            .Get("content")
            .Get("musicPlayButtonRenderer")
            .Get("playNavigationEndpoint")
            .SelectWatchEndpoint()
            .Get("playlistId")
            .AsString();


    public static string? SelectTapBrowseId(
        this JElement element) =>
        element
            .Get("onTap")
            .Get("browseEndpoint")
            .Get("browseId")
            .AsString();


    public static Thumbnail[] SelectThumbnails(
        this JElement element,
        string key = "thumbnail") =>
        element
            .Get(key)
            .Get("thumbnails")
            .AsArray()
            .OrThrow()
            .Select(Thumbnail.Parse)
            .ToArray();


    public static Radio? SelectRadio(
        this JElement element)
    {
        JElement watchEndpoint = element
            .AsArray()
            .Or(JArray.Empty)
            .FirstOrDefault(item => item
                .Get("menuNavigationItemRenderer")
                .SelectRunTextAt("text", 0)
                .Is("Start radio"))
            .Get("menuNavigationItemRenderer")
            .Get("navigationEndpoint")
            .SelectWatchEndpoint();

        string? playlistId = watchEndpoint
            .Get("playlistId")
            .AsString();
        if (playlistId is null)
            return null;

        string? songVideoId = watchEndpoint
            .Get("videoId")
            .AsString();

        return new(playlistId, songVideoId);
    }


    public static YouTubeMusicEntity SelectArtist(
        this JElement element)
    {
        string name = element
            .Get("text")
            .AsString()
            .OrThrow();
        string? id = element
            .SelectNavigationBrowseId();

        return new(name, id, id);
    }

    public static YouTubeMusicEntity[] SelectArtists(
        this JElement element,
        int startIndex = 0) =>
        element
            .AsArray()
            .OrThrow()
            .Skip(startIndex)
            .TakeWhile(item => item
                .Get("text")
                .AsString()
                .OrThrow()
                .Trim()
                .IsNot("•"))
            .Where(item => item
                .Get("text")
                .AsString()
                .OrThrow()
                .Trim()
                .IsNot("&", ","))
            .Select(SelectArtist)
            .ToArray();


    public static YouTubeMusicEntity SelectAlbum(
        this JElement element)
    {
        string name = element
            .Get("text")
            .AsString()
            .OrThrow();
        string? browseId = element
            .SelectNavigationBrowseId();

        return new(name, null, browseId);
    }

    public static YouTubeMusicEntity SelectAlbumUnknown(
        this JElement element)
    {
        string? browseId = element
            .AsArray()
            .Or(JArray.Empty)
            .FirstOrDefault(item => item
                .Get("menuNavigationItemRenderer")
                .SelectRunTextAt("text", 0)
                .Is("Go to album"))
            .Get("menuNavigationItemRenderer")
            .SelectNavigationBrowseId();

        return new("N/A", null, browseId);
    }


    public static YouTubeMusicEntity SelectPodcast(
        this JElement element)
    {
        string name = element
            .Get("text")
            .AsString()
            .OrThrow();
        string? browseId = element
            .SelectNavigationBrowseId();

        return new(name, browseId?.Substring(4), browseId);
    }


    public static bool SelectIsExplicit(
        this JElement element,
        string key = "badges") =>
        element
            .Get(key)
            .AsArray()
            .Or(JArray.Empty)
            .Any(item => item
                .Get("musicInlineBadgeRenderer")
                .Get("icon")
                .Get("iconType")
                .AsString()
                .Is("MUSIC_EXPLICIT_BADGE"));

    public static bool SelectIsCreditsAvailable(
        this JElement element) =>
        element
            .AsArray()
            .Or(JArray.Empty)
            .FirstOrDefault(item => item
                .Get("menuNavigationItemRenderer")
                .SelectRunTextAt("text", 0)
                .Is("View song credits"))
            .Get("menuNavigationItemRenderer")
            .SelectNavigationBrowseId()
            .IsNotNull();

    public static string? SelectPlaylistId(
        this JElement element) =>
        element
            .AsArray()
            .Or(JArray.Empty)
            .FirstOrDefault(item => item
                .Get("menuNavigationItemRenderer")
                .SelectRunTextAt("text", 0)
                .Is("Shuffle play"))
            .Get("menuNavigationItemRenderer")
            .SelectNavigationPlaylistId();
}