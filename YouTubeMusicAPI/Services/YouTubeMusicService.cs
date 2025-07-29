using Microsoft.Extensions.Logging;
using YouTubeMusicAPI.Http;

namespace YouTubeMusicAPI.Services;

/// <summary>
/// Base class for YouTube Music service components.
/// </summary>
public abstract class YouTubeMusicService
{
    internal readonly RequestHandler requestHandler;
    internal readonly ILogger? logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="YouTubeMusicService"/> class.
    /// </summary>
    /// <param name="requestHandler">The request handler.</param>
    /// <param name="logger">The logger used to provide progress and error messages.</param>
    internal YouTubeMusicService(
        RequestHandler requestHandler,
        ILogger? logger = null)
    {
        this.requestHandler = requestHandler;
        this.logger = logger;
    }
}