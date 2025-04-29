using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.RegularExpressions;
using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Internal;
using YouTubeMusicAPI.Internal.Parsers;
using YouTubeMusicAPI.Models.Info;
using YouTubeMusicAPI.Models.Library;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Models.Streaming;

namespace YouTubeMusicAPI.Client;

/// <summary>
/// Client for searching on YouTube Music
/// </summary>
public class YouTubeMusicClient
{
    readonly ILogger? logger;
    readonly RequestHelper requestHelper;
    readonly YouTubeMusicBase baseClient;

    readonly bool isCookieAuthenticated;

    Player? player = null;

    /// <summary>
    /// Creates a new search client
    /// </summary>
    /// <param name="geographicalLocation">The region for the payload</param>
    /// <param name="visitorData">The persistent visitor data used for session tailoring</param>
    /// <param name="poToken">The Proof of Origin Token for attestation (may be required for streaming)</param>
    /// <param name="cookies">Initial cookies used for authentication</param>
    /// <param name="httpClient">Http client which handles sending requests</param>
    public YouTubeMusicClient(
        string geographicalLocation = "US",
        string? visitorData = null,
        string? poToken = null,
        IEnumerable<Cookie>? cookies = null,
        HttpClient? httpClient = null)
    {
        GeographicalLocation = geographicalLocation;
        VisitorData = visitorData;
        PoToken = poToken;

        isCookieAuthenticated = cookies is not null;

        this.requestHelper = new(httpClient ?? new(), cookies);
        this.baseClient = new(requestHelper);

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
    /// <param name="httpClient">Http client which handles sending requests</param>
    public YouTubeMusicClient(
        ILogger logger,
        string geographicalLocation = "US",
        string? visitorData = null,
        string? poToken = null,
        IEnumerable<Cookie>? cookies = null,
        HttpClient? httpClient = null)
    {
        GeographicalLocation = geographicalLocation;
        VisitorData = visitorData;
        PoToken = poToken;

        isCookieAuthenticated = cookies is not null;

        this.logger = logger;
        this.requestHelper = new(httpClient ?? new(), cookies);
        this.baseClient = new(logger, requestHelper);

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
    public string? PoToken
    {
        get => poToken;
        set
        {
            poToken = value;
            
            if (player is not null)
                player.PoToken = value;
        }
    }
    string? poToken = null;


    /// <summary>
    /// Searches for a query on YouTube Music
    /// </summary>
    /// <param name="query">The query to search for</param>
    /// <param name="category">The category of items to search for</param>
    /// <returns>An array of shelves containing all search results</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public PaginatedAsyncEnumerable<SearchResult> SearchAsync(
        string query,
        SearchCategory? category = null)
    {
        // Prepare request
        if (string.IsNullOrWhiteSpace(query))
        {
            logger?.LogError($"[YouTubeMusicClient-SearchAsync] Search failed. Query parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(query), "Search failed. Query parameter is null or whitespace.");
        }

        async Task<Page<SearchResult>> FetchPageDelegate(
            string? nextContinuationToken,
            CancellationToken cancelToken = default)
        {
            // Send request
            Dictionary<string, object> payload = Payload.WebRemix(GeographicalLocation, VisitorData, PoToken, null,
            [
                ("query", query),
                ("params", category.ToParams()),
                ("continuation", nextContinuationToken)
            ]);

            JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Search, payload, cancelToken);

            // Parse request response
            Page<SearchResult> page = SearchParser.GetPage(requestResponse);
            return page;
        }
        return new(FetchPageDelegate);
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

            string s = requestResponse.ToString();

            // Parse request response
            CommunityPlaylistInfo info = InfoParser.GetCommunityPlaylist(requestResponse);
            return info;
        }
        catch
        {
            logger?.LogWarning($"[YouTubeMusicClient-GetCommunityPlaylistInfoAsync] Primary Browse endpoint failed. Falling back to Next endpoint.");

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
    /// Gets the songs of a community playlist on YouTube Music
    /// </summary>
    /// <param name="browseId">The brwose id of the community playlist</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public PaginatedAsyncEnumerable<CommunityPlaylistSong> GetCommunityPlaylistSongsAsync(
        string browseId)
    {
        // Prepare request
        if (string.IsNullOrWhiteSpace(browseId))
        {
            logger?.LogError($"[YouTubeMusicClient-GetCommunityPlaylistInfoAsync] Getting info failed. Browse id parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(browseId), "Getting info failed. Browse id parameter is null or whitespace.");
        }

        bool useFallback = false;

        async Task<Page<CommunityPlaylistSong>> FetchPageDelegate(
            string? continuationToken,
            CancellationToken cancelToken = default)
        {
            if (!useFallback)
                try
                {
                    // Send request
                    Dictionary<string, object> payload = Payload.WebRemix(GeographicalLocation, VisitorData, PoToken, null,
                        [
                            ("browseId", browseId),
                            ("continuation", continuationToken)
                        ]);
                    JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Browse, payload, cancelToken);

                    // Parse request response
                    Page<CommunityPlaylistSong> page = InfoParser.GetCommunityPlaylistSongsPage(requestResponse);
                    return page;
                }
                catch // Use fallback path (shits prob an infinite auto generated playlist)
                {
                    logger?.LogWarning($"[YouTubeMusicClient-GetCommunityPlaylistSongsAsync] Primary Browse endpoint failed. Falling back to Next endpoint.");
                    useFallback = true;
                }

            // Send request
            Dictionary<string, object> fallbackPayload = Payload.WebRemix(GeographicalLocation, VisitorData, PoToken, null,
                [
                    ("playlistId", browseId.StartsWith("VL") ? browseId.Substring(2) : browseId),
                    ("continuation", continuationToken)
                ]);
            JObject fallbackRequestResponse = await baseClient.SendRequestAsync(Endpoints.Next, fallbackPayload, cancelToken);

            // Parse request response
            Page<CommunityPlaylistSong> fallbackPage = InfoParser.GetCommunityPlaylistSimpleSongsPage(fallbackRequestResponse);
            return fallbackPage;
        }
        return new(FetchPageDelegate);
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
            JToken[]? runs = itemToken.SelectObjectOptional<JToken[]>("musicTwoRowItemRenderer.subtitle.runs");
            if (runs is null || runs.Length < 3)
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
            if (menuItems is null || menuItems.Length == 1)
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

        // Get VisitorData if necessary
        if (VisitorData is null)
        {
            logger?.LogInformation("[YouTubeMusicClient-GetStreamingDataAsync] Getting required visitor data for streaming...");
            VisitorData = await baseClient.GetVisitorDataAsync(cancellationToken);
        }

        if (!isCookieAuthenticated)
        {
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
        else // Mobile client does not support cookie authentication -> falling back to WebRemix client
        {
            if (player is null)
            {
                logger?.LogInformation("[YouTubeMusicClient-GetStreamingDataAsync] Creating required player for streaming...");
                player = await Player.CreateAsync(requestHelper, PoToken, cancellationToken);
            }

            // Send request
            Dictionary<string, object> payload = Payload.WebRemix(GeographicalLocation, VisitorData, PoToken, player.SigTimestamp,
                [
                    ("videoId", id)
                ]);
            JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Player, payload, cancellationToken);

            // Parse request response
            StreamingData streamingData = StreamingParser.GetData(requestResponse, player);
            return streamingData;
        }
    }
}