using Microsoft.Extensions.Logging;
using System.Text.Json;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Info;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services;

/// <summary>
/// Service used to get information from YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="SearchService"/> class.
/// </remarks>
/// <param name="client">The shared base client.</param>
public sealed class InfoService(
    YouTubeMusicClient client)
{
    readonly YouTubeMusicClient client = client;

    /// <summary>
    /// Gets detailed information about a song from YouTube Music.
    /// </summary>
    /// <param name="id">The ID of the song.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>The <see cref="SongInfo"/> containing the information.</returns>
    /// <exception cref="ArgumentException">Occurrs when the <c>id</c> is <see langword="null"/> or empty or when the provided ID does not correspond to a song.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public async Task<SongInfo> GetSongAsync(
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
        client.Logger?.LogInformation("[InfoService-GetSongAsync] Parsing response...");
        using JsonDocument json = JsonDocument.Parse(response);

        bool isSong = json.RootElement
            .GetProperty("playerOverlays")
            .GetProperty("playerOverlayRenderer")
            .GetProperty("browserMediaSession")
            .GetProperty("browserMediaSessionRenderer")
            .TryGetProperty("album", out _);

        if (!isSong)
        {
            client.Logger?.LogError("[InfoService-GetSongAsync] The provided ID does not correspond to a song. Use 'GetVideoAsync' instead.");
            throw new ArgumentException("The provided ID does not correspond to a song. Use 'GetVideoAsync' instead.", nameof(id));
        }

        SongInfo song = SongInfo.Parse(json.RootElement);
        return song;
    }

    /// <summary>
    /// Gets the credits (like performers, writers, producers etc.) of a song from YouTube Music.
    /// </summary>
    /// <param name="id">The ID of the song.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>The <see cref="SongCredits"/> containing the information about the credits.</returns>
    /// <exception cref="ArgumentException">Occurrs when the <c>id</c> is <see langword="null"/> or empty or when the provided ID does not correspond to a song with available credits.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public async Task<SongCredits> GetSongCreditsAsync(
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
        client.Logger?.LogInformation("[InfoService-GetSongCreditsAsync] Parsing response...");
        using JsonDocument json = JsonDocument.Parse(response);

        JsonElement dialogRenderer = json.RootElement
            .GetProperty("onResponseReceivedActions")
            .GetPropertyAt(0)
            .GetProperty("openPopupAction")
            .GetProperty("popup")
            .GetProperty("dismissableDialogRenderer");

        if (!dialogRenderer.TryGetProperty("sections", out _))
        {
            client.Logger?.LogError("[InfoService-GetSongCreditsAsync] The provided ID does not correspond to a song with available credits.");
            throw new ArgumentException("The provided ID does not correspond to a song with available credits.", nameof(id));
        }

        SongCredits credits = SongCredits.Parse(dialogRenderer);
        return credits;
    }
}