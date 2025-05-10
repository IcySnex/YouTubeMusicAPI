using YouTubeMusicAPI.Http;

namespace YouTubeMusicAPI;

/// <summary>
/// Client for interacting with the YouTube Music API.
/// </summary>
public class YouTubeMusicClient
{
    readonly RequestHandler requestHandler;

    /// <summary>
    /// The configuration options for this YouTube Music client.
    /// </summary>
    public YouTubeMusicConfig Config { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="YouTubeMusicClient"/> class.
    /// </summary>
    /// <param name="config"></param>
    public YouTubeMusicClient(
        YouTubeMusicConfig? config = null)
    {
        Config = config ?? new();

        requestHandler = new(Config.HttpClient, Config.Authenticator, Config.Logger);
    }

}