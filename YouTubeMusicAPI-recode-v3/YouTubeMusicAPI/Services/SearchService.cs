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
    // Query Params
    const string QueryParamsArtists = "EgWKAQIgAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D";
    const string QueryParamsPodcasts = "EgWKAQJQAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D";
    const string QueryParamsEpisodes = "EgWKAQJIAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D";
    const string QueryParamsProfiles = "EgWKAQJYAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D";


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
    /// <param name="parseItem">The function to parse items from JSON to a search result.</param>
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
        Func<JsonElement, T> parseItem,
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
            T searchResult = parseItem(item);
            result.Add(searchResult);
        }

        return new(result, nextContinuationToken);
    }

    /// <summary>
    /// Creates a delegate that fetches a page of search results from YouTube Music.
    /// </summary>
    /// <typeparam name="T">The type of search results to parse.</typeparam>
    /// <param name="query">The query to search for.</param>
    /// <param name="queryParams">The query params to filter.</param>
    /// <param name="categoryTitle">The title of the shelf category.</param>
    /// <param name="parseItem">The function to parse items from JSON to a search result.</param>
    /// <returns>A delegate that fetches a page of search results.</returns>
    FetchPageDelegate<T> CreateFetchPageDelegate<T>(
        string query,
        string queryParams,
        string categoryTitle,
        Func<JsonElement, T> parseItem) where T : SearchResult =>
        (contiuationToken, cancellationToken) => FetchPageAsync(query, contiuationToken, queryParams, categoryTitle, parseItem, cancellationToken);


    /// <summary>
    /// Searches for songs on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="SongSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurrs when the query is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<SongSearchResult> SongsAsync(
        string query)
    {
        Ensure.NotNullOrEmpty(query, nameof(query));

        FetchPageDelegate<SongSearchResult> fetchPageDelegate = CreateFetchPageDelegate(
            query,
            "EgWKAQIIAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            "Songs",
            SongSearchResult.Parse);
        return new(fetchPageDelegate);
    }

    /// <summary>
    /// Searches for videos on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="VideoSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurrs when the query is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<VideoSearchResult> VideosAsync(
        string query)
    {
        Ensure.NotNullOrEmpty(query, nameof(query));

        FetchPageDelegate<VideoSearchResult> fetchPageDelegate = CreateFetchPageDelegate(
            query,
            "EgWKAQIQAWoQEAMQBBAJEAoQBRAREBAQFQ%3D%3D",
            "Videos",
            VideoSearchResult.Parse);
        return new(fetchPageDelegate);
    }

    /// <summary>
    /// Searches for playlists on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="PlaylistSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurrs when the query is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<PlaylistSearchResult> PlaylistsAsync(
        string query)
    {
        Ensure.NotNullOrEmpty(query, nameof(query));

        FetchPageDelegate<PlaylistSearchResult> fetchPageDelegate = CreateFetchPageDelegate(
            query,
            "EgeKAQQoAEABahAQAxAKEAkQBBAFEBEQEBAV",
            "Community playlists",
            PlaylistSearchResult.Parse);
        return new(fetchPageDelegate);
    }

    /// <summary>
    /// Searches for albums on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="AlbumSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurrs when the query is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<AlbumSearchResult> AlbumsAsync(
        string query)
    {
        Ensure.NotNullOrEmpty(query, nameof(query));

        FetchPageDelegate<AlbumSearchResult> fetchPageDelegate = CreateFetchPageDelegate(
            query,
            "EgWKAQIYAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            "Albums",
            AlbumSearchResult.Parse);
        return new(fetchPageDelegate);
    }
}