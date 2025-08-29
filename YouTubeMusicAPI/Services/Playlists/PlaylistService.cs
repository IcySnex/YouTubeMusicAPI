using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Services.Albums;
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


    // <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    // <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    async Task<Page<PlaylistItem>> FetchItemsPageAsync(
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

        Page<PlaylistItem> page = PlaylistInfo.ParseItemsPage(root);
        return page;
    }

    // <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    // <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    async Task<Page<PlaylistItem>> FetchItemsRadioPageAsync(
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

        Page<PlaylistItem> page = PlaylistInfo.ParseItemsRadioPage(root);
        return page;
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
    /// <remarks>
    /// This method does not perform any network operations. It is pseudo-async to preserve consistency with similar methods in the library, e.g. <see cref="AlbumService.GetBrowseIdAsync(string, CancellationToken)"/>.
    /// </remarks>
    /// <param name="id">The id of the playlist.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>The browse ID of the playlist.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>id</c> is <see langword="null"/> or empty.</exception>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Preserve consistency with similar methods in the library")]
    public Task<string> GetBrowseIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));

        if (id.StartsWith("VL"))
            return Task.FromResult(id);

        // Create browse ID
        client.Logger?.LogInformation("[PlaylistService-GetBrowseIdAsync] Creating browse ID...");

        string browseId = $"VL{id}";
        return Task.FromResult(browseId);
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

        if (isRadio)
        {
            PaginatedAsyncEnumerable<PlaylistItem> items = new(
                (contiuationToken, cancellationToken) => FetchItemsRadioPageAsync(browseId[2..], contiuationToken, cancellationToken),
                PlaylistInfo.ParseItemsRadioPage(root));

            PlaylistInfo playlist = PlaylistInfo.ParseRadio(root, items);
            return playlist;
        }
        else
        {
            PaginatedAsyncEnumerable<PlaylistItem> items = new(
                (contiuationToken, cancellationToken) => FetchItemsPageAsync(browseId, contiuationToken, cancellationToken),
                PlaylistInfo.ParseItemsPage(root));

            PlaylistInfo playlist = PlaylistInfo.Parse(root, items);
            return playlist;
        }
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