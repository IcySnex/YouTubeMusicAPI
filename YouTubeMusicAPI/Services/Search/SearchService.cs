using Microsoft.Extensions.Logging;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Services.Albums;
using YouTubeMusicAPI.Services.Artists;
using YouTubeMusicAPI.Services.Episodes;
using YouTubeMusicAPI.Services.Playlists;
using YouTubeMusicAPI.Services.Podcasts;
using YouTubeMusicAPI.Services.Profiles;
using YouTubeMusicAPI.Services.Songs;
using YouTubeMusicAPI.Services.Videos;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Search;

/// <summary>
/// Service which handles searches on YouTube Music.
/// </summary>
public sealed class SearchService
{
    readonly YouTubeMusicClient client;

    /// <summary>
    /// Creates a new instance of the <see cref="SearchService"/> class.
    /// </summary>
    /// <param name="client">The shared base client.</param>
    internal SearchService(
        YouTubeMusicClient client)
    {
        this.client = client;
    }


    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    async Task<Page<T>> FetchPageAsync<T>(
        string query,
        string? continuationToken,
        string queryParams,
        Func<JElement, T> parse,
        string categoryTitle,
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
        using IDisposable _ = response.ParseJson(out JElement root);

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
                        .Is(categoryTitle)));
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
            .Where(item => // istg youtube. why tf do u return PODCAST EPISODES in video searches???? now i gotta do that unnecessary extra saftey check
                item.Contains("menu") && (categoryTitle == "Episodes" || !item.SelectIsPodcast())) // duh no fluent syntax :( "short-circuiting" performance blabla
            .Select(item => parse(item))
            .ToList();

        return new(result, nextContinuationToken);
    }


    /// <summary>
    /// Creates an async paginator that fetches search results from YouTube Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <param name="category">The category of content to search for.</param>
    /// <param name="scope">The scope of the search.</param>
    /// <param name="ignoreSpelling">Weither to ignore spelling suggestions.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="SearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
    /// <exception cref="InvalidOperationException">Occurs when the <c>category</c> is <see cref="SearchCategory.CommunityPlaylists"/> or <see cref="SearchCategory.FeaturedPlaylists"/> while <c>scope</c> is <see cref="SearchScope.Library"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Occurs when the given <see cref="SearchCategory"/> is invalid.</exception>
    public PaginatedAsyncEnumerable<SearchResult> ByCategoryAsync(
        string query,
        SearchCategory category,
        SearchScope scope = SearchScope.Global,
        bool ignoreSpelling = true)
    {
        Ensure.NotNullOrEmpty(query, nameof(query));

        if (scope == SearchScope.Library && (category == SearchCategory.CommunityPlaylists || category == SearchCategory.FeaturedPlaylists))
            throw new InvalidOperationException("The categories 'CommunityPlaylists' and 'FeaturedPlaylists' not supported for the scope 'Library'.");

        (Func<JElement, SearchResult> parse, string categoryTitle) = category switch
        {
            SearchCategory.Songs => ((Func<JElement, SearchResult>)SongSearchResult.Parse, "Songs"),
            SearchCategory.Videos => (VideoSearchResult.Parse, "Videos"),
            SearchCategory.Albums => (AlbumSearchResult.Parse, "Albums"),
            SearchCategory.Artists => (ArtistSearchResult.Parse, "Artists"),
            SearchCategory.CommunityPlaylists => (CommunityPlaylistSearchResult.Parse, "Community playlists"),
            SearchCategory.FeaturedPlaylists => (FeaturedPlaylistSearchResult.Parse, "Featured playlists"),
            SearchCategory.Profiles => (ProfileSearchResult.Parse, "Profiles"),
            SearchCategory.Podcasts => (PodcastSearchResult.Parse, "Podcasts"),
            SearchCategory.Episodes => (EpisodeSearchResult.Parse, "Episodes"),
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, "The given SearchCategory is invalid.")
        };
        string queryParams = scope.ToQueryParams(category, ignoreSpelling);

        return new((contiuationToken, cancellationToken) =>
            FetchPageAsync(query, contiuationToken, queryParams, parse, categoryTitle, cancellationToken));
    }

    /// <summary>
    /// Creates an async paginator that fetches search results from YouTube Music.
    /// </summary>
    /// <typeparam name="T">The category of content to search for.</typeparam>
    /// <param name="query">The query to search for.</param>
    /// <param name="scope">The scope of the search.</param>
    /// <param name="ignoreSpelling">Weither to ignore spelling suggestions.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="SearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
    /// <exception cref="InvalidOperationException">Occurs when the type <c>T</c> is <see cref="CommunityPlaylistSearchResult"/> or <see cref="FeaturedPlaylistSearchResult"/> while <c>scope</c> is <see cref="SearchScope.Library"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Occurs when the given type <c>T</c> is invalid.</exception>
    public PaginatedAsyncEnumerable<T> ByCategoryAsync<T>(
        string query,
        SearchScope scope = SearchScope.Global,
        bool ignoreSpelling = true) where T : SearchResult
    {
        Ensure.NotNullOrEmpty(query, nameof(query));

        Type categoryType = typeof(T);
        if (scope == SearchScope.Library && categoryType == typeof(CommunityPlaylistSearchResult))
            throw new InvalidOperationException("The categories 'CommunityPlaylists' and 'FeaturedPlaylists' not supported for the scope 'Library'.");

        (Func<JElement, T> parse, string categoryTitle, SearchCategory category) = categoryType switch
        { // holy ugly switch statement, batman! :o still better than runtime casting using LINQ (apparently)...
            Type _ when categoryType == typeof(SongSearchResult) => ((Func<JElement, T>)(object)SongSearchResult.Parse, "Songs", SearchCategory.Songs),
            Type _ when categoryType == typeof(VideoSearchResult) => ((Func<JElement, T>)(object)VideoSearchResult.Parse, "Videos", SearchCategory.Videos),
            Type _ when categoryType == typeof(AlbumSearchResult) => ((Func<JElement, T>)(object)AlbumSearchResult.Parse, "Albums", SearchCategory.Albums),
            Type _ when categoryType == typeof(ArtistSearchResult) => ((Func<JElement, T>)(object)ArtistSearchResult.Parse, "Artists", SearchCategory.Artists),
            Type _ when categoryType == typeof(CommunityPlaylistSearchResult) => ((Func<JElement, T>)(object)CommunityPlaylistSearchResult.Parse, "Community playlists", SearchCategory.CommunityPlaylists),
            Type _ when categoryType == typeof(FeaturedPlaylistSearchResult) => ((Func<JElement, T>)(object)FeaturedPlaylistSearchResult.Parse, "Featured playlists", SearchCategory.FeaturedPlaylists),
            Type _ when categoryType == typeof(ProfileSearchResult) => ((Func<JElement, T>)(object)ProfileSearchResult.Parse, "Profiles", SearchCategory.Profiles),
            Type _ when categoryType == typeof(PodcastSearchResult) => ((Func<JElement, T>)(object)PodcastSearchResult.Parse, "Podcasts", SearchCategory.Podcasts),
            Type _ when categoryType == typeof(EpisodeSearchResult) => ((Func<JElement, T>)(object)EpisodeSearchResult.Parse, "Episodes", SearchCategory.Episodes),
            _ => throw new ArgumentOutOfRangeException(nameof(T), categoryType, "The given category type is invalid.")
        };
        string queryParams = scope.ToQueryParams(category, ignoreSpelling);

        return new((contiuationToken, cancellationToken) =>
            FetchPageAsync(query, contiuationToken, queryParams, parse, categoryTitle, cancellationToken));
    }


    /// <summary>
    /// Searches for all content types of results on YouTUbe Music.
    /// </summary>
    /// <param name="query">The query to search for.</param>
    /// <param name="scope">The scope of the search.</param>
    /// <param name="ignoreSpelling">Weither to ignore spelling suggestions.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>A <see cref="SearchPage"/> containing all items, including the top result.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public async Task<SearchPage> AllAsync(
        string query,
        SearchScope scope = SearchScope.Global,
        bool ignoreSpelling = true,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrEmpty(query, nameof(query));

        // Send request
        KeyValuePair<string, object?>[] payload =
        [
            new("query", query),
            new("params", scope.ToQueryParams(null, ignoreSpelling)),
        ];

        string response = await client.RequestHandler.PostAsync(Endpoints.Search, payload, ClientType.WebMusic, cancellationToken);

        // Parse response
        client.Logger?.LogInformation("[SearchService-AllAsync] Parsing response...");
        using IDisposable _ = response.ParseJson(out JElement root);

        JArray contents = root
            .Get("contents")
            .Get("tabbedSearchResultsRenderer")
            .Get("tabs")
            .Coalesce(
                item => item
                    .GetAt(0)
                    .Get("tabRenderer")
                    .Get("content")
                    .Get("sectionListRenderer")
                    .Get("contents"),
                item => item
                    .GetAt(1)
                    .Get("tabRenderer")
                    .Get("content")
                    .Get("sectionListRenderer")
                    .Get("contents"))
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

        string? categoryTitle = cardShelf
            .SelectRunTextAt("subtitle", 0);

        Func<JElement, SearchResult>? parseTopResult = categoryTitle switch
        {
            "Song" => SongSearchResult.ParseTopResult,
            "Video" => VideoSearchResult.ParseTopResult,
            "Mix" => FeaturedPlaylistSearchResult.ParseTopResult,
            "Playlist" when cardShelf.SelectIsFeaturedPlaylist() => FeaturedPlaylistSearchResult.ParseTopResult,
            "Playlist" => CommunityPlaylistSearchResult.ParseTopResult,
            "Album" or "EP" or "Single" => AlbumSearchResult.ParseTopResult,
            "Artist" => ArtistSearchResult.ParseTopResult,
            "Profile" => null, // never found any top result profile; If u did, CREATE AN ISSUE plz :3
            "Podcast" => null, // bruh; YT returns top result podcasts in a "musicShelfRenderer". cba rn to parse that shi frfr
            "Episode" => EpisodeSearchResult.ParseTopResult,
            _ => null
        };
        if (parseTopResult is null)
            client.Logger?.LogWarning("[SearchService-AllAsync] Could not parse top result. Unsupported category: {category}.", categoryTitle);
        else
            topResult = parseTopResult(cardShelf);

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

                string? categoryTitle = firstDescriptionRun
                    .Get("text")
                    .AsString();
                Func<JElement, SearchResult>? parseItem = categoryTitle switch
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
                    client.Logger?.LogWarning("[SearchService-AllAsync] Could not parse related top result. Unsupported caegory: {category}.", categoryTitle);
                    return null;
                }

                if (categoryTitle != "Episodes" && content.SelectIsPodcast()) // istg youtube. why tf do u return PODCAST EPISODES in video searches???? now i gotta do that unnecessary extra saftey check
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
            .SelectMany(item => item
                .Get("musicShelfRenderer")
                .Get("contents")
                .AsArray()
                .Or(JArray.Empty)
                .Select(item =>
                {
                    if (!item.Contains("musicResponsiveListItemRenderer", out JElement content))
                        return null;

                    string? categoryTitle = content
                        .Get("flexColumns")
                        .GetAt(1)
                        .Get("musicResponsiveListItemFlexColumnRenderer")
                        .SelectRunTextAt("text", 0);
                    Func<JElement, SearchResult>? parseItem = categoryTitle switch
                    {
                        "Song" => SongSearchResult.Parse,
                        "Video" => VideoSearchResult.Parse,
                        "Playlist" when content.SelectIsFeaturedPlaylist() => FeaturedPlaylistSearchResult.Parse,
                        "Playlist" => CommunityPlaylistSearchResult.Parse,
                        "Album" => AlbumSearchResult.Parse,
                        "Artist" => ArtistSearchResult.Parse,
                        "Profile" => ProfileSearchResult.Parse,
                        "Podcast" => PodcastSearchResult.Parse,
                        "Episode" => EpisodeSearchResult.Parse,
                        _ => null
                    };
                    if (parseItem is null)
                    {
                        client.Logger?.LogWarning("[SearchService-AllAsync] Could not parse result. Unsupported caegory: {category}.", categoryTitle);
                        return null;
                    }

                    return parseItem(content);
                }))
            .Where(Syntax.IsNotNull)
            .Cast<SearchResult>()
            .ToList();

        return new(results, topResult, relatedTopResults);
    }


    /// <summary>
    /// Gets search, history and result suggestions for a specific input on YouTube Music.
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
        using IDisposable _ = response.ParseJson(out JElement root);
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
            .Where(item => item
                .Contains("musicResponsiveListItemRenderer"))
            .Select(item =>
            {
                JElement content = item
                    .Get("musicResponsiveListItemRenderer");

                string? category = content
                    .Get("flexColumns")
                    .GetAt(1)
                    .Get("musicResponsiveListItemFlexColumnRenderer")
                    .SelectRunTextAt("text", 0);
                Func<JElement, SearchResult>? parseItem = category switch
                {
                    "Song" => SongSearchResult.ParseSuggestion,
                    "Video" => VideoSearchResult.ParseSuggestion,
                    "Mix" => FeaturedPlaylistSearchResult.ParseSuggestion,
                    "Playlist" when content.SelectIsFeaturedPlaylist() => FeaturedPlaylistSearchResult.ParseSuggestion,
                    "Playlist" => CommunityPlaylistSearchResult.ParseSuggestion,
                    "Album" or "EP" or "Single" => AlbumSearchResult.ParseSuggestion,
                    "Artist" => ArtistSearchResult.ParseSuggestion,
                    "Profile" => null, // never found any search suggestion profiles; If u did, CREATE AN ISSUE plz :3
                    "Podcast" => PodcastSearchResult.ParseSuggestion,
                    "Episode" => null, // never found any search suggestion episodes; If u did, CREATE AN ISSUE plz :3
                    _ => null,
                };
                if (parseItem is null && // // bruh; YT cmon why dont u have a description text for artists??? now i gotta do that dumb fallback again >:(
                    content
                        .SelectNavigationBrowseId()
                        .IsNotNull(out string? browseId) &&
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
    /// Removes the specific input from the search history suggestions on YouTube Music.
    /// </summary>
    /// <remarks>
    /// The user must be authenticated.
    /// </remarks>
    /// <param name="input">The input to get remove from the suggestions.</param>
    /// <param name="cancellationToken">The token to cancel this action.</param>
    /// <exception cref="ArgumentException">Occurs when the <c>input</c> is <see langword="null"/> or empty.</exception>
    /// <exception cref="InvalidOperationException">Occurs when the search suggestion could not be found in the history or it was not processed.</exception>
    /// <exception cref="InvalidOperationException">Occurs when the user is not authenticated.</exception>
    /// <exception cref="NullReferenceException">Occurs when a the <c>input</c> could not be found in the search history suggestions.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public async Task RemoveSuggestionAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrEmpty(input, nameof(input));
        Ensure.Authenticated(client);

        // Send suggestions request
        client.Logger?.LogInformation("[SearchService-RemoveSuggestionAsync] Getting suggestions...");
        KeyValuePair<string, object?>[] suggestionsPayload =
        [
            new("input", input)
        ];

        string suggestionsResponse = await client.RequestHandler.PostAsync(Endpoints.SearchSuggestions, suggestionsPayload, ClientType.WebMusic, cancellationToken);

        // Parse suggestions response
        client.Logger?.LogInformation("[SearchService-RemoveSuggestionAsync] Parsing suggestions response...");
        using IDisposable _ = suggestionsResponse.ParseJson(out JElement suggestionsRoot);

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
            throw new InvalidOperationException($"Could not find search suggestion '{input}' in history to remove.");
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
        using IDisposable _1 = removeResponse.ParseJson(out JElement removeRoot);

        bool isProcessed = removeRoot
            .Get("feedbackResponses")
            .GetAt(0)
            .Get("isProcessed")
            .AsBool()
            .Or(false);
        if (!isProcessed)
        {
            client.Logger?.LogError("[SearchService-RemoveSuggestionAsync] Remove search suggestion '{input}' not processed.", input);
            throw new InvalidOperationException($"Remove search suggestion '{input}' not processed.");
        }
    }
}