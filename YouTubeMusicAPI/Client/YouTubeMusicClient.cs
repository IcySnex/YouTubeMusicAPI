using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.RegularExpressions;
using YouTubeMusicAPI.Internal;
using YouTubeMusicAPI.Internal.Parsers;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Info;
using YouTubeMusicAPI.Models.Library;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Models.Streaming;
using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Client;

/// <summary>
/// Client for searching on YouTube Music
/// </summary>
public class YouTubeMusicClient
{
    readonly ILogger? logger;
    readonly YouTubeMusicBase baseClient;

    /// <summary>
    /// Creates a new search client
    /// </summary>
    /// <param name="geographicalLocation">The region for the payload</param>
    /// <param name="visitorData">The persistent visitor data used for session tailoring</param>
    /// <param name="poToken">The Proof of Origin Token for attestation (may be required for streaming)</param>
    /// <param name="cookies">Initial cookies used for authentication</param>
    public YouTubeMusicClient(
        string geographicalLocation = "US",
        string? visitorData = null,
        string? poToken = null,
        IEnumerable<Cookie>? cookies = null)
    {
        GeographicalLocation = geographicalLocation;
        VisitorData = visitorData;
        PoToken = poToken;

        this.baseClient = new(cookies);

        logger?.LogInformation($"[YouTubeMusicClient-.ctor] YouTubeMusicClient has been initialized.");
    }

    /// <summary>
    /// Creates a new search client with extendended logging functions
    /// </summary>
    /// <param name="logger">The optional logger used for logging</param>
    /// <param name="geographicalLocation">The region for the payload</param>
    /// <param name="visitorData">The persistent visitor data used for session tailoring</param>
    /// <param name="poToken">The Proof of Origin Token for attestation (may be required for streaming)</param>
    /// <param name="cookies">Initial cookies used for authentication</param>
    public YouTubeMusicClient(
        ILogger logger,
        string geographicalLocation = "US",
        string? visitorData = null,
        string? poToken = null,
        IEnumerable<Cookie>? cookies = null)
    {
        GeographicalLocation = geographicalLocation;
        VisitorData = visitorData;
        PoToken = poToken;

        this.logger = logger;
        this.baseClient = new(logger, cookies);

        logger?.LogInformation($"[YouTubeMusicClient-.ctor] YouTubeMusicClient with extendended logging functions has been initialized.");
    }


    /// <summary>
    /// The geographical location
    /// </summary>
    public string GeographicalLocation { get; set; }

    /// <summary>
    /// The persistent visitor data used for session tailoring
    /// </summary>
    public string? VisitorData { get; set; }

    /// <summary>
    /// The Proof of Origin Token for attestation (may be required for streaming)
    /// </summary>
    public string? PoToken { get; set; }


    /// <summary>
    /// Gets the YouTube Music item kind based on the type of the item
    /// </summary>
    /// <typeparam name="T">The requested YouTube Music Item type</typeparam>
    /// <returns>The shelf kind</returns>
    YouTubeMusicItemKind GetSearchResultKind<T>() where T : IYouTubeMusicItem =>
        typeof(T) switch
        {
            Type type when type == typeof(SongSearchResult) => YouTubeMusicItemKind.Songs,
            Type type when type == typeof(VideoSearchResult) => YouTubeMusicItemKind.Videos,
            Type type when type == typeof(AlbumSearchResult) => YouTubeMusicItemKind.Albums,
            Type type when type == typeof(CommunityPlaylistSearchResult) => YouTubeMusicItemKind.CommunityPlaylists,
            Type type when type == typeof(ArtistSearchResult) => YouTubeMusicItemKind.Artists,
            Type type when type == typeof(PodcastSearchResult) => YouTubeMusicItemKind.Podcasts,
            Type type when type == typeof(EpisodeSearchResult) => YouTubeMusicItemKind.Episodes,
            Type type when type == typeof(ProfileSearchResult) => YouTubeMusicItemKind.Profiles,
            _ => YouTubeMusicItemKind.Unknown
        };

    /// <summary>
    /// Parses a search request response into shelf results
    /// </summary>
    /// <param name="requestResponse">The request response to parse</param>
    /// <returns>An array of shelves containing all items</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    IEnumerable<Shelf> ParseSearchResponse(
        JObject requestResponse)
    {
        List<Shelf> results = [];

        // Get shelves
        logger?.LogInformation($"[YouTubeMusicClient-ParseSearchResponse] Getting shelves from search response.");
        bool isContinued = requestResponse.ContainsKey("continuationContents");

        IEnumerable<JToken>? shelvesData = isContinued
            ? requestResponse
                .SelectToken("continuationContents")
            : requestResponse
                .SelectToken("contents.tabbedSearchResultsRenderer.tabs[0].tabRenderer.content.sectionListRenderer.contents")
                ?.Where(token => token["musicShelfRenderer"] is not null)
                ?.Select(token => token.First!);

        if (shelvesData is null || !shelvesData.Any())
        {
            logger?.LogWarning($"[YouTubeMusicClient-ParseSearchResponse] Parsing search failed. Request response does not contain any shelves.");
            return [];
        }

        foreach (JToken? shelfData in shelvesData)
        {
            JToken? shelfDataObject = shelfData.First;
            if (shelfDataObject is null)
                continue;

            // Parse info from shelf data
            string? nextContinuationToken = shelfDataObject.SelectObjectOptional<string>("continuations[0].nextContinuationData.continuation");

            string? category = isContinued
                ? requestResponse
                    .SelectToken("header.musicHeaderRenderer.header.chipCloudRenderer.chips")
                    ?.FirstOrDefault(token => token.SelectObjectOptional<bool>("chipCloudChipRenderer.isSelected"))
                    ?.SelectObjectOptional<string>("chipCloudChipRenderer.uniqueId")
                : shelfDataObject
                    .SelectObjectOptional<string>("title.runs[0].text");
            JToken[] shelfItems = shelfDataObject.SelectObjectOptional<JToken[]>("contents") ?? [];

            YouTubeMusicItemKind kind = category.ToShelfKind();
            Func<JToken, IYouTubeMusicItem>? getShelfItem = kind switch
            {
                YouTubeMusicItemKind.Songs => SearchParser.GetSong,
                YouTubeMusicItemKind.Videos => SearchParser.GetVideo,
                YouTubeMusicItemKind.Albums => SearchParser.GetAlbums,
                YouTubeMusicItemKind.CommunityPlaylists => SearchParser.GetCommunityPlaylist,
                YouTubeMusicItemKind.Artists => SearchParser.GetArtist,
                YouTubeMusicItemKind.Podcasts => SearchParser.GetPodcast,
                YouTubeMusicItemKind.Episodes => SearchParser.GetEpisode,
                YouTubeMusicItemKind.Profiles => SearchParser.GetProfile,
                _ => null
            };

            List<IYouTubeMusicItem> items = [];
            if (getShelfItem is not null)
                foreach (JToken shelfItem in shelfItems)
                {
                    // Parse shelf item
                    JToken? itemObject = shelfItem.First?.First;

                    if (itemObject is null)
                        continue;

                    items.Add(getShelfItem(itemObject));
                }

            // Create shelf
            Shelf shelf = new(nextContinuationToken, [.. items], kind);
            results.Add(shelf);
        }

        return results;
    }



    /// <summary>
    /// Searches for a query on YouTube Music
    /// </summary>
    /// <param name="query">The query to search for</param>
    /// <param name="continuationToken">The continuation token to get further elemnts from a pervious search</param>
    /// <param name="kind">The kind of items to search for</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>An array of shelves containing all search results</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<IEnumerable<Shelf>> SearchAsync(
        string query,
        string? continuationToken = null,
        YouTubeMusicItemKind? kind = null,
        CancellationToken cancellationToken = default)
    {
        // Prepare request
        if (string.IsNullOrWhiteSpace(query))
        {
            logger?.LogError($"[YouTubeMusicClient-SearchAsync] Search failed. Query parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(query), "Search failed. Query parameter is null or whitespace.");
        }

        // Send request
        Dictionary<string, object> payload = Payload.WebRemix(GeographicalLocation, VisitorData, PoToken, null,
            [
                ("query", query),
                ("params", kind.ToParams()),
                ("continuation", continuationToken)
            ]);
        JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Search, payload, cancellationToken);

        // Parse request response
        IEnumerable<Shelf> searchResults = ParseSearchResponse(requestResponse);
        return kind is null || kind == YouTubeMusicItemKind.Unknown ? searchResults : searchResults.Where(searchResult => searchResult.Kind == kind);
    }

    /// <summary>
    /// Searches for a specfic shelf for a query on YouTube Music
    /// </summary>
    /// <param name="query">The query to search for</param>
    /// <param name="limit">The limit of items to return</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>An array of the specific shelf items</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<IEnumerable<T>> SearchAsync<T>(
        string query,
        int limit = 20,
        CancellationToken cancellationToken = default) where T : IYouTubeMusicItem
    {
        YouTubeMusicItemKind kind = GetSearchResultKind<T>();

        // All items requested (does not support continuation)
        if (kind == YouTubeMusicItemKind.Unknown)
        {
            IEnumerable<Shelf> allShelves = await SearchAsync(query, null, kind, cancellationToken);

            return allShelves
                .SelectMany(shelf => shelf.Items)
                .Take(limit)
                .Cast<T>();
        }

        // Special items requested
        List<T> searchResults = [];

        string? nextContinuationToken = null;
        bool hasMoreResults = true;

        while (searchResults.Count < limit && hasMoreResults)
        {
            IEnumerable<Shelf> currentShelf = await SearchAsync(query, nextContinuationToken, kind, cancellationToken);

            Shelf? requestedShelf = currentShelf.FirstOrDefault(shelf => shelf.Kind == kind);
            if (requestedShelf is null)
            {
                logger?.LogWarning($"[YouTubeMusicClient-SearchAsync] Search results do not cotain requested filtered shelf.");
                break;
            }

            searchResults.AddRange(requestedShelf.Items.Cast<T>());

            nextContinuationToken = requestedShelf.NextContinuationToken;
            hasMoreResults = !string.IsNullOrEmpty(nextContinuationToken);
        }

        return searchResults.Take(limit);
    }


    /// <summary>
    /// Gets the browse id for an album used for getting information
    /// </summary>
    /// <param name="id">The id of the album</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>The browse id of the album</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<string> GetAlbumBrowseIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        // Prepare request
        if (string.IsNullOrWhiteSpace(id))
        {
            logger?.LogError($"[YouTubeMusicClient-GetAlbumBrowseIdAsync] Getting album browse id failed. Id parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(id), "Getting album browse id failed. Id parameter is null or whitespace.");
        }

        if (id.StartsWith("MPRE"))
            return id;

        (string key, string? value)[] parameters =
        [
            ("list", id)
        ];

        // Send request
        string requestResponse = await baseClient.GetWebContentAsync(Endpoints.Playlist, parameters, cancellationToken);

        // Parse request response
        Match match = Regex.Match(Regex.Unescape(requestResponse), "\"MPRE.+?\"");
        if (!match.Success)
        {
            logger?.LogError($"[YouTubeMusicClient-GetAlbumBrowseIdAsync] Getting album browse id failed. Found no match.");
            throw new Exception("Getting album browse id failed. Found no match.");
        }

        return match.Value.Trim('"');
    }

    /// <summary>
    /// Gets the browse id for an playlist used for getting information
    /// </summary>
    /// <param name="id">The id of the playlist</param>
    /// <returns>The browse id of the playlist</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    public string GetCommunityPlaylistBrowseId(
        string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            logger?.LogError($"[YouTubeMusicClient-GetCommunityPlaylistBrowseId] Getting community playlist browse id failed. Id parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(id), "Getting community playlist browse id failed. Id parameter is null or whitespace.");
        }

        if (id.StartsWith("VL"))
            return id;

        return $"VL{id}";
    }


    /// <summary>
    /// Gets the information about a song or video on YouTube Music
    /// </summary>
    /// <param name="id">The id of the song or video</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>The song or video info</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<SongVideoInfo> GetSongVideoInfoAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        // Prepare request
        if (string.IsNullOrWhiteSpace(id))
        {
            logger?.LogError($"[YouTubeMusicClient-GetSongVideoInfoAsync] Getting info failed. Id parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(id), "Getting info failed. Id parameter is null or whitespace.");
        }

        // Send requests
        Dictionary<string, object> payload = Payload.WebRemix(GeographicalLocation, VisitorData, PoToken, null,
            [
                ("videoId", id)
            ]);
        JObject playerRequestResponse = await baseClient.SendRequestAsync(Endpoints.Player, payload, cancellationToken);
        JObject nextRequestResponse = await baseClient.SendRequestAsync(Endpoints.Next, payload, cancellationToken);

        // Parse request response
        SongVideoInfo info = InfoParser.GetSongVideo(playerRequestResponse, nextRequestResponse);
        return info;
    }


    /// <summary>
    /// Gets the information about an album on YouTube Music
    /// </summary>
    /// <param name="browseId">The brwose id of the album</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>The album info</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<AlbumInfo> GetAlbumInfoAsync(
        string browseId,
        CancellationToken cancellationToken = default)
    {
        // Prepare request
        if (string.IsNullOrWhiteSpace(browseId))
        {
            logger?.LogError($"[YouTubeMusicClient-GetAlbumInfoAsync] Getting info failed. Browse id parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(browseId), "Getting info failed. Browse id parameter is null or whitespace.");
        }

        // Send request
        Dictionary<string, object> payload = Payload.WebRemix(GeographicalLocation, VisitorData, PoToken, null,
            [
                ("browseId", browseId)
            ]);
        JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Browse, payload, cancellationToken);

        // Parse request response
        AlbumInfo info = InfoParser.GetAlbum(requestResponse);
        return info;
    }

    /// <summary>
    /// Gets the information about a community playlist on YouTube Music
    /// </summary>
    /// <param name="browseId">The brwose id of the community playlist</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>The community playlist info</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<CommunityPlaylistInfo> GetCommunityPlaylistInfoAsync(
        string browseId,
        CancellationToken cancellationToken = default)
    {
        // Prepare request
        if (string.IsNullOrWhiteSpace(browseId))
        {
            logger?.LogError($"[YouTubeMusicClient-GetCommunityPlaylistInfoAsync] Getting info failed. Browse id parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(browseId), "Getting info failed. Browse id parameter is null or whitespace.");
        }

        try
        {
            // Send request
            Dictionary<string, object> payload = Payload.WebRemix(GeographicalLocation, VisitorData, PoToken, null,
                [
                    ("browseId", browseId)
                ]);
            JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Browse, payload, cancellationToken);

            // Parse request response
            CommunityPlaylistInfo info = InfoParser.GetCommunityPlaylist(requestResponse);
            return info;
        }
        catch (HttpRequestException ex)
        {
            if (ex.Message != "HTTP request failed. StatusCode: NotFound.")
                throw;

            // Send request
            Dictionary<string, object> payload = Payload.WebRemix(GeographicalLocation, VisitorData, PoToken, null,
                [
                    ("playlistId", browseId.StartsWith("VL") ? browseId.Substring(2) : browseId)
                ]);
            JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Next, payload, cancellationToken);

            // Parse request response
            CommunityPlaylistInfo info = InfoParser.GetCommunityPlaylistSimple(requestResponse);
            return info;
        }
    }

    /// <summary>
    /// Gets the information about a community playlist on YouTube Music
    /// </summary>
    /// <param name="browseId">The brwose id of the community playlist</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>The community playlist info</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<ArtistInfo> GetArtistInfoAsync(
        string browseId,
        CancellationToken cancellationToken = default)
    {
        // Prepare request
        if (string.IsNullOrWhiteSpace(browseId))
        {
            logger?.LogError($"[YouTubeMusicClient-GetArtistInfoAsync] Getting info failed. Browse id parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(browseId), "Getting info failed. Browse id parameter is null or whitespace.");
        }

        // Send request
        Dictionary<string, object> payload = Payload.WebRemix(GeographicalLocation, VisitorData, PoToken, null,
            [
                ("browseId", browseId)
            ]);
        JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Browse, payload, cancellationToken);

        // Parse request response
        ArtistInfo info = InfoParser.GetArtist(requestResponse);
        return info;
    }


    /// <summary>
    /// Gets all saved community playlists for the currently authenticated user on YouTube Music
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>The community playlists</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<IEnumerable<LibraryCommunityPlaylist>> GetLibraryCommunityPlaylistsAsync(
        CancellationToken cancellationToken = default)
    {
        // Send request
        Dictionary<string, object> payload = Payload.WebRemix(GeographicalLocation, VisitorData, PoToken, null,
            [
                ("browseId", "FEmusic_liked_playlists")
            ]);
        JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Browse, payload, cancellationToken);

        // Get items
        JObject[] itemTokens = requestResponse.SelectObjectOptional<JObject[]>("contents.singleColumnBrowseResultsRenderer.tabs[0].tabRenderer.content.sectionListRenderer.contents[0].gridRenderer.items") ?? [];

        // Parse request response
        List<LibraryCommunityPlaylist> items = [];
        foreach (JObject itemToken in itemTokens)
        {
            JToken[]? menuItems = itemToken.SelectObjectOptional<JToken[]>("musicTwoRowItemRenderer.menu.menuRenderer.items");
            if (menuItems is null || menuItems.Length < 6)
                continue;

            items.Add(LibraryParser.GetCommunityPlaylist(itemToken));
        }

        return items;
    }

    /// <summary>
    /// Gets all saved songs for the currently authenticated user on YouTube Music
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>The songs</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<IEnumerable<LibrarySong>> GetLibrarySongsAsync(
        CancellationToken cancellationToken = default)
    {
        // Send request
        Dictionary<string, object> payload = Payload.WebRemix(GeographicalLocation, VisitorData, PoToken, null,
            [
                ("browseId", "FEmusic_liked_videos")
            ]);
        JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Browse, payload, cancellationToken);

        // Get items
        JObject[]? itemTokens = requestResponse.SelectObjectOptional<JObject[]>("contents.singleColumnBrowseResultsRenderer.tabs[0].tabRenderer.content.sectionListRenderer.contents[0].musicShelfRenderer.contents");
        if (itemTokens is null || itemTokens.Length < 2)
            return [];

        // Parse request response
        List<LibrarySong> items = [];
        for (int i = 1; i < itemTokens.Length; i++)
            items.Add(LibraryParser.GetSong(itemTokens[i]));

        return items;
    }

    /// <summary>
    /// Gets all saved albums for the currently authenticated user on YouTube Music
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>The albums</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<IEnumerable<LibraryAlbum>> GetLibraryAlbumsAsync(
        CancellationToken cancellationToken = default)
    {
        // Send request
        Dictionary<string, object> payload = Payload.WebRemix(GeographicalLocation, VisitorData, PoToken, null,
            [
                ("browseId", "FEmusic_liked_albums")
            ]);
        JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Browse, payload, cancellationToken);

        // Get items
        JObject[] itemTokens = requestResponse.SelectObjectOptional<JObject[]>("contents.singleColumnBrowseResultsRenderer.tabs[0].tabRenderer.content.sectionListRenderer.contents[0].gridRenderer.items") ?? [];

        // Parse request response
        List<LibraryAlbum> items = [];
        foreach (JObject itemToken in itemTokens)
            items.Add(LibraryParser.GetAlbum(itemToken));

        return items;
    }

    /// <summary>
    /// Gets all artists with saved songs for the currently authenticated user on YouTube Music
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>The artists</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<IEnumerable<LibraryArtist>> GetLibraryArtistsAsync(
        CancellationToken cancellationToken = default)
    {
        // Send request
        Dictionary<string, object> payload = Payload.WebRemix(GeographicalLocation, VisitorData, PoToken, null,
            [
                ("browseId", "FEmusic_library_corpus_track_artists")
            ]);
        JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Browse, payload, cancellationToken);

        // Get items
        JObject[] itemTokens = requestResponse.SelectObjectOptional<JObject[]>("contents.singleColumnBrowseResultsRenderer.tabs[0].tabRenderer.content.sectionListRenderer.contents[0].musicShelfRenderer.contents") ?? [];

        // Parse request response
        List<LibraryArtist> items = [];
        foreach (JObject itemToken in itemTokens)
            items.Add(LibraryParser.GetArtist(itemToken));

        return items;
    }

    /// <summary>
    /// Gets all subscribed artists for the currently authenticated user on YouTube Music
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>The artists</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<IEnumerable<LibrarySubscription>> GetLibrarySubscriptionsAsync(
        CancellationToken cancellationToken = default)
    {
        // Send request
        Dictionary<string, object> payload = Payload.WebRemix(GeographicalLocation, VisitorData, PoToken, null,
            [
                ("browseId", "FEmusic_library_corpus_artists")
            ]);
        JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Browse, payload, cancellationToken);

        // Get items
        JObject[] itemTokens = requestResponse.SelectObjectOptional<JObject[]>("contents.singleColumnBrowseResultsRenderer.tabs[0].tabRenderer.content.sectionListRenderer.contents[0].musicShelfRenderer.contents") ?? [];

        // Parse request response
        List<LibrarySubscription> items = [];
        foreach (JObject itemToken in itemTokens)
            items.Add(LibraryParser.GetSubscription(itemToken));

        return items;
    }

    /// <summary>
    /// Gets all saved podcasts for the currently authenticated user on YouTube Music
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>The podcasts</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<IEnumerable<LibraryPodcast>> GetLibraryPodcastsAsync(
        CancellationToken cancellationToken = default)
    {
        // Send request
        Dictionary<string, object> payload = Payload.WebRemix(GeographicalLocation, VisitorData, PoToken, null,
            [
                ("browseId", "FEmusic_library_non_music_audio_list")
            ]);
        JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Browse, payload, cancellationToken);

        // Get items
        JObject[] itemTokens = requestResponse.SelectObjectOptional<JObject[]>("contents.singleColumnBrowseResultsRenderer.tabs[0].tabRenderer.content.sectionListRenderer.contents[0].gridRenderer.items") ?? [];

        // Parse request response
        List<LibraryPodcast> items = [];
        foreach (JObject itemToken in itemTokens)
        {
            JToken[]? menuItems = itemToken.SelectObjectOptional<JToken[]>("musicTwoRowItemRenderer.menu.menuRenderer.items");
            if (menuItems is null || menuItems.Length != 2)
                continue;

            items.Add(LibraryParser.GetPodcast(itemToken));
        }

        return items;
    }


    /// <summary>
    /// Gets the streaming data of a song or video on YouTube Music
    /// </summary>
    /// <param name="id">The id of the song or video</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>The song or video streaming data</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<StreamingData> GetStreamingDataAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        // Prepare request
        if (string.IsNullOrWhiteSpace(id))
        {
            logger?.LogError($"[YouTubeMusicClient-GetStreamingDataAsync] Getting streaming data failed. Id parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(id), "Getting streaming data failed. Id parameter is null or whitespace.");
        }

        // Send requests
        Dictionary<string, object> payload = Payload.Mobile(GeographicalLocation, VisitorData, PoToken,
            [
                ("videoId", id)
            ]);
        JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Player, payload, cancellationToken);

        // Parse request response
        StreamingData streamingData = StreamingParser.GetData(requestResponse);
        return streamingData;
    }
}