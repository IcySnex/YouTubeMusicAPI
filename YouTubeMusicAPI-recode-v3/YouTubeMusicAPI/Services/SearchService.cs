﻿using Microsoft.Extensions.Logging;
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
    /// <param name="requestHandler">The request handler.</param>
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
    /// <param name="category">The title of the shelf category.</param>
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
        string category,
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
                .FirstOrDefault(content =>
                {
                    if (!content.TryGetProperty("musicShelfRenderer", out JsonElement shelf))
                        return false;

                    string categoryTitle = shelf
                        .GetProperty("title")
                        .GetProperty("runs")
                        .GetElementAt(0)
                        .GetProperty("text")
                        .GetString()
                        .OrThrow();

                    return category == categoryTitle;
                });
        if (shelf.ValueKind == JsonValueKind.Undefined)
            return new([], null);
        if (!isContinued)
            shelf = shelf.GetProperty("musicShelfRenderer");

        string? nextContinuationToken = shelf
            .GetPropertyOrNull("continuations")
            ?.GetElementAtOrNull(0)
            ?.GetPropertyOrNull("nextContinuationData")
            ?.GetPropertyOrNull("continuation")
            ?.GetString();

        List<T> result = [];
        foreach (JsonElement content in shelf.GetProperty("contents").EnumerateArray())
        {
            JsonElement item = content
                .GetProperty("musicResponsiveListItemRenderer");

            // istg youtube. why tf do u return PODCAST EPISODES in video searches???? now i gotta do that unnecessary extra saftey check for a 1/100 chance
            if (item.SelectIsPodcastEvenThoItShouldnt(category))
                continue;

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
    /// <returns>A page containing all search results, including the top result.</returns>
    /// <exception cref="ArgumentException">Occurrs when the query is <see langword="null"/> or empty.</exception>
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
                    "Profile" => null, // never found any top result profile; If u did, CREATE AN ISSUE plz :3
                    "Podcast" => null, // bruh; YT returns top result podcasts in a "musicShelfRenderer". cba rn to parse that shi frfr
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
                    foreach (JsonElement cardContent in cardShelfContents.EnumerateArray())
                    {
                        if (!cardContent.TryGetProperty("musicResponsiveListItemRenderer", out JsonElement item))
                            continue;

                        JsonElement descriptionRuns = item
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
                            "Playlist" => null, // never found any top result playlist; If u did, CREATE AN ISSUE plz :3
                            "Album" or "EP" or "Single" => AlbumSearchResult.Parse,
                            "Artist" => null, // never found any top result artist; If u did, CREATE AN ISSUE plz :3
                            "Profile" => null, // never found any top result profile; If u did, CREATE AN ISSUE plz :3
                            "Podcast" => null, // never found any top result podcast; If u did, CREATE AN ISSUE plz :3
                            "Episode" => EpisodeSearchResult.Parse,
                            _ => null
                        };
                        if (parseItem is null && // bruh; YT cmon why dont u write "Video" for videos??? now i gotta do that random ahh fallback >:(
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
                            logger?.LogWarning("[SearchService-AllAsync] Could not parse related top result. Unsupported caegory: {category}.", itemCategory);
                            continue;
                        }

                        // istg youtube. why tf do u return PODCAST EPISODES in video searches???? now i gotta do that unnecessary extra saftey check for a 1/100 chance
                        if (item.SelectIsPodcastEvenThoItShouldnt(category))
                            continue;

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

                foreach (JsonElement shelfContent in shelf.GetProperty("contents").EnumerateArray())
                {
                    if (!shelfContent.TryGetProperty("musicResponsiveListItemRenderer", out JsonElement item))
                        continue;

                    // istg youtube. why tf do u return PODCAST EPISODES in video searches???? now i gotta do that unnecessary extra saftey check for a 1/100 chance
                    if (item.SelectIsPodcastEvenThoItShouldnt(category))
                        continue;

                    SearchResult searchResult = parse(item);
                    items.Add(searchResult);
                }
            }
        }

        return new(items, topResult, relatedTopResults);
    }


    /// <summary>
    /// Gets search, history and result suggestions for a specific input.
    /// </summary>
    /// <remarks>
    /// For history suggestions, the user must be authenticated.
    /// </remarks>
    /// <param name="input">The input to get suggestions for.</param>
    /// <param name="cancellationToken">The token to cancel this action.</param>
    /// <returns>Search suggestions, including search, history and result suggestions.</returns>
    /// <exception cref="KeyNotFoundException">Occurrs when no property in the JSON was found with the requested name.</exception>
    /// <exception cref="IndexOutOfRangeException">Occurrs when an index in the JSON is out of bounds.</exception>
    /// <exception cref="AuthenticationException">Occurrs when applying authentication fails.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public async Task<SearchSuggestions> GetSuggestionsAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        // Send request
        KeyValuePair<string, object?>[] payload =
        [
            new("input", input)
        ];

        string response = await requestHandler.PostAsync(Endpoints.SearchSuggestions, payload, ClientType.WebMusic, cancellationToken);

        // Parse response
        using JsonDocument json = JsonDocument.Parse(response);
        JsonElement rootElement = json.RootElement;

        if (!rootElement.TryGetProperty("contents", out JsonElement contents))
            return new([], [], []);

        // Text Suggestions
        JsonElement textSuggestions = contents
            .GetElementAt(0)
            .GetProperty("searchSuggestionsSectionRenderer")
            .GetProperty("contents");

        List<string> search = [];
        List<string> history = [];
        foreach (JsonElement item in textSuggestions.EnumerateArray())
        {
            // Search
            if (item.TryGetProperty("searchSuggestionRenderer", out JsonElement searchItemContent))
            {
                IEnumerable<string> text = searchItemContent
                    .GetProperty("suggestion")
                    .GetProperty("runs")
                    .EnumerateArray()
                    .Select(run => run
                        .GetProperty("text")
                        .GetString()
                        .Or(""));

                search.Add(string.Join("", text));
            }
            // History
            else if (item.TryGetProperty("historySuggestionRenderer", out JsonElement historyItemContent))
            {
                IEnumerable<string> text = historyItemContent
                    .GetProperty("suggestion")
                    .GetProperty("runs")
                    .EnumerateArray()
                    .Select(run => run
                        .GetProperty("text")
                        .GetString()
                        .Or(""));

                history.Add(string.Join("", text));
            }
        }

        // Result Suggestions
        JsonElement? resultSuggestions = contents
            .GetElementAtOrNull(1)
            ?.GetPropertyOrNull("searchSuggestionsSectionRenderer")
            ?.GetPropertyOrNull("contents");

        if (resultSuggestions is null)
            return new(search, history, []);

        List<SearchResult> results = [];
        foreach (JsonElement item in resultSuggestions.Value.EnumerateArray())
        {
            if (!item.TryGetProperty("musicResponsiveListItemRenderer", out JsonElement itemContent))
                continue;

            JsonElement? firstDescriptionRun = itemContent
                .GetProperty("flexColumns")
                .GetElementAtOrNull(1)
                ?.GetPropertyOrNull("musicResponsiveListItemFlexColumnRenderer")
                ?.GetPropertyOrNull("text")
                ?.GetPropertyOrNull("runs")
                ?.GetElementAtOrNull(0);

            string? itemCategory = firstDescriptionRun?
                .GetPropertyOrNull("text")
                ?.GetString();

            Func<JsonElement, SearchResult>? parseItem = itemCategory switch
            {
                "Song" => SongSearchResult.ParseSuggestion,
                "Video" => VideoSearchResult.ParseSuggestion,
                "Playlist" => PlaylistSearchResult.ParseSuggestion,
                "Album" or "EP" or "Single" => AlbumSearchResult.ParseSuggestion,
                "Artist" => ArtistSearchResult.ParseSuggestion,
                "Profile" => null, // never found any search suggestion profiles; If u did, CREATE AN ISSUE plz :3
                "Podcast" => PodcastSearchResult.ParseSuggestion,
                "Episode" => null, // never found any search suggestion episodes; If u did, CREATE AN ISSUE plz :3
                _ => null,
            };
            if (parseItem is null && // // bruh; YT cmon why dont u have a description text for artists??? now i gotta do that dumb fallback again >:(
                itemContent
                    .GetPropertyOrNull("navigationEndpoint")
                    ?.GetPropertyOrNull("browseEndpoint")
                    ?.GetPropertyOrNull("browseId")
                    ?.GetString() is string browseId &&
                browseId.StartsWith("UC"))
                parseItem = ArtistSearchResult.ParseSuggestion;
            if (parseItem is null)
            {
                logger?.LogWarning("[SearchService-AllAsync] Could not parse search suggestion result. Unsupported caegory: {category}.", itemCategory);
                continue;
            }

            SearchResult searchResult = parseItem(itemContent);
            results.Add(searchResult);
        }

        return new(search, history, results);
    }


    /// <summary>
    /// Removes the specific input from the search history.
    /// </summary>
    /// <remarks>
    /// The user must be authenticated.
    /// </remarks>
    /// <param name="input">The input to get remove from the suggestions.</param>
    /// <param name="cancellationToken">The token to cancel this action.</param>
    /// <exception cref="ArgumentException">Occurrs when the string is <see langword="null"/> or empty.</exception>
    /// <exception cref="InvalidOperationException">Occurrs when the user is not authenticated.</exception>
    /// <exception cref="NullReferenceException">Occurrs when a the input could not be found in the search history suggestions.</exception>
    /// <exception cref="KeyNotFoundException">Occurrs when no property in the JSON was found with the requested name.</exception>
    /// <exception cref="IndexOutOfRangeException">Occurrs when an index in the JSON is out of bounds.</exception>
    /// <exception cref="AuthenticationException">Occurrs when applying authentication fails.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public async Task RemoveSuggestionAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrEmpty(input, nameof(input));
        Ensure.IsAuthenticated(requestHandler);

        // Get suggestions
        KeyValuePair<string, object?>[] suggestionsPayload =
        [
            new("input", input)
        ];

        string suggestionsResponse = await requestHandler.PostAsync(Endpoints.SearchSuggestions, suggestionsPayload, ClientType.WebMusic, cancellationToken);

        // Parse suggestions response
        using JsonDocument suggestionsJson = JsonDocument.Parse(suggestionsResponse);
        JsonElement suggestionsRootElement = suggestionsJson.RootElement;

        string feedbackToken = ((suggestionsRootElement
            .GetPropertyOrNull("contents")
            ?.GetElementAtOrNull(0)
            ?.GetPropertyOrNull("searchSuggestionsSectionRenderer")
            ?.GetPropertyOrNull("contents")
            ?.EnumerateArray()
            .FirstOrDefault(item =>
            {
                if (!item.TryGetProperty("historySuggestionRenderer", out JsonElement itemContent))
                    return false;

                string? suggestionText = itemContent
                    .GetPropertyOrNull("suggestion")
                    ?.GetPropertyOrNull("runs")
                    ?.GetElementAtOrNull(0)
                    ?.GetPropertyOrNull("text")
                    ?.GetString();

                return suggestionText == input;
            }))
            ?.GetPropertyOrNull("historySuggestionRenderer")
            ?.GetPropertyOrNull("serviceEndpoint")
            ?.GetPropertyOrNull("feedbackEndpoint")
            ?.GetPropertyOrNull("feedbackToken")
            ?.GetString())
            .OrThrow();


        // Remove
        KeyValuePair<string, object?>[] payload =
        [
            new("feedbackTokens", new string[] { feedbackToken })
        ];

        string response = await requestHandler.PostAsync(Endpoints.Feedback, payload, ClientType.WebMusic, cancellationToken);

        // Parse response
        using JsonDocument json = JsonDocument.Parse(response);
        JsonElement rootElement = json.RootElement;

        bool isProcessed = rootElement
            .GetPropertyOrNull("feedbackResponses")
            ?.GetElementAtOrNull(0)
            ?.GetPropertyOrNull("isProcessed")
            ?.GetBoolean() ?? false;

        if (!isProcessed)
            logger?.LogWarning("[SearchService-RemoveSuggestionAsync] Remove search suggestion '{input}' not processed.", input);
    }
}