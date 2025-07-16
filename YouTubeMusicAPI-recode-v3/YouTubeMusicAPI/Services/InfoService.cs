using Microsoft.Extensions.Logging;
using YouTubeMusicAPI.Http;

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

}