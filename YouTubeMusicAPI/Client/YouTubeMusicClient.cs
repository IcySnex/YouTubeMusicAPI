using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using YouTubeMusicAPI.Internal;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Info;
using YouTubeMusicAPI.Models.Shelf;
using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Client;

/// <summary>
/// Client for searching on YouTube Music
/// </summary>
public class YouTubeMusicClient
{
    readonly ILogger? logger;
    readonly YouTubeMusicBase baseClient;

    readonly string hostLanguage;
    readonly string geographicalLocation;

    /// <summary>
    /// Creates a new search client
    /// </summary>
    /// <param name="hostLanguage">The language for the payload</param>
    /// <param name="geographicalLocation">The region for the payload</param>
    public YouTubeMusicClient(
        string hostLanguage = "en",
        string geographicalLocation = "US")
    {
        this.hostLanguage = hostLanguage;
        this.geographicalLocation = geographicalLocation;

        this.baseClient = new();

        logger?.LogInformation($"[YouTubeMusicClient-.ctor] YouTubeMusicClient has been initialized.");
    }

    /// <summary>
    /// Creates a new search client with extendended logging functions
    /// </summary>
    /// <param name="logger">The optional logger used for logging</param>
    /// <param name="hostLanguage">The language for the payload</param>
    /// <param name="geographicalLocation">The region for the payload</param>
    public YouTubeMusicClient(
        ILogger logger,
        string hostLanguage = "en",
        string geographicalLocation = "US")
    {
        this.hostLanguage = hostLanguage;
        this.geographicalLocation = geographicalLocation;

        this.logger = logger;
        this.baseClient = new(logger);

        logger?.LogInformation($"[YouTubeMusicClient-.ctor] YouTubeMusicClient with extendended logging functions has been initialized.");
    }


    /// <summary>
    /// Gets the shelf kind based on the type of the shelf item
    /// </summary>
    /// <typeparam name="T">The requested shelf type</typeparam>
    /// <returns>The shelf kind</returns>
    ShelfKind GetShelfKind<T>() where T : IShelfItem =>
        typeof(T) switch
        {
            Type type when type == typeof(Song) => ShelfKind.Songs,
            Type type when type == typeof(Video) => ShelfKind.Videos,
            Type type when type == typeof(Album) => ShelfKind.Albums,
            Type type when type == typeof(CommunityPlaylist) => ShelfKind.CommunityPlaylists,
            Type type when type == typeof(Artist) => ShelfKind.Artists,
            Type type when type == typeof(Podcast) => ShelfKind.Podcasts,
            Type type when type == typeof(Episode) => ShelfKind.Episodes,
            Type type when type == typeof(Profile) => ShelfKind.Profiles,
            _ => ShelfKind.Unknown
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
        logger?.LogInformation($"[YouTubeMusicClient-ParseSearchResponse] Getting shelves.");
        IEnumerable<JProperty> shelvesData = requestResponse
            .DescendantsAndSelf()
            .OfType<JProperty>()
            .Where(prop => prop.Name == "musicShelfRenderer");

        if (shelvesData is null || !shelvesData.Any())
        {
            logger?.LogError($"[YouTubeMusicClient-ParseSearchResponse] Parsing search failed. Request response does not contain any shelves.");
            throw new ArgumentNullException(nameof(shelvesData), "Parsing search failed. Request response does not contain any shelves.");
        }

        foreach (JProperty shelfData in shelvesData)
        {
            // Parse shelf data
            logger?.LogInformation($"[YouTubeMusicClient-ParseSearchResponse] Parsing shelf data: {shelfData.Path}.");
            JToken? shelfDataObject = shelfData.First;

            if (shelfDataObject is null)
                continue;

            // Parse info from shelf data
            string query = shelfDataObject.SelectToken("bottomEndpoint.searchEndpoint.query")?.ToString() ?? string.Empty;
            string @params = shelfDataObject.SelectToken("bottomEndpoint.searchEndpoint.params")?.ToString() ?? string.Empty;

            string? category = shelfDataObject.SelectToken("title.runs[0].text")?.ToString();
            JToken shelfItems = shelfDataObject.SelectToken("contents") ?? new JArray();

            ShelfKind kind = category.ToShelfKind();

            List<IShelfItem> items = [];
            foreach (JToken shelfItem in shelfItems)
            {
                // Parse shelf item
                logger?.LogInformation($"[YouTubeMusicClient-ParseSearchResponse] Parsing shelf item: {shelfItem.Path}.");
                JToken? itemObject = shelfItem.First?.First;

                if (itemObject is null)
                    continue;

                // Parse info from shelf item
                IShelfItem item = kind switch
                {
                    ShelfKind.Songs => ShelfItemParser.GetSong(itemObject),
                    ShelfKind.Videos => ShelfItemParser.GetVideo(itemObject),
                    ShelfKind.Albums => ShelfItemParser.GetAlbums(itemObject),
                    ShelfKind.CommunityPlaylists => ShelfItemParser.GetCommunityPlaylist(itemObject),
                    ShelfKind.Artists => ShelfItemParser.GetArtist(itemObject),
                    ShelfKind.Podcasts => ShelfItemParser.GetPodcast(itemObject),
                    ShelfKind.Episodes => ShelfItemParser.GetEpisode(itemObject),
                    ShelfKind.Profiles => ShelfItemParser.GetProfile(itemObject),
                    _ => ShelfItemParser.GetUnknown(itemObject),

                };
                items.Add(item);
            }

            // Create shelf
            Shelf shelf = new(query, @params, [.. items], kind);
            results.Add(shelf);
        }

        return results;
    }



    /// <summary>
    /// Searches for a query on YouTube Music
    /// </summary>
    /// <param name="query">The query to search for</param>
    /// <param name="kind">The shelf kind of items to search for</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>An array of shelves containing all search results</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<IEnumerable<Shelf>> SearchAsync(
        string query,
        ShelfKind? kind = null,
        CancellationToken cancellationToken = default)
    {
        // Prepare request
        if (string.IsNullOrWhiteSpace(query))
        {
            logger?.LogError($"[YouTubeMusicClient-SearchAsync] Search failed. Query parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(query), "Search failed. Query parameter is null or whitespace.");
        }

        (string key, object? value)[] payload =
        [
            ("query", query),
            ("params", kind.ToParams())
        ];

        // Send request
        JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Search, payload, hostLanguage, geographicalLocation, cancellationToken);

        // Parse request response
        IEnumerable<Shelf> searchResults = ParseSearchResponse(requestResponse);
        return searchResults;
    }

    /// <summary>
    /// Searches for a specfic shelf for a query on YouTube Music
    /// </summary>
    /// <param name="query">The query to search for</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>An array of the specific shelf items</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<IEnumerable<T>> SearchAsync<T>(
        string query,
        CancellationToken cancellationToken = default) where T : IShelfItem
    {
        ShelfKind kind = GetShelfKind<T>();

        // Send request
        IEnumerable<Shelf> searchResults = await SearchAsync(query, kind, cancellationToken);

        if (kind == ShelfKind.Unknown)
        {
            logger?.LogInformation($"[YouTubeMusicClient-SearchSongsAsync] Concatenating shelf items.");
            IEnumerable<T> allShelfItems = searchResults.SelectMany(shelf => shelf.Items).Cast<T>();

            return allShelfItems;
        }

        // Get songs shelf
        Shelf? songsResults = searchResults.FirstOrDefault(shelf => shelf.Kind == kind);

        if (songsResults is null)
        {
            logger?.LogError($"[YouTubeMusicClient-SearchSongsAsync] Search failed. Search results do not cotain requested filtered shelf.");
            throw new ArgumentNullException(nameof(songsResults), "Search failed. Search results do not cotain requested filtered shelf.");
        }

        logger?.LogInformation($"[YouTubeMusicClient-SearchSongsAsync] Casting shelf items.");
        IEnumerable<T> singleShelfItems = songsResults.Items.Cast<T>();

        return singleShelfItems;
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

        return $"Vl{id}";
    }


    /// <summary>
    /// Gets the information about a song on YouTube Music
    /// </summary>
    /// <param name="id">The id of the song</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>The song info</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<SongInfo> GetSongInfoAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        // Prepare request
        if (string.IsNullOrWhiteSpace(id))
        {
            logger?.LogError($"[YouTubeMusicClient-GetSongInfoAsync] Getting info failed. Id parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(id), "Getting info failed. Id parameter is null or whitespace.");
        }

        (string key, object? value)[] payload =
        [
            ("video_id", id)
        ];

        // Send request
        JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Player, payload, hostLanguage, geographicalLocation, cancellationToken);

        // Parse request response
        SongInfo info = InfoParser.GetSong(requestResponse);
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

        (string key, object? value)[] payload =
        [
            ("browseId", browseId)
        ];

        // Send request
        JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Browse, payload, hostLanguage, geographicalLocation, cancellationToken);

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

        (string key, object? value)[] payload =
        [
            ("browseId", browseId)
        ];

        // Send request
        JObject requestResponse = await baseClient.SendRequestAsync(Endpoints.Browse, payload, hostLanguage, geographicalLocation, cancellationToken);

        // Parse request response
        CommunityPlaylistInfo info = InfoParser.GetCommunityPlaylist(requestResponse);
        return info;
    }
}