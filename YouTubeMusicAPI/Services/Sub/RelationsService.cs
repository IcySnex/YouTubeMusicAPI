using Microsoft.Extensions.Logging;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Models.Relations;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Sub;

/// <summary>
/// Service which handles getting related content for songs/videos from YouTube Music
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="RelationsService"/> class.
/// </remarks>
/// <param name="client">The shared base client.</param>
internal sealed class RelationsService(
    YouTubeMusicClient client)
{
    readonly YouTubeMusicClient client = client;


    /// <summary>
    /// Gets the related content for the song/video on YouTube Music.
    /// </summary>
    /// <param name="browseId">The relations browse ID, obtained from <c>MediaItemService.GetAsync()</c>.</param>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>The <see cref="SongVideoRelations"/> containing the related content for the song/video.</returns>
    public async Task<SongVideoRelations> GetAsync(
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

        return SongVideoRelations.Parse(root);
    }
}