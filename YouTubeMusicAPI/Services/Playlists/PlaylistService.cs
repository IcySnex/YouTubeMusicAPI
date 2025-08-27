using Microsoft.Extensions.Logging;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Services.Relations;
using YouTubeMusicAPI.Services.Search;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Playlists;

/// <summary>
/// Service which handles getting information about playlists from YouTube Music.
/// </summary>
public sealed class PlaylistService
{
    readonly YouTubeMusicClient client;
    /// <summary>
    /// Creates a new instance of the <see cref="PlaylistService"/> class.
    /// </summary>
    /// <param name="client">The shared base client.</param>
    internal PlaylistService(
        YouTubeMusicClient client)
    {
        this.client = client;
    }


    /// <summary>
    /// Creates a paginator that searches for community playlists on YouTube Music.
    /// </summary>
    /// <remarks>
    /// Convenience method that forwards to <see cref="SearchService.ByCategoryAsync{T}(string, SearchScope, bool)"/>.
    /// </remarks>
    /// <param name="query">The query to search for.</param>
    /// <param name="scope">The scope of the search.</param>
    /// <param name="ignoreSpelling">Weither to ignore spelling suggestions.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="CommunityPlaylistSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<CommunityPlaylistSearchResult> SearchCommunityAsync(
        string query,
        SearchScope scope = SearchScope.Global,
        bool ignoreSpelling = true) =>
        client.Search.ByCategoryAsync<CommunityPlaylistSearchResult>(query, scope, ignoreSpelling);

    /// <summary>
    /// Creates a paginator that searches for featured playlists on YouTube Music.
    /// </summary>
    /// <remarks>
    /// Convenience method that forwards to <see cref="SearchService.ByCategoryAsync{T}(string, SearchScope, bool)"/>.
    /// </remarks>
    /// <param name="query">The query to search for.</param>
    /// <param name="scope">The scope of the search.</param>
    /// <param name="ignoreSpelling">Weither to ignore spelling suggestions.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="FeaturedPlaylistSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<FeaturedPlaylistSearchResult> SearchFeaturedAsync(
        string query,
        SearchScope scope = SearchScope.Global,
        bool ignoreSpelling = true) =>
        client.Search.ByCategoryAsync<FeaturedPlaylistSearchResult>(query, scope, ignoreSpelling);


    /// <summary>
    /// Gets the browse ID for a playlist on YouTube Music.
    /// </summary>
    /// <param name="id">The id of the playlist.</param>
    /// <returns>The browse ID of the playlist.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>id</c> is <see langword="null"/> or empty.</exception>
    public string GetBrowseId(
        string id)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));

        client.Logger?.LogInformation("[PlaylistService-GetBrowseId] Getting browse ID for playlist...");

        if (id.StartsWith("VL"))
            return id;
        return $"VL{id}";
    }


    /// <summary>
    /// Gets detailed information about a playlist on YouTube Music.
    /// </summary>
    /// <param name="browseId">The browse ID of the playlist.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>The <see cref="PlaylistInfo"/> containing the information.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>browseId</c> is <see langword="null"/> or empty or it is not a valid browse ID.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public async Task<PlaylistInfo> GetAsync(
        string browseId,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrEmpty(browseId, nameof(browseId));
        Ensure.StartsWith(browseId, "VL", nameof(browseId));

        bool isRadio = browseId.StartsWith("VLRDAM");

        // Send request
        KeyValuePair<string, object?>[] payload =
        [
            new("browseId", browseId),
            new("playlistId", browseId[2..])
        ];

        string response = await client.RequestHandler.PostAsync(isRadio ? Endpoints.Next : Endpoints.Browse, payload, ClientType.WebMusic, cancellationToken);

        // Parse response
        client.Logger?.LogInformation("[PlaylistService-GetAsync] Parsing {radioInfo}response...", isRadio ? "radio " : "");
        using IDisposable _ = response.ParseJson(out JElement root);

        PlaylistInfo playlist = isRadio ? PlaylistInfo.ParseRadio(root) : PlaylistInfo.Parse(root);
        return playlist;
    }

    /// <summary>
    /// Creates an async paginator that fetches items from a playlist on YouTube Music.
    /// </summary>
    /// <param name="browseId">The browse ID of the playlist.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="PlaylistItem"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>browseId</c> is <see langword="null"/> or empty or it is not a valid browse ID.</exception>
    public PaginatedAsyncEnumerable<PlaylistItem> GetItemsAsync(
        string browseId)
    {
        Ensure.NotNullOrEmpty(browseId, nameof(browseId));
        Ensure.StartsWith(browseId, "VL", nameof(browseId));

        bool isRadio = browseId.StartsWith("VLRDAM");

        // <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
        // <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
        async Task<Page<PlaylistItem>> FetchPageAsync(
            string browseId,
            string? continuationToken,
            CancellationToken cancellationToken = default)
        {
            // Send request
            KeyValuePair<string, object?>[] payload =
            [
                new("browseId", browseId),
                new("continuation", continuationToken),
            ];

            string response = await client.RequestHandler.PostAsync(Endpoints.Browse, payload, ClientType.WebMusic, cancellationToken);

            // Parse response
            client.Logger?.LogInformation("[PlaylistService-FetchPageAsync] Parsing response...");
            using IDisposable _ = response.ParseJson(out JElement root);

            JElement contents = root
                .Coalesce(
                    item => item
                        .Get("contents")
                        .Get("twoColumnBrowseResultsRenderer")
                        .Get("secondaryContents")
                        .Get("sectionListRenderer")
                        .Get("contents")
                        .GetAt(0)
                        .Get("musicPlaylistShelfRenderer")
                        .Get("contents"),
                    item => item
                        .Get("onResponseReceivedActions")
                        .GetAt(0)
                        .Get("appendContinuationItemsAction")
                        .Get("continuationItems"));

            string? nextContinuationToken = contents
                .GetAt(contents.ArrayLength - 1)
                .Get("continuationItemRenderer")
                .Get("continuationEndpoint")
                .Get("continuationCommand")
                .Get("token")
                .AsString();

            List<PlaylistItem> result = contents
                .AsArray()
                .Or(JArray.Empty)
                .Where(item => item
                    .Contains("musicResponsiveListItemRenderer"))
                .Select(item => item
                    .Get("musicResponsiveListItemRenderer"))
                .Select(PlaylistItem.Parse)
                .ToList();

            return new(result, nextContinuationToken);
        }

        // <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
        // <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
        async Task<Page<PlaylistItem>> FetchRadioPageAsync(
            string id,
            string? continuationToken,
            CancellationToken cancellationToken = default)
        {
            // Send request
            KeyValuePair<string, object?>[] payload =
            [
                new("playlistId", id),
                new("continuation", continuationToken),
            ];

            string response = await client.RequestHandler.PostAsync(Endpoints.Next, payload, ClientType.WebMusic, cancellationToken);

            // Parse response
            client.Logger?.LogInformation("[PlaylistService-FetchRadioPageAsync] Parsing radio response...");
            using IDisposable _ = response.ParseJson(out JElement root);

            JElement playlistPanel = root
                .Coalesce(
                    item => item
                        .Get("contents")
                        .Get("singleColumnMusicWatchNextResultsRenderer")
                        .Get("tabbedRenderer")
                        .Get("watchNextTabbedResultsRenderer")
                        .Get("tabs")
                        .GetAt(0)
                        .Get("tabRenderer")
                        .Get("content")
                        .Get("musicQueueRenderer")
                        .Get("content")
                        .Get("playlistPanelRenderer"),
                    item => item
                        .Get("continuationContents")
                        .Get("playlistPanelContinuation"));

            string? nextContinuationToken = playlistPanel
                .Get("continuations")
                .GetAt(0)
                .Get("nextRadioContinuationData")
                .Get("continuation")
                .AsString();

            List<PlaylistItem> result = playlistPanel
                .Get("contents")
                .AsArray()
                .Or(JArray.Empty)
                .Where(item => item
                    .Contains("playlistPanelVideoRenderer"))
                .Select(item => item
                    .Get("playlistPanelVideoRenderer"))
                .Select(PlaylistItem.ParseRadio)
                .ToList();

            return new(result, nextContinuationToken);
        }

        return isRadio
            ? new((contiuationToken, cancellationToken) =>
                FetchRadioPageAsync(browseId[2..], contiuationToken, cancellationToken))
            : new((contiuationToken, cancellationToken) =>
                FetchPageAsync(browseId, contiuationToken, cancellationToken));
    }


    /// <summary>
    /// Gets the related content for the playlist on YouTube Music.
    /// </summary>
    /// <remarks>
    /// Only available when <see cref="PlaylistInfo.IsMix"/> is <see langword="false"/>."/>
    /// </remarks>
    /// <param name="playlist">The playlist to get the relations for.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>The <see cref="PlaylistRelations"/> containing the related content for the playlist.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>playlist</c> does not support getting related playlists.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public async Task<PlaylistRelations> GetRelationsAsync(
        PlaylistInfo playlist,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrEmpty(playlist.RelationsContinuationToken, nameof(playlist.RelationsContinuationToken));

        PlaylistRelations relations;

        // Send first request (either suggestions or directly related)
        string firstUrl = Endpoints.Browse
            .SetQueryParameter("ctoken", playlist.RelationsContinuationToken)
            .SetQueryParameter("continuation", playlist.RelationsContinuationToken);

        string firstResponse = await client.RequestHandler.PostAsync(firstUrl, null, ClientType.WebMusic, cancellationToken);

        // Parse first response
        client.Logger?.LogInformation("[PlaylistService-GetRelationsAsync] Parsing first response (either suggestions or directly related)...");
        using IDisposable _ = firstResponse.ParseJson(out JElement firstRoot);

        JElement firstSection = firstRoot
            .Get("continuationContents")
            .Get("sectionListContinuation");

        string? secondContinuationToken = firstSection
            .Get("continuations")
            .GetAt(0)
            .Get("nextContinuationData")
            .Get("continuation")
            .AsString();
        if (secondContinuationToken is null)
        { // firstRoot was already related so ignore em suggestions
            client.Logger?.LogInformation("[PlaylistService-GetRelationsAsync] First response was related, skipping suggestions...");

            relations = PlaylistRelations.Parse(default, firstRoot);
        }
        else
        { // firstRoot is suggestions, lezz fetch them related
            client.Logger?.LogInformation("[PlaylistService-GetRelationsAsync] First response was suggestions, getting related...");

            // Send related request
            string relatedUrl = Endpoints.Browse
                .SetQueryParameter("ctoken", secondContinuationToken)
                .SetQueryParameter("continuation", secondContinuationToken);

            string relatedResponse = await client.RequestHandler.PostAsync(relatedUrl, null, ClientType.WebMusic, cancellationToken);

            // Parse first response
            client.Logger?.LogInformation("[PlaylistService-GetRelationsAsync] Parsing related response...");
            using IDisposable _1 = relatedResponse.ParseJson(out JElement relatedRoot);

            relations = PlaylistRelations.Parse(firstSection, relatedRoot);
        }

        return relations;
    }
}