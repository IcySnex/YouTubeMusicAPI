using Microsoft.Extensions.Logging;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Services.Albums;
using YouTubeMusicAPI.Services.Search;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Artists;

/// <summary>
/// Service which handles getting information about artists from YouTube Music.
/// </summary>
public sealed class ArtistService
{
    readonly YouTubeMusicClient client;

    /// <summary>
    /// Creates a new instance of the <see cref="ArtistService"/> class.
    /// </summary>
    /// <param name="client">The shared base client.</param>
    internal ArtistService(
        YouTubeMusicClient client)
    {
        this.client = client;
    }


    /// <summary>
    /// Creates a paginator that searches for artists on YouTube Music.
    /// </summary>
    /// <remarks>
    /// Convenience method that forwards to <see cref="SearchService.ByCategoryAsync{T}(string, SearchScope, bool)"/>.
    /// </remarks>
    /// <param name="query">The query to search for.</param>
    /// <param name="scope">The scope of the search.</param>
    /// <param name="ignoreSpelling">Weither to ignore spelling suggestions.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="ArtistSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<ArtistSearchResult> SearchAsync(
        string query,
        SearchScope scope = SearchScope.Global,
        bool ignoreSpelling = true) =>
        client.Search.ByCategoryAsync<ArtistSearchResult>(query, scope, ignoreSpelling);


    /// <summary>
    /// Gets detailed information about an artist on YouTube Music.
    /// </summary>
    /// <param name="browseId">The browse ID of the artist.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>The <see cref="ArtistInfo"/> containing the information.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>browseId</c> is <see langword="null"/> or empty or it is not a valid browse ID.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public async Task<ArtistInfo> GetAsync(
        string browseId,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrEmpty(browseId, nameof(browseId));

        // Send request
        KeyValuePair<string, object?>[] payload =
        [
            new("browseId", browseId),
        ];

        string response = await client.RequestHandler.PostAsync(Endpoints.Browse, payload, ClientType.WebMusic, cancellationToken);

        // Parse response
        client.Logger?.LogInformation("[ArtistService-GetAsync] Parsing response...");
        using IDisposable _ = response.ParseJson(out JElement root);

        bool isArtist = root
            .Get("header")
            .Contains("musicImmersiveHeaderRenderer");
        if (!isArtist)
        {
            client.Logger?.LogError("[ArtistService-GetAsync] The provided browse ID does not correspond to an artist. Use 'client.Profiles.GetAsync(string browseId)' instead.");
            throw new InvalidOperationException("The provided browse ID does not correspond to an artist. Use 'client.Profiles.GetAsync(string browseId)' instead.");
        }

        ArtistInfo artist = ArtistInfo.Parse(root);
        return artist;
    }

    /// <summary>
    /// Gets the albums of an artist
    /// </summary>
    /// <param name="browseId">The browse id of artist</param>
    /// <param name="params">The browse params, will decide whether to return albums or singles EPs</param>
    /// <param name="sortingOrder">The sorting order of the returned albums</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>A list of the <see cref="ArtistAlbum"/> containing the information</returns>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was canceled.</exception>
    public async Task<ArtistAlbums> GetAlbumsAsync(
        string browseId,
        string @params,
        AlbumSortingOrder sortingOrder = AlbumSortingOrder.Default,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrEmpty(browseId, nameof(browseId));
        Ensure.NotNullOrEmpty(@params, nameof(@params));

        KeyValuePair<string, object?>[] payload =
        [
            new("browseId", browseId),
            new("params", @params)
        ];

        Task<string> MakeRequest() =>
            client.RequestHandler.PostAsync(Endpoints.Browse, payload, ClientType.WebMusic, cancellationToken);

        const string methodName = $"{nameof(AlbumService)}-{nameof(GetAlbumsAsync)}";
        var logger = client.Logger;

        string response = await MakeRequest();

        logger?.LogInformation($"[{methodName}] Parsing response...");
        using IDisposable _ = response.ParseJson(out JElement root);

        ArtistAlbums ParseAlbums(bool isContinuationResponse = false) 
            => ArtistAlbums.Parse(root, browseId, @params, sortingOrder, isContinuationResponse);

        if (sortingOrder is AlbumSortingOrder.Default)
        {
            return ParseAlbums();
        }

        string? continuationToken = null;
        var sortingOptions = root.Get("contents")
            .Get("singleColumnBrowseResultsRenderer")
            .Get("tabs")
            .GetAt(0)
            .Get("tabRenderer")
            .Get("content")
            .Get("sectionListRenderer")
            .Get("header")
            .Get("musicSideAlignedItemRenderer")
            .Get("endItems")
            .GetAt(0)
            .Get("musicSortFilterButtonRenderer")
            .Get("menu")
            .GetMultiSelectMenu()
            .Get("options")
            .AsArray()
            .Or(JArray.Empty);

        var sortingOrderString = sortingOrder.ToString().ToLower();

        foreach (var option in sortingOptions)
        {
            var optionText = option.Get("musicMultiSelectMenuItemRenderer")
                .Get("title").GetFirstRun().GetText()
                .AsString()
                .OrThrow();
            if (optionText.Equals(sortingOrderString, StringComparison.InvariantCultureIgnoreCase))
            {
                continuationToken = option
                    .GetMultiSelectMenuItem()
                    .Get("selectedCommand")
                    .Get("commandExecutorCommand")
                    .Get("commands")
                    .AsArray().OrThrow()
                    .Last()
                    .Get("browseSectionListReloadEndpoint")
                    .Get("continuation")
                    .Get("reloadContinuationData")
                    .Get("continuation")
                    .AsString();
                break;
            }
        }

        if (continuationToken is null)
        {
            return ParseAlbums();
        }

        logger?.LogInformation($"[{methodName}] Resending request to get sorted albums...");
        payload = [new("continuation", continuationToken)];
        response = await MakeRequest();
        using IDisposable __ = response.ParseJson(out root);
        return ParseAlbums(isContinuationResponse: true);
    }
}