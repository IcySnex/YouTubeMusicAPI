using Microsoft.Extensions.Logging;
using System.Text.Json;
using YouTubeMusicAPI.Exceptions;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services;

/// <summary>
/// Service used to search on YouTube Music.
/// </summary>
public sealed class SearchService : YouTubeMusicService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SearchService"/> class.
    /// </summary>
    /// <param name="requestHandler"></param>
    /// <param name="logger">The logger used to provide progress and error messages.</param>
    internal SearchService(
        RequestHandler requestHandler,
        ILogger? logger = null) : base(requestHandler, logger) { }


    /// <summary>
    /// Fetches a page of search results from YouTube Music.
    /// </summary>
    /// <typeparam name="T">The type of search results to parse.</typeparam>
    /// <param name="query">The query to search for.</param>
    /// <param name="continuationToken">The token used to continue a previous search.</param>
    /// <param name="queryParams">The query params to filter.</param>
    /// <param name="categoryTitle">The title of the shelf category.</param>
    /// <param name="parse">The function to parse JSON elements to a search result.</param>
    /// <param name="cancellationToken">The token to cancel this action.</param>
    /// <returns>A page of search results.</returns>
    /// <exception cref="KeyNotFoundException">Occurrs when no property in the JSON was found with the requested name.</exception>
    /// <exception cref="IndexOutOfRangeException">Occurrs when an index in the JSON is out of bounds.</exception>
    /// <exception cref="AuthenticationException">Occurrs when applying authentication fails.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    async Task<Page<T>> FetchPageAsync<T>(
        string query,
        string? continuationToken,
        string queryParams,
        string categoryTitle,
        Func<JsonElement, T> parse,
        CancellationToken cancellationToken = default) where T : SearchResult
    {
        // Send request
        KeyValuePair<string, object?>[] payload =
        [
            new("query", query),
            new("params", queryParams),
            new("continuation", continuationToken)
        ];

        string response = await requestHandler.PostAsync(Endpoints.Search, payload, ClientType.WebMusic, cancellationToken);

        // Parse response
        using JsonDocument json = JsonDocument.Parse(response);
        JsonElement rootElement = json.RootElement;

        bool isContinued = rootElement.TryGetProperty("continuationContents", out JsonElement continuationContents);

        JsonElement shelf = isContinued
            ? continuationContents
                .GetProperty("musicShelfContinuation")
            : rootElement
                .GetProperty("contents")
                .GetProperty("tabbedSearchResultsRenderer")
                .GetProperty("tabs")
                .GetElementAt(0)
                .GetProperty("tabRenderer")
                .GetProperty("content")
                .GetProperty("sectionListRenderer")
                .GetProperty("contents")
                .EnumerateArray()
                .First(content =>
                {
                    if (!content.TryGetProperty("musicShelfRenderer", out JsonElement shelf))
                        return false;

                    string category = shelf
                        .GetProperty("title")
                        .GetProperty("runs")
                        .GetElementAt(0)
                        .GetProperty("text")
                        .GetString()
                        .OrThrow();

                    return category == categoryTitle;
                })
                .GetProperty("musicShelfRenderer");

        string? nextContinuationToken = shelf
                .GetPropertyOrNull("continuations")
                ?.GetElementAtOrNull(0)
                ?.GetPropertyOrNull("nextContinuationData")
                ?.GetPropertyOrNull("continuation")
                ?.GetString();

        List<T> result = [];
        foreach (JsonElement item in shelf.GetProperty("contents").EnumerateArray())
        {
            T searchResult = parse(item);
            result.Add(searchResult);
        }

        return new(result, nextContinuationToken);
    }

    /// <summary>
    /// Creates a paginator that fetches search results from YouTube Music.
    /// </summary>
    /// <typeparam name="T">The type of search results to parse.</typeparam>
    /// <param name="query">The query to search for.</param>
    /// <param name="queryParams">The query params to filter.</param>
    /// <param name="categoryTitle">The title of the shelf category.</param>
    /// <param name="parse">The function to parse JSON elements to a search result.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="SearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurrs when the query is <see langword="null"/> or empty.</exception>
    PaginatedAsyncEnumerable<T> CreatePaginatorAsync<T>(
        string query,
        string queryParams,
        string categoryTitle,
        Func<JsonElement, T> parse) where T : SearchResult
    {
        Ensure.NotNullOrEmpty(query, nameof(query));

        return new((contiuationToken, cancellationToken) =>
            FetchPageAsync(query, contiuationToken, queryParams, categoryTitle, parse, cancellationToken));
    }


    /// <summary>
    /// Searches for songs on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="SongSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurrs when the query is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<SongSearchResult> SongsAsync(
        string query) =>
        CreatePaginatorAsync(
            query,
            "EgWKAQIIAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            "Songs",
            SongSearchResult.Parse);

    /// <summary>
    /// Searches for videos on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="VideoSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurrs when the query is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<VideoSearchResult> VideosAsync(
        string query) =>
        CreatePaginatorAsync(
            query,
            "EgWKAQIQAWoQEAMQBBAJEAoQBRAREBAQFQ%3D%3D",
            "Videos",
            VideoSearchResult.Parse);

    /// <summary>
    /// Searches for playlists on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="PlaylistSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurrs when the query is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<PlaylistSearchResult> PlaylistsAsync(
        string query) =>
        CreatePaginatorAsync(
            query,
            "EgeKAQQoAEABahAQAxAKEAkQBBAFEBEQEBAV",
            "Community playlists",
            PlaylistSearchResult.Parse);

    /// <summary>
    /// Searches for albums on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="AlbumSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurrs when the query is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<AlbumSearchResult> AlbumsAsync(
        string query) =>
        CreatePaginatorAsync(
            query,
            "EgWKAQIYAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            "Albums",
            AlbumSearchResult.Parse);

    /// <summary>
    /// Searches for artists on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="ArtistSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurrs when the query is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<ArtistSearchResult> ArtistsAsync(
        string query) =>
        CreatePaginatorAsync(
            query,
            "EgWKAQIgAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            "Artists",
            ArtistSearchResult.Parse);

    /// <summary>
    /// Searches for profiles on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="ProfileSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurrs when the query is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<ProfileSearchResult> ProfilesAsync(
        string query) =>
        CreatePaginatorAsync(
            query,
            "EgWKAQJYAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            "Profiles",
            ProfileSearchResult.Parse);

    /// <summary>
    /// Searches for podcasts on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="PodcastSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurrs when the query is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<PodcastSearchResult> PodcastsAsync(
        string query) =>
        CreatePaginatorAsync(
            query,
            "EgWKAQJQAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            "Podcasts",
            PodcastSearchResult.Parse);

    /// <summary>
    /// Searches for podcast episodes on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="EpisodeSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurrs when the query is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<EpisodeSearchResult> EpisodesAsync(
        string query) =>
        CreatePaginatorAsync(
            query,
            "EgWKAQJIAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            "Episodes",
            EpisodeSearchResult.Parse);


    /// <summary>
    /// Searches for all kind of results on YouTUbe Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <param name="cancellationToken">The token to cancel this action.</param>
    /// <returns>A list of search results.</returns>
    /// <exception cref="ArgumentException">Occurrs when the query is <see langword="null"/> or empty.</exception>
    /// <exception cref="NotSupportedException">Occurrs when trying to parse an unsupported kind of shelf.</exception>
    /// <exception cref="KeyNotFoundException">Occurrs when no property in the JSON was found with the requested name.</exception>
    /// <exception cref="IndexOutOfRangeException">Occurrs when an index in the JSON is out of bounds.</exception>
    /// <exception cref="AuthenticationException">Occurrs when applying authentication fails.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public async Task<SearchPage> AllAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrEmpty(query, nameof(query));

        // Send request
        KeyValuePair<string, object?>[] payload =
        [
            new("query", query)
        ];

        string response = await requestHandler.PostAsync(Endpoints.Search, payload, ClientType.WebMusic, cancellationToken);

        // Parse response
        using JsonDocument json = JsonDocument.Parse(response);
        JsonElement rootElement = json.RootElement;

        JsonElement contents = rootElement
            .GetProperty("contents")
            .GetProperty("tabbedSearchResultsRenderer")
            .GetProperty("tabs")
            .GetElementAt(0)
            .GetProperty("tabRenderer")
            .GetProperty("content")
            .GetProperty("sectionListRenderer")
            .GetProperty("contents");

        List<SearchResult> items = [];
        SearchResult? topResult = null;
        List<SearchResult> relatedTopResults = [];
        foreach (JsonElement content in contents.EnumerateArray())
        {
            // Top Result
            if (content.TryGetProperty("musicCardShelfRenderer", out JsonElement cardShelf))
            {
                string category = cardShelf
                    .GetProperty("subtitle")
                    .GetProperty("runs")
                    .GetElementAt(0)
                    .GetProperty("text")
                    .GetString()
                    .OrThrow();

                // Primary
                Func<JsonElement, SearchResult>? parseTopResult = category switch
                {
                    "Song" => SongSearchResult.ParseTopResult,
                    "Video" => VideoSearchResult.ParseTopResult,
                    "Playlist" => PlaylistSearchResult.ParseTopResult,
                    "Album" or "EP" or "Single" => AlbumSearchResult.ParseTopResult,
                    "Artist" => ArtistSearchResult.ParseTopResult,
                    "Profile" => null, // never found any top results profiles lol; If u did, CREATE AN ISSUE plz
                    "Podcast" => null, // bruh; YT returns top result podcasts in a "musicShelfRenderer". cba rn frfr
                    "Episode" => EpisodeSearchResult.ParseTopResult,
                    _ => null
                };
                if (parseTopResult is null)
                {
                    logger?.LogWarning("[SearchService-AllAsync] Could not parse top result. Unsupported caegory: {category}.", category);
                    continue;
                }

                topResult = parseTopResult(cardShelf);

                // Related
                if (cardShelf.TryGetProperty("contents", out JsonElement cardShelfContents))
                {
                    foreach (JsonElement item in cardShelfContents.EnumerateArray())
                    {
                        if (!item.TryGetProperty("musicResponsiveListItemRenderer", out JsonElement itemContent))
                            continue;

                        JsonElement descriptionRuns = itemContent
                            .GetProperty("flexColumns")
                            .GetElementAt(1)
                            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
                            .GetProperty("text")
                            .GetProperty("runs");

                        JsonElement firstDescriptionRun = descriptionRuns
                            .GetElementAt(0);

                        string itemCategory = firstDescriptionRun
                            .GetProperty("text")
                            .GetString()
                            .OrThrow();

                        Func<JsonElement, SearchResult>? parseItem = itemCategory switch
                        {
                            "Song" => SongSearchResult.Parse,
                            "Video" => VideoSearchResult.Parse,
                            "Album" or "EP" or "Single" => AlbumSearchResult.Parse,
                            _ => null // never found any top results playlists, artists, profiles, podcasts or episodes lol; If u did, CREATE AN ISSUE plz
                        };
                        if (parseItem is null &&
                            firstDescriptionRun
                                .TryGetProperty("navigationEndpoint", out _) &&
                            descriptionRuns
                                .GetElementAtOrNull(2)?
                                .GetPropertyOrNull("text")
                                ?.GetString() is string viewsInfo &&
                            viewsInfo
                                .Contains("views") &&
                            descriptionRuns
                                .GetElementAtOrNull(4)
                                ?.GetPropertyOrNull("text")
                                ?.GetString()
                                ?.ToTimeSpan() is not null)
                            parseItem = VideoSearchResult.Parse;
                        if (parseItem is null)
                        {
                            logger?.LogWarning("[SearchService-AllAsync] Could not parse related top result. Unsupported caegory: {category}.", category);
                            continue;
                        }

                        SearchResult searchResult = parseItem(item);
                        relatedTopResults.Add(searchResult);
                    }
                }

            }

            // Shelf
            if (content.TryGetProperty("musicShelfRenderer", out JsonElement shelf))
            {
                string category = shelf
                    .GetProperty("title")
                    .GetProperty("runs")
                    .GetElementAt(0)
                    .GetProperty("text")
                    .GetString()
                    .OrThrow();

                Func<JsonElement, SearchResult>? parse = category switch
                {
                    "Songs" => SongSearchResult.Parse,
                    "Videos" => VideoSearchResult.Parse,
                    "Community playlists" or "Featured playlists" => PlaylistSearchResult.Parse,
                    "Albums" => AlbumSearchResult.Parse,
                    "Artists" => ArtistSearchResult.Parse,
                    "Profiles" => ProfileSearchResult.Parse,
                    "Podcasts" => PodcastSearchResult.Parse,
                    "Episodes" => EpisodeSearchResult.Parse,
                    _ => null
                };
                if (parse is null)
                {
                    logger?.LogWarning("[SearchService-AllAsync] Could not parse item. Unsupported caegory: {category}.", category);
                    continue;
                }

                foreach (JsonElement item in shelf.GetProperty("contents").EnumerateArray())
                {
                    SearchResult searchResult = parse(item);
                    items.Add(searchResult);
                }
            }
        }

        return new(items, topResult, relatedTopResults);
    }
}