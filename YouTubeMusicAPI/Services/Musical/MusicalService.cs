using Microsoft.Extensions.Logging;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Musical;

/// <summary>
/// Service which handles getting lyrics for songs/videos from YouTube Music
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="MusicalService"/> class.
/// </remarks>
/// <param name="client">The shared base client.</param>
internal sealed class MusicalService(
    YouTubeMusicClient client)
{
    readonly YouTubeMusicClient client = client;


    /// <summary>
    /// Gets the related content for the song/video on YouTube Music.
    /// </summary>
    /// <param name="browseId">The relations browse ID, obtained from <c>MediaItemService.GetAsync()</c>.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>The <see cref="MusicalRelations"/> containing the related content for the song/video.</returns>
    public async Task<MusicalRelations> GetRelationsAsync(
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
        client.Logger?.LogInformation("[RelationsService-GetAsync] Parsing response...");
        using IDisposable _ = response.ParseJson(out JElement root);

        return MusicalRelations.Parse(root);
    }

    /// <summary>
    /// Gets the lyrics for a song/video on YouTube Music.
    /// </summary>
    /// <param name="browseId">The lyrics browse ID, obtained from <c>MediaItemService.GetAsync()</c>.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>The <see cref="MusicalLyrics"/> containing the either synced or plain lyrics text.</returns>
    public async Task<MusicalLyrics> GetLyricsAsync(
        string browseId,
        CancellationToken cancellationToken = default)
    {
        Ensure.NotNullOrEmpty(browseId, nameof(browseId));

        // Send request
        KeyValuePair<string, object?>[] payload =
        [
            new("browseId", browseId),
        ];

        string response = await client.RequestHandler.PostAsync(Endpoints.Browse, payload, ClientType.IOSMusic, cancellationToken);

        // Parse response
        client.Logger?.LogInformation("[LyricsService-GetAsync] Parsing response...");
        using IDisposable _ = response.ParseJson(out JElement root);

        JElement lyricsData = root
            .Get("contents")
            .Get("elementRenderer")
            .Get("newElement")
            .Get("type")
            .Get("componentType")
            .Get("model")
            .Get("timedLyricsModel")
            .Get("lyricsData");
        if (lyricsData.IsUndefined)
        {
            client.Logger?.LogError("[LyricsService-GetAsync] The provided song/video does not have available lyrics.");
            throw new InvalidOperationException("The provided song/video does not have available lyrics.");
        }

        return MusicalLyrics.Parse(lyricsData);
    }
}