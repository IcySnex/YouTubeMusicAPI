using Microsoft.Extensions.Logging;
using System.Text.Json;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Models.MediaItems;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services;

/// <summary>
/// Service which handles getting information about media items from YouTube Music
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="MediaItemService"/> class.
/// </remarks>
/// <param name="client">The shared base client.</param>
public abstract class MediaItemService(
    YouTubeMusicClient client)
{
    /// <summary>
    /// The shared base client.
    /// </summary>
    protected readonly YouTubeMusicClient client = client;


    /// <summary>
    /// Gets the lyrics for a media item from YouTube Music.
    /// </summary>
    /// <param name="browseId">The lyrics browse ID, obtained from <c>GetAsync()</c>.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Lyrics> GetLyricsAsync(
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
        client.Logger?.LogInformation("[MediaItemService-GetLyricsAsync] Parsing response...");
        using JsonDocument json = JsonDocument.Parse(response);
        JElement root = new(json.RootElement);

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
            client.Logger?.LogError("[MediaItemService-GetLyricsAsync] The provided ID does not correspond to a song with available lyrics.");
            throw new InvalidOperationException("The provided ID does not correspond to a song with available lyrics.");
        }

        bool isPlain = lyricsData
            .Get("staticLayout")
            .AsBool()
            .Or(false);
        return isPlain ? PlainLyrics.Parse(lyricsData) : SyncedLyrics.Parse(lyricsData);
    }
}