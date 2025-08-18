using Microsoft.Extensions.Logging;
using System.Text.Json;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services;

/// <summary>
/// Service used to search on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="SearchService"/> class.
/// </remarks>
/// <param name="client">The shared base client.</param>
public sealed class SearchService(
    YouTubeMusicClient client)
{
    readonly YouTubeMusicClient client = client;


    /// <summary>
    /// Fetches a page of search results from YouTube Music.
    /// </summary>
    /// <typeparam name="T">The type of search results to parse.</typeparam>
    /// <param name="query">The query to search for.</param>
    /// <param name="continuationToken">The token used to continue a previous search.</param>
    /// <param name="queryParams">The query params to filter.</param>
    /// <param name="category">The title of the shelf category.</param>
    /// <param name="parse">The function to parse JSON elements to a search result.</param>
    /// <param name="cancellationToken">The token to cancel this tas.</param>
    /// <returns>A page of search results.</returns>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    async Task<Page<T>> FetchPageAsync<T>(
        string query,
        string? continuationToken,
        string queryParams,
        string category,
        Func<JElement, T> parse,
        CancellationToken cancellationToken = default) where T : SearchResult
    {
        // Send request
        KeyValuePair<string, object?>[] payload =
        [
            new("query", query),
            new("params", queryParams),
            new("continuation", continuationToken)
        ];

        string response = await client.RequestHandler.PostAsync(Endpoints.Search, payload, ClientType.WebMusic, cancellationToken);

        // Parse response
        client.Logger?.LogInformation("[SearchService-FetchPageAsync] Parsing response...");
        using JsonDocument json = JsonDocument.Parse(response);
        JElement root = new(json.RootElement);

        bool isContinued = root
            .Contains("continuationContents", out JElement continuationContents);

        JElement shelf = isContinued
            .If(true,
                continuationContents
                    .Get("musicShelfContinuation"),
                root
                    .Get("contents")
                    .Get("tabbedSearchResultsRenderer")
                    .Get("tabs")
                    .GetAt(0)
                    .Get("tabRenderer")
                    .Get("content")
                    .Get("sectionListRenderer")
                    .Get("contents")
                    .AsArray()
                    .Or(JArray.Empty)
                    .FirstOrDefault(item => item
                        .Get("musicShelfRenderer")
                        .SelectRunTextAt("title", 0)
                        .Is(category)));
        if (shelf.IsUndefined)
            return new([], null);
        if (!isContinued)
            shelf = shelf.Get("musicShelfRenderer");

        string? nextContinuationToken = shelf
            .Get("continuations")
            .GetAt(0)
            .Get("nextContinuationData")
            .Get("continuation")
            .AsString();

        List<T> result = shelf
            .Get("contents")
            .AsArray()
            .OrThrow()
            .Select(item => item
                .Get("musicResponsiveListItemRenderer"))
            .Where(item => item // istg youtube. why tf do u return PODCAST EPISODES in video searches???? now i gotta do that unnecessary extra saftey check for a 1/100 chance
                .Get("menu")
                .Get("menuRenderer")
                .Contains("topLevelButtons")
                .And(category != "Episodes")
                .Not())
            .Select(item => parse(item))
            .ToList();

        return new(result, nextContinuationToken);
    }

    /// <summary>
    /// Creates a paginator that fetches search results from YouTube Music.
    /// </summary>
    /// <typeparam name="T">The type of search results to parse.</typeparam>
    /// <param name="query">The query to search for.</param>
    /// <param name="queryParams">The query params to filter.</param>
    /// <param name="category">The title of the shelf category.</param>
    /// <param name="parse">The function to parse JSON elements to a search result.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="SearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
    PaginatedAsyncEnumerable<T> CreatePaginatorAsync<T>(
        string query,
        string queryParams,
        string category,
        Func<JElement, T> parse) where T : SearchResult
    {
        Ensure.NotNullOrEmpty(query, nameof(query));

        return new((contiuationToken, cancellationToken) =>
            FetchPageAsync(query, contiuationToken, queryParams, category, parse, cancellationToken));
    }


    /// <summary>
    /// Creates a paginator that searches for songs on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="SongSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<SongSearchResult> SongsAsync(
        string query) =>
        CreatePaginatorAsync(
            query,
            "EgWKAQIIAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            "Songs",
            SongSearchResult.Parse);

    /// <summary>
    /// Creates a paginator that searches for videos on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="VideoSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<VideoSearchResult> VideosAsync(
        string query) =>
        CreatePaginatorAsync(
            query,
            "EgWKAQIQAWoQEAMQBBAJEAoQBRAREBAQFQ%3D%3D",
            "Videos",
            VideoSearchResult.Parse);

    /// <summary>
    /// Creates a paginator that searches for playlists on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="PlaylistSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<PlaylistSearchResult> PlaylistsAsync(
        string query) =>
        CreatePaginatorAsync(
            query,
            "EgeKAQQoAEABahAQAxAKEAkQBBAFEBEQEBAV",
            "Community playlists",
            PlaylistSearchResult.Parse);

    /// <summary>
    /// Creates a paginator that searches for albums on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="AlbumSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<AlbumSearchResult> AlbumsAsync(
        string query) =>
        CreatePaginatorAsync(
            query,
            "EgWKAQIYAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            "Albums",
            AlbumSearchResult.Parse);

    /// <summary>
    /// Creates a paginator that searches for artists on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="ArtistSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<ArtistSearchResult> ArtistsAsync(
        string query) =>
        CreatePaginatorAsync(
            query,
            "EgWKAQIgAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            "Artists",
            ArtistSearchResult.Parse);

    /// <summary>
    /// Creates a paginator that searches for profiles on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="ProfileSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<ProfileSearchResult> ProfilesAsync(
        string query) =>
        CreatePaginatorAsync(
            query,
            "EgWKAQJYAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            "Profiles",
            ProfileSearchResult.Parse);

    /// <summary>
    /// Creates a paginator that searches for podcasts on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="PodcastSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<PodcastSearchResult> PodcastsAsync(
        string query) =>
        CreatePaginatorAsync(
            query,
            "EgWKAQJQAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            "Podcasts",
            PodcastSearchResult.Parse);

    /// <summary>
    /// Creates a paginator that searches for episodes on YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="EpisodeSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
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
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>A <see cref="SearchPage"/> containing all items, including the top result.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
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

        string response = await client.RequestHandler.PostAsync(Endpoints.Search, payload, ClientType.WebMusic, cancellationToken);

        // Parse response
        client.Logger?.LogInformation("[SearchService-AllAsync] Parsing response...");
        using JsonDocument json = JsonDocument.Parse(response);
        JElement root = new(json.RootElement);

        JArray contents = root
            .Get("contents")
            .Get("tabbedSearchResultsRenderer")
            .Get("tabs")
            .GetAt(0)
            .Get("tabRenderer")
            .Get("content")
            .Get("sectionListRenderer")
            .Get("contents")
            .AsArray()
            .OrThrow();

        // Top Result
        JElement cardShelf = contents
            .FirstOrDefault(item => item.
                Contains("musicCardShelfRenderer"))
            .Get("musicCardShelfRenderer");

        // - Primary
        client.Logger?.LogInformation("[SearchService-AllAsync] Parsing top result...");
        SearchResult? topResult = null;

        string? category = cardShelf
            .SelectRunTextAt("subtitle", 0);
        switch (category)
        {
            case "Song":
                topResult = SongSearchResult.ParseTopResult(cardShelf);
                break;

            case "Video":
                topResult = VideoSearchResult.ParseTopResult(cardShelf);
                break;

            case "Playlist":
                topResult = PlaylistSearchResult.ParseTopResult(cardShelf);
                break;

            case "Album" or "EP" or "Single":
                topResult = AlbumSearchResult.ParseTopResult(cardShelf);
                break;

            case "Artist":
                topResult = ArtistSearchResult.ParseTopResult(cardShelf);
                break;

            case "Profile": // never found any top result profile; If u did, CREATE AN ISSUE plz :3
            case "Podcast": // bruh; YT returns top result podcasts in a "musicShelfRenderer". cba rn to parse that shi frfr
                break;

            case "Episode":
                topResult = EpisodeSearchResult.ParseTopResult(cardShelf);
                break;

            default:
                client.Logger?.LogWarning("[SearchService-AllAsync] Could not parse top result. Unsupported category: {categoryTitle}.", category);
                break;
        }

        // - Related
        List<SearchResult> relatedTopResults = cardShelf
            .Get("contents")
            .AsArray()
            .Or(JArray.Empty)
            .Select(item =>
            {
                if (!item.Contains("musicResponsiveListItemRenderer", out JElement content))
                    return null;

                JElement descriptionRuns = content
                        .Get("flexColumns")
                        .GetAt(1)
                        .Get("musicResponsiveListItemFlexColumnRenderer")
                        .Get("text")
                        .Get("runs");
                JElement firstDescriptionRun = descriptionRuns
                    .GetAt(0);

                string? category = firstDescriptionRun
                    .Get("text")
                    .AsString();
                Func<JElement, SearchResult>? parseItem = category switch
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
                        .Contains("navigationEndpoint") &&
                    descriptionRuns
                        .GetAt(2)
                        .Get("text")
                        .AsString()
                        .IsNotNull(out string? viewsInfo) &&
                    viewsInfo
                        .Contains("views") &&
                    descriptionRuns
                        .GetAt(4)
                        .Get("text")
                        .AsString()
                        .ToTimeSpan()
                        .IsNotNull())
                    parseItem = VideoSearchResult.Parse;
                if (parseItem is null)
                {
                    client.Logger?.LogWarning("[SearchService-AllAsync] Could not parse related top result. Unsupported caegory: {category}.", category);
                    return null;
                }

                if (content // istg youtube. why tf do u return PODCAST EPISODES in video searches???? now i gotta do that unnecessary extra saftey check for a 1/100 chance
                    .Get("menu")
                    .Get("menuRenderer")
                    .Contains("topLevelButtons")
                    .And(category != "Episodes"))
                    return null;

                return parseItem(content);
            })
            .Where(Syntax.IsNotNull)
            .Cast<SearchResult>()
            .ToList();

        // Results
        List<SearchResult> results = contents
            .Where(item => item
                .Contains("musicShelfRenderer"))
            .SelectMany(item =>
            {
                JElement shelf = item
                    .Get("musicShelfRenderer");

                string? category = shelf
                    .SelectRunTextAt("title", 0);
                Func<JElement, SearchResult>? parse = category switch
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
                    client.Logger?.LogWarning("[SearchService-AllAsync] Could not parse search result. Unsupported caegory: {category}.", category);
                    return Enumerable.Empty<SearchResult>();
                }

                client.Logger?.LogInformation("[SearchService-AllAsync] Parsing '{categoryTitle}' shelf...", category);
                return shelf
                    .Get("contents")
                    .AsArray()
                    .Or(JArray.Empty)
                    .Select(item => item
                        .Get("musicResponsiveListItemRenderer"))
                    .Where(item => item
                        .Get("menu")
                        .Get("menuRenderer")
                        .Contains("topLevelButtons")
                        .And(category != "Episodes")
                        .Not())
                    .Select(parse);
            })
            .Where(Syntax.IsNotNull)
            .ToList();

        return new(results, topResult, relatedTopResults);
    }


    /// <summary>
    /// Gets search, history and result suggestions for a specific input.
    /// </summary>
    /// <remarks>
    /// For history suggestions, the user must be authenticated.
    /// </remarks>
    /// <param name="input">The input to get suggestions for.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>Search suggestions, including search, history and result suggestions.</returns>
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

        string response = await client.RequestHandler.PostAsync(Endpoints.SearchSuggestions, payload, ClientType.WebMusic, cancellationToken);

        // Parse response
        client.Logger?.LogInformation("[SearchService-GetSuggestionsAsync] Parsing response...");
        using JsonDocument json = JsonDocument.Parse(response);
        JElement root = new(json.RootElement);
        if (!root.Contains("contents", out JElement contents))
            return new([], [], []);

        // Text Suggestions
        client.Logger?.LogInformation("[SearchService-GetSuggestionsAsync] Parsing text suggestions...");
        JArray textSuggestions = contents
            .GetAt(0)
            .Get("searchSuggestionsSectionRenderer")
            .Get("contents")
            .AsArray()
            .Or(JArray.Empty);

        List<string> searchSuggestions = textSuggestions
            .Where(item => item
                .Contains("searchSuggestionRenderer"))
            .Select(item => item
                .Get("searchSuggestionRenderer")
                .Get("suggestion")
                    .Get("runs")
                    .AsArray()
                    .OrThrow()
                    .Select(run => run
                        .Get("text")
                        .AsString()
                        .Or(""))
                    .Join(""))
            .ToList();
        List<string> historySuggestions = textSuggestions
            .Where(item => item
                .Contains("historySuggestionRenderer"))
            .Select(item => item
                .Get("historySuggestionRenderer")
                .Get("suggestion")
                    .Get("runs")
                    .AsArray()
                    .OrThrow()
                    .Select(run => run
                        .Get("text")
                        .AsString()
                        .Or(""))
                    .Join(""))
            .ToList();

        // Result Suggestions
        client.Logger?.LogInformation("[SearchService-GetSuggestionsAsync] Parsing result suggestions...");
        JArray? resultSuggestions = contents
            .GetAt(1)
            .Get("searchSuggestionsSectionRenderer")
            .Get("contents")
            .AsArray();
        if (resultSuggestions is null)
            return new(searchSuggestions, historySuggestions, []);

        List<SearchResult> results = resultSuggestions
            .Select(item =>
            {
                if (!item.Contains("musicResponsiveListItemRenderer", out JElement content))
                    return null;

                string? category = content
                    .Get("flexColumns")
                    .GetAt(0)
                    .Get("musicResponsiveListItemFlexColumnRenderer")
                    .SelectRunTextAt("text", 0);
                Func<JElement, SearchResult>? parseItem = category switch
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
                    content.SelectNavigationBrowseId() is string browseId &&
                    browseId.StartsWith("UC"))
                {
                    parseItem = ArtistSearchResult.ParseSuggestion;
                }
                if (parseItem is null)
                {
                    client.Logger?.LogWarning("[SearchService-GetSuggestionsAsync] Could not parse search suggestion result. Unsupported caegory: {category}.", category);
                    return null;
                }

                return parseItem(content);
            })
            .Where(Syntax.IsNotNull)
            .Cast<SearchResult>()
            .ToList();

        return new(searchSuggestions, historySuggestions, results);
    }


    /// <summary>
    /// Removes the specific input from the search history suggestions.
    /// </summary>
    /// <remarks>
    /// The user must be authenticated.
    /// </remarks>
    /// <param name="input">The input to get remove from the suggestions.</param>
    /// <param name="cancellationToken">The token to cancel this action.</param>
    /// <exception cref="ArgumentException">Occurs when the <c>input</c> is <see langword="null"/> or empty.</exception>
    /// <exception cref="NullReferenceException">Occurs when the search suggestion could not be found in the history.</exception>
    /// <exception cref="InvalidOperationException">Occurs when the user is not authenticated.</exception>
    /// <exception cref="NullReferenceException">Occurs when a the <c>input</c> could not be found in the search history suggestions.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public async Task RemoveSuggestionAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrEmpty(input, nameof(input));
        Ensure.IsAuthenticated(client.RequestHandler);

        // Send suggestions request
        client.Logger?.LogInformation("[SearchService-RemoveSuggestionAsync] Getting suggestions...");
        KeyValuePair<string, object?>[] suggestionsPayload =
        [
            new("input", input)
        ];

        string suggestionsResponse = await client.RequestHandler.PostAsync(Endpoints.SearchSuggestions, suggestionsPayload, ClientType.WebMusic, cancellationToken);

        // Parse suggestions response
        client.Logger?.LogInformation("[SearchService-RemoveSuggestionAsync] Parsing suggestions response...");
        using JsonDocument suggestionsJson = JsonDocument.Parse(suggestionsResponse);
        JElement suggestionsRoot = new(suggestionsJson.RootElement);

        string? feedbackToken = suggestionsRoot
            .Get("contents")
            .GetAt(0)
            .Get("searchSuggestionsSectionRenderer")
            .Get("contents")
            .AsArray()
            .Or(JArray.Empty)
            .FirstOrDefault(item => item
                .Get("historySuggestionRenderer")
                .SelectRunTextAt("suggestion", 0)
                .IsNotNull(out string? suggestion)
                .And(string.Equals(suggestion, input, StringComparison.InvariantCultureIgnoreCase)))
            .Get("historySuggestionRenderer")
            .Get("serviceEndpoint")
            .Get("feedbackEndpoint")
            .Get("feedbackToken")
            .AsString();
        if (feedbackToken is null)
        {
            client.Logger?.LogError("[SearchService-RemoveSuggestionAsync] Could not find search suggestion '{input}' in history to remove.", input);
            throw new NullReferenceException($"Could not find search suggestion '{input}' in history to remove.");
        }

        // Send remove request
        client.Logger?.LogInformation("[SearchService-RemoveSuggestionAsync] Removing suggestion...");
        KeyValuePair<string, object?>[] payload =
        [
            new("feedbackTokens", new string[] { feedbackToken })
        ];

        string removeResponse = await client.RequestHandler.PostAsync(Endpoints.Feedback, payload, ClientType.WebMusic, cancellationToken);

        // Parse remove response
        client.Logger?.LogInformation("[SearchService-RemoveSuggestionAsync] Parsing remove response...");
        using JsonDocument removeJson = JsonDocument.Parse(removeResponse);
        JElement removeRoot = new(removeJson.RootElement);

        bool isProcessed = removeRoot
            .Get("feedbackResponses")
            .GetAt(0)
            .Get("isProcessed")
            .AsBool()
            .Or(false);
        if (!isProcessed)
        {
            client.Logger?.LogError("[SearchService-RemoveSuggestionAsync] Remove search suggestion '{input}' not processed.", input);
            throw new NotSupportedException($"Remove search suggestion '{input}' not processed.");
        }
    }
}