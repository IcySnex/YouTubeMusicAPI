using Microsoft.Extensions.Logging;
using YouTubeMusicAPI.Http.Authentication;

namespace YouTubeMusicAPI;

/// <summary>
/// Contains configuration options for the YouTube Music client.
/// </summary>
/// <remarks>
/// Create an instance of the <see cref="YouTubeMusicConfig"/> class.
/// </remarks>
public class YouTubeMusicConfig
{
    /// <summary>
    /// The geographical location for YouTube Music requests.
    /// </summary>
    public string GeographicalLocation { get; init; } = "US";

    IAuthenticator? authenticator = null;
    /// <summary>
    /// The authenticator used to authenticate HTTP requests sent to YouTube Music.
    /// </summary>
    public IAuthenticator Authenticator
    {
        get => authenticator ??= new AnonymousAuthenticator();
        init => authenticator = value;
    }

    HttpClient? httpClient = null;
    /// <summary>
    /// The HTTP client used to send requests to YouTube Music.
    /// </summary>
    public HttpClient HttpClient
    {
        get => httpClient ??= new();
        init => httpClient = value;
    }

    /// <summary>
    /// The logger used to provide progress and error messages.
    /// </summary>
    public ILogger? Logger { get; init; }
}