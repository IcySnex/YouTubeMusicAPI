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
        this JElement element) =>
        element
            .Coalesce(
                item => item.Get("watchEndpoint"),
                item => item.Get("watchPlaylistEndpoint"));


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


    public static bool SelectIsPodcast(
        this JElement element)
    {
        JArray menu = element
            .SelectMenu()
            .AsArray()
            .OrThrow(); // Soomething went wrong!! normally I just skip and use fallback values but not here.

        if (menu.Length == 1)
            return false; // Only profiles have a single item, so it's not an episode

        foreach (JElement item in menu)
        {
            // If menu contains unique video/song items, it's not an episode
            bool hasStartRadio = item
                .Get("menuNavigationItemRenderer")
                .SelectRunTextAt("text", 0)
                .Is("Start radio");
            if (hasStartRadio)
                return false;

            bool hasArtist = item
                .Get("menuNavigationItemRenderer")
                .SelectRunTextAt("text", 0)
                .Is("Go to artist");
            if (hasArtist)
                return false;

            bool hasAddToRemoveFromLiked = item
                .Get("toggleMenuServiceItemRenderer")
                .SelectRunTextAt("defaultText", 0)
                .Is("Add to liked songs", "Remove from liked songs");
            if (hasAddToRemoveFromLiked)
                return false;

            // If menu contains unique playlist/album/artist/podcast items, it's not an episode
            bool hasShufflePlay = item
                .Get("menuNavigationItemRenderer")
                .SelectRunTextAt("text", 0)
                .Is("Shuffle play");
            if (hasShufflePlay)
                return false;

            bool hasSaveToRemoveFromLibrary = item
                .Get("toggleMenuServiceItemRenderer")
                .SelectRunTextAt("defaultText", 0)
                .Is("Save to library", "Remove from library");
            if (hasSaveToRemoveFromLibrary)
                return false;


            // If menu contains unique episode items, it's an episode
            bool hasSaveRemoveEpisodeOrMarkAsPlayedUnplayed = item
                .Get("toggleMenuServiceItemRenderer")
                .SelectRunTextAt("defaultText", 0)
                .Is("Save episode for later", "Remove from ", "Mark as played", "Mark as unplayed");
            if (hasSaveRemoveEpisodeOrMarkAsPlayedUnplayed)
                return true;

            bool hasGoToPodcast = item
                .Get("menuNavigationItemRenderer")
                .SelectRunTextAt("text", 0)
                .Is("Go to podcast");
            if (hasGoToPodcast)
                return true;
        }

        // If none of the unique video/song items were found, treat it as an episode. BETTER SAFE THAN SORRY!
        return true;
    }

    public static bool SelectIsFeaturedPlaylist(
        this JElement element)
    {
        JElement descriptionRuns = element
            .Coalesce(
                item => item
                    .Get("subtitle"),
                item => item
                    .Get("flexColumns")
                    .GetAt(1)
                    .Get("musicResponsiveListItemFlexColumnRenderer")
                    .Get("text"))
            .Get("runs");

        string type = descriptionRuns
            .GetAt(0)
            .Get("text")
            .AsString()
            .OrThrow();
        if (type == "Mix")
            return true;

        int descriptionStartIndex = type
            .If("Playlist", 2, 0);


        JElement creatorRun = descriptionRuns
            .GetAt(descriptionStartIndex);

        string name = creatorRun
            .Get("text")
            .AsString()
            .OrThrow();
        if (name != "YouTube Music")
            return false;

        string? id = creatorRun
            .SelectNavigationBrowseId();
        return id is null;
    }
}