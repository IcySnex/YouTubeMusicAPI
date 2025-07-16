using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Services;

namespace YouTubeMusicAPI;

/// <summary>
/// Client for interacting with the YouTube Music API.
/// </summary>
public class YouTubeMusicClient
{
    /// <summary>
    /// Creates a new instance of the <see cref="YouTubeMusicClient"/> class.
    /// </summary>
    /// <param name="config">The configuration for this YouTube Music client</param>
    public YouTubeMusicClient(
        YouTubeMusicConfig? config = null)
    {
        Config = config ?? new();

        RequestHandler requestHandler = new(Config.GeographicalLocation, Config.Authenticator, Config.HttpClient, Config.Logger);
        Search = new(requestHandler, Config.Logger);
        Info = new(requestHandler, Config.Logger);
    }


    /// <summary>
    /// The configuration for this YouTube Music client.
    /// </summary>
    public YouTubeMusicConfig Config { get; }


    /// <summary>
    /// The service used to search on YouTube Music.
    /// </summary>
    public SearchService Search { get; }

    /// <summary>
    /// The service used to get information from YouTube Music.
    /// </summary>
    public InfoService Info { get; }
}