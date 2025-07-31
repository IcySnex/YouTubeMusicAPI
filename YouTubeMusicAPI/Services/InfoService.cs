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
public sealed class InfoService : YouTubeMusicService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InfoService"/> class.
    /// </summary>
    /// <param name="requestHandler">The handler for all outgoing HTTP requests.</param>
    /// <param name="logger">The logger used to provide progress and error messages.</param>
    internal InfoService(
        RequestHandler requestHandler,
        ILogger? logger = null) : base(requestHandler, logger) { }


    /// <summary>
    /// Gets detailed information about a song from YouTube Music.
    /// </summary>
    /// <param name="id">The ID of the song.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>The <see cref="SongInfo"/> containing the information.</returns>
    /// <exception cref="ArgumentException">Occurrs when the <c>id</c> is <see langword="null"/> or empty.</exception>
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

        string response = await requestHandler.PostAsync(Endpoints.Next, payload, ClientType.WebMusic, cancellationToken);

        // Parse response
        using JsonDocument json = JsonDocument.Parse(response);
        JsonElement rootElement = json.RootElement;

        bool isSong = rootElement
            .GetProperty("playerOverlays")
            .GetProperty("playerOverlayRenderer")
            .GetProperty("browserMediaSession")
            .GetProperty("browserMediaSessionRenderer")
            .TryGetProperty("album", out _);
        if (!isSong)
            throw new ArgumentException("The provided ID does not correspond to a song. Use 'GetVideoAsync' instead.", nameof(id));

        SongInfo song = SongInfo.Parse(rootElement);
        return song;
    }

    /// <summary>
    /// Gets the credits (like performers, writers, producers etc.) of a song from YouTube Music.
    /// </summary>
    /// <param name="id">The ID of the song.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>The <see cref="SongCredits"/> containing the information about the credits.</returns>
    /// <exception cref="ArgumentException">Occurrs when the <c>id</c> is <see langword="null"/> or empty.</exception>
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

        string response = await requestHandler.PostAsync(Endpoints.Browse, payload, ClientType.WebMusic, cancellationToken);

        // Parse response
        using JsonDocument json = JsonDocument.Parse(response);
        JsonElement rootElement = json.RootElement;

        JsonElement dialogRenderer = rootElement
            .GetProperty("onResponseReceivedActions")
            .GetElementAt(0)
            .GetProperty("openPopupAction")
            .GetProperty("popup")
            .GetProperty("dismissableDialogRenderer");

        if (!dialogRenderer.TryGetProperty("sections", out _))
            throw new ArgumentException("The provided ID does not correspond to a song with available credits.", nameof(id));

        SongCredits credits = SongCredits.Parse(dialogRenderer);
        return credits;
    }
}