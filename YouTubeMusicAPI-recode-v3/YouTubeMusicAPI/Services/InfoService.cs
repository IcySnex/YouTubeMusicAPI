using Microsoft.Extensions.Logging;
using System.Text.Json;
using YouTubeMusicAPI.Http;
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
    /// <param name="requestHandler">The request handler.</param>
    /// <param name="logger">The logger used to provide progress and error messages.</param>
    internal InfoService(
        RequestHandler requestHandler,
        ILogger? logger = null) : base(requestHandler, logger) { }


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
}