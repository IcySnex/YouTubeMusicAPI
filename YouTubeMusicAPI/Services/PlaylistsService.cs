using Microsoft.Extensions.Logging;
using System.Text.Json;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Models.Playlists;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Models.Songs;
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

}