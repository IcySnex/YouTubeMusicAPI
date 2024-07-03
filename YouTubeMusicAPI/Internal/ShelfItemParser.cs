using Newtonsoft.Json.Linq;
using System.Globalization;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Shelf;
using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Internal;

/// <summary>
/// Contains methods to parse shelf items from json tokens
/// </summary>
internal static class ShelfItemParser
{
    /// <summary>
    /// Parses data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>An unknown shelf item</returns>
    public static ShelfItem GetUnknown(
        JToken jsonToken)
    {
        // Parse info from json token
        string? name = jsonToken.SelectToken("flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text")?.ToString();
        string? id = jsonToken.SelectToken("navigationEndpoint.browseEndpoint.browseId")?.ToString();

        // Return result
        return new(
            name!,
            id!,
            ShelfKind.Unknown);
    }


    /// <summary>
    /// Parses song data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>A shelf song</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static Song GetSong(
        JToken jsonToken)
    {
        // Parse info from json token
        ShelfItem[] artists = Parser.GetArtists(jsonToken, "flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs", 3);
        Thumbnail[] thumbnails = Parser.GetThumbnails(jsonToken);

        string? name = jsonToken.SelectToken("flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text")?.ToString();
        string? id = jsonToken.SelectToken("overlay.musicItemThumbnailOverlayRenderer.content.musicPlayButtonRenderer.playNavigationEndpoint.watchEndpoint.videoId")?.ToString();
        
        int albumIndex = artists[0].Id is null ? 2 : (artists.Length * 2);
        string? album = jsonToken.SelectToken($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{albumIndex}].text")?.ToString();
        string? albumId = jsonToken.SelectToken($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{albumIndex}].navigationEndpoint.browseEndpoint.browseId")?.ToString();

        int durationIndex = artists[0].Id is null ? 4 : ((artists.Length * 2) + 2);
        string? duration = jsonToken.SelectToken($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{durationIndex}].text")?.ToString();
        string? plays = jsonToken.SelectToken("flexColumns[2].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text")?.ToString();
        string? isExplicit = jsonToken.SelectToken("badges.musicInlineBadgeRenderer.accessibilityData.accessibilityData.label")?.ToString();

        string? radioPlaylistId = jsonToken.SelectToken("menu.menuRenderer.items[0].menuNavigationItemRenderer.navigationEndpoint.watchEndpoint.playlistId")?.ToString();
        string? radioVideoId = jsonToken.SelectToken("menu.menuRenderer.items[0].menuNavigationItemRenderer.navigationEndpoint.watchEndpoint.videoId")?.ToString();

        if (name is null || id is null || album is null || albumId is null || duration is null || plays is null || radioPlaylistId is null)
            throw new ArgumentNullException(null, "One or more values of item is null");

        // Return result
        return new(
            name,
            id,
            artists,
            new(album, albumId, ShelfKind.Albums),
            Parser.TimeSpanExact(duration),
            isExplicit == "Explicit",
            plays,
            new(radioPlaylistId, radioVideoId),
            thumbnails);
    }

    /// <summary>
    /// Parses video data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>A shelf video</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static Video GetVideo(
        JToken jsonToken)
    {
        // Parse runs from json token
        JToken[]? runs = (jsonToken.SelectToken("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs")?.ToObject<JToken[]>()) ?? throw new ArgumentNullException(null, "One or more values of item is null");
        int runsIndex = runs.Length == 7 ? 2 : 0;

        // Parse info from json token
        Thumbnail[] thumbnails = Parser.GetThumbnails(jsonToken);

        string? name = jsonToken.SelectToken("flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text")?.ToString();
        string? id = jsonToken.SelectToken("overlay.musicItemThumbnailOverlayRenderer.content.musicPlayButtonRenderer.playNavigationEndpoint.watchEndpoint.videoId")?.ToString();

        string? channel = jsonToken.SelectToken($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex}].text")?.ToString();
        string? channelId = jsonToken.SelectToken($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex}].navigationEndpoint.browseEndpoint.browseId")?.ToString();

        string? duration = jsonToken.SelectToken($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex + 4}].text")?.ToString();
        string? views = jsonToken.SelectToken($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex + 2}].text")?.ToString();

        string? radioPlaylistId = jsonToken.SelectToken("menu.menuRenderer.items[0].menuNavigationItemRenderer.navigationEndpoint.watchEndpoint.playlistId")?.ToString();
        string? radioVideoId = jsonToken.SelectToken("menu.menuRenderer.items[0].menuNavigationItemRenderer.navigationEndpoint.watchEndpoint.videoId")?.ToString();

        if (name is null || id is null || channel is null || duration is null || views is null || radioPlaylistId is null)
            throw new ArgumentNullException(null, "One or more values of item is null");

        // Return result
        return new(
            name,
            id,
            new(channel, channelId, ShelfKind.Artists),
            Parser.TimeSpanExact(duration),
            views,
            new(radioPlaylistId, radioVideoId),
            thumbnails);
    }

    /// <summary>
    /// Parses album data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>A shelf album</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static Album GetAlbums(
        JToken jsonToken)
    {
        // Parse info from json token
        ShelfItem[] artists = Parser.GetArtists(jsonToken, "flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs", 2, 1);
        Thumbnail[] thumbnails = Parser.GetThumbnails(jsonToken);

        string? name = jsonToken.SelectToken("flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text")?.ToString();
        string? id = jsonToken.SelectToken("overlay.musicItemThumbnailOverlayRenderer.content.musicPlayButtonRenderer.playNavigationEndpoint.watchPlaylistEndpoint.playlistId")?.ToString();

        int yearIndex = artists[0].Id is null ? 4 : (artists.Length * 2) + 2;
        string? year = jsonToken.SelectToken($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{yearIndex}].text")?.ToString();
        string? isSingle = jsonToken.SelectToken("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text")?.ToString();

        string? radioPlaylistId = jsonToken.SelectToken("menu.menuRenderer.items[1].menuNavigationItemRenderer.navigationEndpoint.watchPlaylistEndpoint.playlistId")?.ToString();

        if (name is null || id is null || year is null || isSingle is null || radioPlaylistId is null)
            throw new ArgumentNullException(null, "One or more values of item is null");

        // Return result
        return new(
            name,
            id,
            artists,
            int.Parse(year),
            isSingle == "Single",
            new(radioPlaylistId, null),
            thumbnails);
    }

    /// <summary>
    /// Parses community playlist data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>A shelf community playlist</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static CommunityPlaylist GetCommunityPlaylist(
        JToken jsonToken)
    {
        // Parse runs from json token
        JToken[]? runs = (jsonToken.SelectToken("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs")?.ToObject<JToken[]>()) ?? throw new ArgumentNullException(null, "One or more values of item is null");
        int runsIndex = runs.Length == 5 ? 2 : 0;

        // Parse info from json token
        Thumbnail[] thumbnails = Parser.GetThumbnails(jsonToken);

        string? name = jsonToken.SelectToken("flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text")?.ToString();
        string? id = jsonToken.SelectToken("overlay.musicItemThumbnailOverlayRenderer.content.musicPlayButtonRenderer.playNavigationEndpoint.watchPlaylistEndpoint.playlistId")?.ToString();

        string? creator = jsonToken.SelectToken($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex}].text")?.ToString();
        string? creatorId = jsonToken.SelectToken($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex}].navigationEndpoint.browseEndpoint.browseId")?.ToString();

        string? views = jsonToken.SelectToken($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex + 2}].text")?.ToString();

        string? radioPlaylistId = jsonToken.SelectToken("menu.menuRenderer.items[1].menuNavigationItemRenderer.navigationEndpoint.watchPlaylistEndpoint.playlistId")?.ToString();

        if (name is null || id is null || creator is null || views is null || radioPlaylistId is null)
            throw new ArgumentNullException(null, "One or more values of item is null");

        // Return result
        return new(
            name,
            id,
            new(creator, creatorId, ShelfKind.Artists),
            views,
            new(radioPlaylistId, null),
            thumbnails);
    }

    /// <summary>
    /// Parses artist data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>A shelf artist</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static Artist GetArtist(
        JToken jsonToken)
    {
        // Parse info from json token
        Thumbnail[] thumbnails = Parser.GetThumbnails(jsonToken);

        string? name = jsonToken.SelectToken("flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text")?.ToString();
        string? id = jsonToken.SelectToken("navigationEndpoint.browseEndpoint.browseId")?.ToString();
        string? type = jsonToken.SelectToken("navigationEndpoint.browseEndpoint.browseEndpointContextSupportedConfigs.browseEndpointContextMusicConfig.pageType")?.ToString();

        string? subscribers = jsonToken.SelectToken("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[2].text")?.ToString();

        string? radioPlaylistId = jsonToken.SelectToken("menu.menuRenderer.items[1].menuNavigationItemRenderer.navigationEndpoint.watchPlaylistEndpoint.playlistId")?.ToString();

        if (name is null || id is null || type is null || subscribers is null || radioPlaylistId is null)
            throw new ArgumentNullException(null, "One or more values of item is null");

        // Return result
        return new(
            name,
            id,
            subscribers,
            new(radioPlaylistId, null),
            thumbnails);
    }

    /// <summary>
    /// Parses podcast data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>A shelf podcast</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static Podcast GetPodcast(
        JToken jsonToken)
    {
        // Parse runs from json token
        JToken[]? runs = (jsonToken.SelectToken("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs")?.ToObject<JToken[]>()) ?? throw new ArgumentNullException(null, "One or more values of item is null");
        int runsIndex = runs.Length == 3 ? 2 : 0;

        // Parse info from json token
        Thumbnail[] thumbnails = Parser.GetThumbnails(jsonToken);

        string? name = jsonToken.SelectToken("flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text")?.ToString();
        string? id = jsonToken.SelectToken("overlay.musicItemThumbnailOverlayRenderer.content.musicPlayButtonRenderer.playNavigationEndpoint.watchPlaylistEndpoint.playlistId")?.ToString();

        string? host = jsonToken.SelectToken($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex}].text")?.ToString();
        string? hostId = jsonToken.SelectToken($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex}].navigationEndpoint.browseEndpoint.browseId")?.ToString();

        if (name is null || id is null || host is null)
            throw new ArgumentNullException(null, "One or more values of item is null");

        // Return result
        return new(
            name,
            id,
            new(host, hostId, ShelfKind.Artists),
            thumbnails);
    }

    /// <summary>
    /// Parses podcast episode data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>A shelf podcast episode</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static Episode GetEpisode(
        JToken jsonToken)
    {
        // Parse runs from json token
        JToken[]? runs = (jsonToken.SelectToken("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs")?.ToObject<JToken[]>()) ?? throw new ArgumentNullException(null, "One or more values of item is null");
        int runsIndex = runs.Length == 5 ? 2 : 0;

        // Parse info from json token
        Thumbnail[] thumbnails = Parser.GetThumbnails(jsonToken);

        string? name = jsonToken.SelectToken("flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text")?.ToString();
        string? id = jsonToken.SelectToken("overlay.musicItemThumbnailOverlayRenderer.content.musicPlayButtonRenderer.playNavigationEndpoint.watchEndpoint.videoId")?.ToString();

        string? podcast = jsonToken.SelectToken($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex + 2}].text")?.ToString();
        string? podcastId = jsonToken.SelectToken($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex + 2}].navigationEndpoint.browseEndpoint.browseId")?.ToString();

        string? releasedAt = jsonToken.SelectToken($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex}].text")?.ToString();
        string? isLikesAllowed = jsonToken.SelectToken("menu.menuRenderer.topLevelButtons[0].likeButtonRenderer.likesAllowed")?.ToString();

        if (name is null || id is null || podcast is null || releasedAt is null || isLikesAllowed is null)
            throw new ArgumentNullException(null, "One or more values of item is null");

        // Return result
        return new(
            name,
            id,
            new(podcast, podcastId, ShelfKind.Podcasts),
            DateTime.Parse(releasedAt),
            bool.Parse(isLikesAllowed),
            thumbnails);
    }

    /// <summary>
    /// Parses profile data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>A shelf profile</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static Profile GetProfile(
        JToken jsonToken)
    {
        // Parse info from json token
        Thumbnail[] thumbnails = Parser.GetThumbnails(jsonToken);

        string? name = jsonToken.SelectToken("flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text")?.ToString();
        string? id = jsonToken.SelectToken("navigationEndpoint.browseEndpoint.browseId")?.ToString();

        string? handle = jsonToken.SelectToken("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[2].text")?.ToString();

        if (name is null || id is null || handle is null)
            throw new ArgumentNullException(null, "One or more values of item is null");

        // Return result
        return new(
            name,
            id,
            handle,
            thumbnails);
    }
}