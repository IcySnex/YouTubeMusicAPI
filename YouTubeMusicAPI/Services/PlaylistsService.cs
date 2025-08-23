using Microsoft.Extensions.Logging;
using System.Text.Json;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Models.Playlists;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services;

/// <summary>
/// Service which handles getting information about playlists from YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="PlaylistsService"/> class.
/// </remarks>
/// <param name="client">The shared base client.</param>
public sealed class PlaylistsService(
    YouTubeMusicClient client)
{
    readonly YouTubeMusicClient client = client;

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
    /// Gets detailed information about a playlist from YouTube Music.
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

        // Send request
        KeyValuePair<string, object?>[] payload =
        [
            new("browseId", browseId),
        ];

        string response = await client.RequestHandler.PostAsync(Endpoints.Browse, payload, ClientType.WebMusic, cancellationToken);

        // Parse response
        client.Logger?.LogInformation("[PlaylistService-GetAsync] Parsing response...");
        using JsonDocument json = JsonDocument.Parse(response);
        JElement root = new(json.RootElement);

        PlaylistInfo playlist = PlaylistInfo.Parse(root);
        return playlist;
    }


    /// <summary>
    /// Gets the related content for the playlist.
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
        using JsonDocument firstJson = JsonDocument.Parse(firstResponse);
        JElement firstRoot = new(firstJson.RootElement);

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
            using JsonDocument relatedJson = JsonDocument.Parse(relatedResponse);
            JElement relatedRoot = new(relatedJson.RootElement);

            relations = PlaylistRelations.Parse(firstSection, relatedRoot);
        }

        return relations;
    }
}