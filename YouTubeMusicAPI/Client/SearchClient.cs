using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using YouTubeMusicAPI.Internal;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Shelf;
using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Client;

/// <summary>
/// Client for searching on YouTube Music
/// </summary>
public class SearchClient
{
    readonly ILogger? logger;
    readonly BaseClient baseClient;

    /// <summary>
    /// Creates a new search client
    /// </summary>
    public SearchClient()
    {
        this.baseClient = new();

        logger?.LogInformation($"[SearchClient-.ctor] SearchClient has been initialized.");
    }

    /// <summary>
    /// Creates a new search client with extendended logging functions
    /// </summary>
    /// <param name="logger">The optional logger used for logging</param>
    public SearchClient(
        ILogger logger)
    {
        this.logger = logger;
        this.baseClient = new(logger);

        logger?.LogInformation($"[SearchClient-.ctor] SearchClient with extendended logging functions has been initialized.");
    }


    /// <summary>
    /// Gets the shelf kind based on the type of the shelf item
    /// </summary>
    /// <typeparam name="IShelfItem">The requested shelf type</typeparam>
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
    /// Parses a request response into shelf results
    /// </summary>
    /// <param name="requestResponse">The request response to parse</param>
    /// <returns>An array of shelves containing all items</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    public IEnumerable<Shelf> Parse(
        JObject requestResponse)
    {
        List<Shelf> results = [];

        // Get shelves
        logger?.LogInformation($"[SearchClient-Parse] Getting shelves.");
        IEnumerable<JProperty> shelvesData = requestResponse
            .DescendantsAndSelf()
            .OfType<JProperty>()
            .Where(prop => prop.Name == "musicShelfRenderer");

        if (shelvesData is null || !shelvesData.Any())
        {
            logger?.LogError($"[SearchClient-Parse] Parsing search failed. Request response does not contain any shelves.");
            throw new ArgumentNullException(nameof(shelvesData), "Parsing search failed. Request response does not contain any shelves.");
        }

        foreach (JProperty shelfData in shelvesData)
        {
            // Parse shelf data
            logger?.LogInformation($"[SearchClient-Parse] Parsing shelf data: {shelfData.Path}.");
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
                logger?.LogInformation($"[SearchClient-Parse] Parsing shelf item: {shelfItem.Path}.");
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
    /// <param name="hostLanguage">The language for the payload</param>
    /// <param name="geographicalLocation">The region for the payload</param>
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
        string hostLanguage = "en",
        string geographicalLocation = "US",
        CancellationToken cancellationToken = default)
    {
        string apiPath = "search";

        // Prepare request
        if (string.IsNullOrWhiteSpace(query))
        {
            logger?.LogError($"[SearchClient-SearchAsync] Search failed. Query parameter is null or whitespace.");
            throw new ArgumentNullException(nameof(query), "Search failed. Query parameter is null or whitespace.");
        }

        (string key, object? value)[] payload =
        [
            ("query", query),
            ("params", kind.ToParams())
        ];

        // Send request
        JObject requestResponse = await baseClient.SendRequestAsync(apiPath, payload, hostLanguage, geographicalLocation, cancellationToken);

        // Parse request response
        IEnumerable<Shelf> searchResults = Parse(requestResponse);
        return searchResults;
    }

    /// <summary>
    /// Searches for a specfic shelf for a query on YouTube Music
    /// </summary>
    /// <param name="query">The query to search for</param>
    /// <param name="hostLanguage">The language for the payload</param>
    /// <param name="geographicalLocation">The region for the payload</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <returns>An array of the specific shelf items</returns>
    /// <exception cref="ArgumentNullException">Occurs when request response does not contain any shelves or some parsed item info is null</exception>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    public async Task<IEnumerable<T>> SearchAsync<T>(
        string query,
        string hostLanguage = "en",
        string geographicalLocation = "US",
        CancellationToken cancellationToken = default) where T : IShelfItem
    {
        ShelfKind kind = GetShelfKind<T>();

        // Send request
        IEnumerable<Shelf> searchResults = await SearchAsync(query, kind, hostLanguage, geographicalLocation, cancellationToken);

        if (kind == ShelfKind.Unknown)
        {
            logger?.LogInformation($"[SearchClient-SearchSongsAsync] Concatenating shelf items.");
            IEnumerable<T> allShelfItems = searchResults.SelectMany(shelf => shelf.Items).Cast<T>();

            return allShelfItems;
        }

        // Get songs shelf
        Shelf? songsResults = searchResults.FirstOrDefault(shelf => shelf.Kind == kind);

        if (songsResults is null)
        {
            logger?.LogError($"[SearchClient-SearchSongsAsync] Search failed. Search results do not cotain requested filtered shelf.");
            throw new ArgumentNullException(nameof(songsResults), "Search failed. Search results do not cotain requested filtered shelf.");
        }

        logger?.LogInformation($"[SearchClient-SearchSongsAsync] Casting shelf items.");
        IEnumerable<T> singleShelfItems = songsResults.Items.Cast<T>();

        return singleShelfItems;
    }
}