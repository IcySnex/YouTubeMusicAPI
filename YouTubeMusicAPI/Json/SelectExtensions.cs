using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Json;

internal static class SelectExtensions
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
            .Get("watchEndpoint")
            .Get("videoId")
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
        JElement navigationEndpoint = element
            .AsArray()
            .Or(JArray.Empty)
            .FirstOrDefault(item => item
                .Get("menuNavigationItemRenderer")
                .SelectRunTextAt("text", 0)
                .Is("Start radio"))
            .Get("menuNavigationItemRenderer")
            .Get("navigationEndpoint");

        JElement watchEndpoint = navigationEndpoint
            .Get("watchEndpoint");
        if (watchEndpoint.IsUndefined)
            watchEndpoint = navigationEndpoint
                .Get("watchPlaylistEndpoint");

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
                .Is("•")
                .Not())
            .Where(item => item
                .Get("text")
                .AsString()
                .OrThrow()
                .Trim()
                .Is("&", ",")
                .Not())
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
            .IsNull()
            .Not();
}