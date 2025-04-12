using Newtonsoft.Json.Linq;
using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Internal.Parsers;

/// <summary>
/// Contains methods to parse shelf items from json tokens
/// </summary>
internal static class SearchParser
{
    /// <summary>
    /// Parses page data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the page data</param>
    /// <returns>A shelf song</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static Page<SearchResult> GetPage(
        JObject jsonToken)
    {
        // Get shelves
        bool isContinued = jsonToken.ContainsKey("continuationContents");

        JToken?[]? shelves = isContinued
            ? jsonToken
                .SelectToken("continuationContents")
                ?.ToArray()
            : jsonToken
                .SelectToken("contents.tabbedSearchResultsRenderer.tabs[0].tabRenderer.content.sectionListRenderer.contents")
                ?.Where(token => token["musicShelfRenderer"] is not null)
                ?.Select(token => token.First)
                ?.ToArray();

        if (shelves is null || shelves.Length == 0)
            return new([], null);

        // Parse shelves
        List<SearchResult> items = [];
        string? nextContinuationToken = null;

        foreach (JToken? shelf in shelves)
        {
            if (shelf?.First is not JToken shelfToken)
                continue;

            nextContinuationToken = shelfToken.SelectObjectOptional<string>("continuations[0].nextContinuationData.continuation");

            string? category = isContinued
                ? jsonToken
                    .SelectToken("header.musicHeaderRenderer.header.chipCloudRenderer.chips")
                    ?.FirstOrDefault(token => token.SelectObjectOptional<bool>("chipCloudChipRenderer.isSelected"))
                    ?.SelectObjectOptional<string>("chipCloudChipRenderer.uniqueId")
                : shelfToken
                    .SelectObjectOptional<string>("title.runs[0].text");
            JToken[] shelfItems = shelfToken.SelectObjectOptional<JToken[]>("contents") ?? [];

            // Shelf item parse function
            Func<JToken, SearchResult>? getShelfItem = category.ToSearchCategory() switch
            {
                SearchCategory.Songs => GetSong,
                SearchCategory.Videos => GetVideo,
                SearchCategory.Albums => GetAlbums,
                SearchCategory.CommunityPlaylists => GetCommunityPlaylist,
                SearchCategory.Artists => GetArtist,
                SearchCategory.Podcasts => GetPodcast,
                SearchCategory.Episodes => GetEpisode,
                SearchCategory.Profiles => GetProfile,
                _ => null
            };
            if (getShelfItem is null)
                continue;

            foreach (JToken shelfItem in shelfItems)
            {
                // Parse shelf item
                JToken? itemObject = shelfItem.First?.First;

                if (itemObject is null)
                    continue;

                items.Add(getShelfItem(itemObject));
            }
        }

        return new(items, nextContinuationToken);
    }


    /// <summary>
    /// Parses song data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>A shelf song</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static SongSearchResult GetSong(
        JToken jsonToken)
    {
        JToken[] runs = jsonToken.SelectObject<JToken[]>("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs");

        NamedEntity[] artists = jsonToken.SelectArtists("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs", 0, jsonToken.SelectObjectOptional<string>($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[1].text") != " & " && runs.Length == 3 ? 1 : 3);
        int albumIndex = artists[0].Id is null ? 2 : artists.Length * 2;

        string? radioPlaylistId = jsonToken.SelectObjectOptional<string>("menu.menuRenderer.items[0].menuNavigationItemRenderer.navigationEndpoint.watchEndpoint.playlistId");
        
        return new(
            name: jsonToken.SelectObject<string>("flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text"),
            id: jsonToken.SelectObject<string>("overlay.musicItemThumbnailOverlayRenderer.content.musicPlayButtonRenderer.playNavigationEndpoint.watchEndpoint.videoId"),
            artists: artists,
            album: albumIndex == runs.Length - 1 ? new(jsonToken.SelectObject<string>("flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text"), null) : jsonToken.SelectNamedEntity($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{albumIndex}].text", $"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{albumIndex}].navigationEndpoint.browseEndpoint.browseId"),
            duration: jsonToken.SelectObject<string>($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runs.Length - 1}].text").ToTimeSpan(),
            isExplicit: jsonToken.SelectIsExplicit("badges"),
            playsInfo: jsonToken.SelectObject<string>("flexColumns[2].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text"),
            radio: radioPlaylistId is null ? null : new(radioPlaylistId, jsonToken.SelectObjectOptional<string>("menu.menuRenderer.items[0].menuNavigationItemRenderer.navigationEndpoint.watchEndpoint.videoId")),
            thumbnails: jsonToken.SelectThumbnails());
    }

    /// <summary>
    /// Parses video data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>A shelf video</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static VideoSearchResult GetVideo(
        JToken jsonToken)
    {
        int runsCount = jsonToken.SelectObject<JToken[]>("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs").Length;
        int runsIndex = runsCount == 7 || runsCount == 3 ? 2 : 0;

        string? radioPlaylistId = jsonToken.SelectObjectOptional<string>("menu.menuRenderer.items[4].menuNavigationItemRenderer.navigationEndpoint.browseEndpoint.browseId");

        return new(
            name: jsonToken.SelectObject<string>("flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text"),
            id: jsonToken.SelectObject<string>("overlay.musicItemThumbnailOverlayRenderer.content.musicPlayButtonRenderer.playNavigationEndpoint.watchEndpoint.videoId"),
            artist: jsonToken.SelectNamedEntity($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex}].text", $"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex}].navigationEndpoint.browseEndpoint.browseId"),
            duration: (jsonToken.SelectObjectOptional<string>($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex + 4}].text") ?? "00:00").ToTimeSpanLong(),
            viewsInfo: jsonToken.SelectObjectOptional<string?>($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex + 2}].text") ?? "0 views",
            radio: runsCount == 3 ? radioPlaylistId is null ? null : new(radioPlaylistId, null) : jsonToken.SelectRadio(),
            thumbnails: jsonToken.SelectThumbnails());
    }

    /// <summary>
    /// Parses album data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>A shelf album</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static AlbumSearchResult GetAlbums(
        JToken jsonToken)
    {
        JToken[] runs = jsonToken.SelectObject<JToken[]>("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs");

        NamedEntity[] artists = jsonToken.SelectArtists("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs", 2, jsonToken.SelectObjectOptional<JToken>($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runs.Length - 1}].navigationEndpoint") is null ? 1 : 0);
        int yearIndex = artists[0].Id is null ? 4 : artists.Length * 2 + 2;

        return new(
            name: jsonToken.SelectObject<string>("flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text"),
            jsonToken.SelectObjectOptional<string>("overlay.musicItemThumbnailOverlayRenderer.content.musicPlayButtonRenderer.playNavigationEndpoint.watchPlaylistEndpoint.playlistId") ?? jsonToken.SelectObject<string>("overlay.musicItemThumbnailOverlayRenderer.content.musicPlayButtonRenderer.playNavigationEndpoint.watchEndpoint.playlistId"),
            artists: artists,
            releaseYear: yearIndex == runs.Length - 1 ? jsonToken.SelectObject<int>($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{yearIndex}].text") : 1970,
            isSingle: jsonToken.SelectObject<string>("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text") == "Single",
            isEp: jsonToken.SelectObject<string>("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text") == "EP",
            radio: jsonToken.SelectRadio("menu.menuRenderer.items[1].menuNavigationItemRenderer.navigationEndpoint.watchPlaylistEndpoint.playlistId", null),
            thumbnails: jsonToken.SelectThumbnails());
    }

    /// <summary>
    /// Parses community playlist data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>A shelf community playlist</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static CommunityPlaylistSearchResult GetCommunityPlaylist(
        JToken jsonToken)
    {
        JToken[]? runs = jsonToken.SelectObject<JToken[]>("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs");
        int runsIndex = runs.Length == 5 ? 2 : 0;

        return new(
            name: jsonToken.SelectObject<string>("flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text"),
            id: jsonToken.SelectObject<string>("navigationEndpoint.browseEndpoint.browseId").Substring(2),
            creator: jsonToken.SelectNamedEntity($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex}].text", $"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex}].navigationEndpoint.browseEndpoint.browseId"),
            viewsInfo: jsonToken.SelectObject<string>($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex + 2}].text"),
            radio: jsonToken.SelectObjectOptional<string>("menu.menuRenderer.items[1].menuNavigationItemRenderer.navigationEndpoint.watchPlaylistEndpoint.playlistId") is string radioPlaylistId ? new(radioPlaylistId, null) : null,
            thumbnails: jsonToken.SelectThumbnails());
    }

    /// <summary>
    /// Parses artist data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>A shelf artist</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static ArtistSearchResult GetArtist(
        JToken jsonToken)
    {
        return new(
            name: jsonToken.SelectObject<string>("flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text"),
            id: jsonToken.SelectObject<string>("navigationEndpoint.browseEndpoint.browseId"),
            subscribersInfo: jsonToken.SelectObject<string>("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[2].text"),
            radio: jsonToken.SelectRadio("menu.menuRenderer.items[1].menuNavigationItemRenderer.navigationEndpoint.watchPlaylistEndpoint.playlistId", null),
            thumbnails: jsonToken.SelectThumbnails());
    }

    /// <summary>
    /// Parses podcast data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>A shelf podcast</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static PodcastSearchResult GetPodcast(
        JToken jsonToken)
    {
        // Parse runs from json token
        JToken[]? runs = jsonToken.SelectObject<JToken[]>("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs");
        int runsIndex = runs.Length == 3 ? 2 : 0;

        return new(
            name: jsonToken.SelectObject<string>("flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text"),
            id: jsonToken.SelectObject<string>("overlay.musicItemThumbnailOverlayRenderer.content.musicPlayButtonRenderer.playNavigationEndpoint.watchPlaylistEndpoint.playlistId"),
            host: jsonToken.SelectNamedEntity($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex}].text", $"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex}].navigationEndpoint.browseEndpoint.browseId"),
            thumbnails: jsonToken.SelectThumbnails());
    }

    /// <summary>
    /// Parses podcast episode data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>A shelf podcast episode</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static EpisodeSearchResult GetEpisode(
        JToken jsonToken)
    {
        JToken[]? runs = jsonToken.SelectObject<JToken[]>("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs");
        int runsIndex = runs.Length == 5 ? 2 : 0;

        return new(
            name: jsonToken.SelectObject<string>("flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text"),
            id: jsonToken.SelectObject<string>("overlay.musicItemThumbnailOverlayRenderer.content.musicPlayButtonRenderer.playNavigationEndpoint.watchEndpoint.videoId"),
            podcast: jsonToken.SelectNamedEntity($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex + 2}].text", $"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex + 2}].navigationEndpoint.browseEndpoint.browseId"),
            releasedAt: jsonToken.SelectDateTime($"flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[{runsIndex}].text"),
            isLikesAllowed: jsonToken.SelectObject<bool>("menu.menuRenderer.topLevelButtons[0].likeButtonRenderer.likesAllowed"),
            thumbnails: jsonToken.SelectThumbnails());
    }

    /// <summary>
    /// Parses profile data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>A shelf profile</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static ProfileSearchResult GetProfile(
        JToken jsonToken)
    {
        return new(
            name: jsonToken.SelectObject<string>("flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text"),
            id: jsonToken.SelectObject<string>("navigationEndpoint.browseEndpoint.browseId"),
            handle: jsonToken.SelectObject<string>("flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs[2].text"),
            thumbnails: jsonToken.SelectThumbnails());
    }
}