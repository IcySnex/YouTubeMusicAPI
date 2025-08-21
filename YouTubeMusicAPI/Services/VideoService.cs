using Microsoft.Extensions.Logging;
using System.Text.Json;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Models.Videos;
using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services;

/// <summary>
/// Service which handles getting information about videos from YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="VideoService"/> class.
/// </remarks>
/// <param name="client">The shared base client.</param>
public sealed class VideoService(
    YouTubeMusicClient client) : MediaItemService(client)
{
    /// <summary>
    /// Creates a paginator that searches for videos on YouTube Music.
    /// </summary>
    /// <remarks>
    /// Convenience method that forwards to <see cref="SearchService.ByCategoryAsync{T}(string, SearchScope, bool)"/>.
    /// </remarks>
    /// <param name="query">The query to search for.</param>
    /// <param name="scope">The scope of the search.</param>
    /// <param name="ignoreSpelling">Weither to ignore spelling suggestions.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="VideoSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<VideoSearchResult> SearchAsync(
        string query,
        SearchScope scope = SearchScope.Global,
        bool ignoreSpelling = true) =>
        client.Search.ByCategoryAsync<VideoSearchResult>(query, scope, ignoreSpelling);


    /// <summary>
    /// Gets detailed information about a videos from YouTube Music.
    /// </summary>
    /// <param name="id">The ID of the videos.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>The <see cref="VideoInfo"/> containing the information.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>id</c> is <see langword="null"/> or empty.</exception>
    /// <exception cref="InvalidOperationException">Occurs when the provided ID does not correspond to a videos.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public async Task<VideoInfo> GetAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));

        // Send request
        KeyValuePair<string, object?>[] payload =
        [
            new("videoId", id)
        ];

        string response = await client.RequestHandler.PostAsync(Endpoints.Next, payload, ClientType.WebMusic, cancellationToken);

        // Parse response
        client.Logger?.LogInformation("[VideoService-GetAsync] Parsing response...");
        using JsonDocument json = JsonDocument.Parse(response);
        JElement root = new(json.RootElement);

        bool isSong = root
            .Get("playerOverlays")
            .Get("playerOverlayRenderer")
            .Get("browserMediaSession")
            .Get("browserMediaSessionRenderer")
            .Contains("album");
        if (isSong)
        {
            client.Logger?.LogError("[VideoService-GetAsync] The provided ID does not correspond to a video. Use 'client.Songs.GetAsync(string id)' instead.");
            throw new InvalidOperationException("The provided ID does not correspond to a video. Use 'client.Songs.GetAsync(string id)' instead.");
        }

        VideoInfo song = VideoInfo.Parse(root);
        return song;
    }
}