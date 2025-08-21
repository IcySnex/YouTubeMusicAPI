using Microsoft.Extensions.Logging;
using System.Text.Json;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Models.Songs;
using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services;

/// <summary>
/// Service which handles getting information about songs from YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="SongService"/> class.
/// </remarks>
/// <param name="client">The shared base client.</param>
public sealed class SongService(
    YouTubeMusicClient client) : MediaItemService(client)
{
    /// <summary>
    /// Creates a paginator that searches for songs on YouTube Music.
    /// </summary>
    /// <remarks>
    /// Convenience method that forwards to <see cref="SearchService.SongsAsync(string)"/>.
    /// </remarks>
    /// <param name="query">The query to search for.</param>
    /// <returns>A <see cref="PaginatedAsyncEnumerable{T}"/> that provides asynchronous iteration over the <see cref="SongSearchResult"/>'s.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>query</c> is <see langword="null"/> or empty.</exception>
    public PaginatedAsyncEnumerable<SongSearchResult> SearchAsync(
        string query) =>
        client.Search.SongsAsync(query);


    /// <summary>
    /// Gets detailed information about a song from YouTube Music.
    /// </summary>
    /// <param name="id">The ID of the song.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>The <see cref="SongInfo"/> containing the information.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>id</c> is <see langword="null"/> or empty.</exception>
    /// <exception cref="InvalidOperationException">Occurs when the provided ID does not correspond to a song.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public async Task<SongInfo> GetAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));

        // Send request
        KeyValuePair<string, object?>[] payload =
        [
            new("videoId", id),
        ];

        string response = await client.RequestHandler.PostAsync(Endpoints.Next, payload, ClientType.WebMusic, cancellationToken);

        // Parse response
        client.Logger?.LogInformation("[SongService-GetAsync] Parsing response...");
        using JsonDocument json = JsonDocument.Parse(response);
        JElement root = new(json.RootElement);

        bool isSong = root
            .Get("playerOverlays")
            .Get("playerOverlayRenderer")
            .Get("browserMediaSession")
            .Get("browserMediaSessionRenderer")
            .Contains("album");
        if (!isSong)
        {
            client.Logger?.LogError("[SongService-GetAsync] The provided ID does not correspond to a song. Use 'client.Videos.GetAsync(string id)' instead.");
            throw new InvalidOperationException("The provided ID does not correspond to a song. Use 'client.Videos.GetAsync(string id)' instead.");
        }

        SongInfo song = SongInfo.Parse(root);
        return song;
    }

    /// <summary>
    /// Gets the credits (like performers, writers, producers etc.) of a song from YouTube Music.
    /// </summary>
    /// <param name="id">The ID of the song.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>The <see cref="SongCredits"/> containing the information about the credits.</returns>
    /// <exception cref="ArgumentException">Occurs when the <c>id</c> is <see langword="null"/> or empty.</exception>
    /// <exception cref="InvalidOperationException">Occurs when when the provided ID does not correspond to a song with available credits.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public async Task<SongCredits> GetCreditsAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));

        // Send request
        KeyValuePair<string, object?>[] payload =
        [
            new("browseId", "MPTC" + id),
        ];

        string response = await client.RequestHandler.PostAsync(Endpoints.Browse, payload, ClientType.WebMusic, cancellationToken);

        // Parse response
        client.Logger?.LogInformation("[SongService-GetCreditsAsync] Parsing response...");
        using JsonDocument json = JsonDocument.Parse(response);
        JElement root = new(json.RootElement);

        JElement dialogRenderer = root
            .Get("onResponseReceivedActions")
            .GetAt(0)
            .Get("openPopupAction")
            .Get("popup")
            .Get("dismissableDialogRenderer");
        if (!dialogRenderer.Contains("sections"))
        {
            client.Logger?.LogError("[SongService-GetCreditsAsync] The provided ID does not correspond to a song with available credits.");
            throw new InvalidOperationException("The provided ID does not correspond to a song with available credits.");
        }

        SongCredits credits = SongCredits.Parse(dialogRenderer);
        return credits;
    }
}