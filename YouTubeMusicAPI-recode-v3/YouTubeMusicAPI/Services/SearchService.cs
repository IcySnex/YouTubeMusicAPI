using Microsoft.Extensions.Logging;
using YouTubeMusicAPI.Http;

namespace YouTubeMusicAPI.Services;

/// <summary>
/// Service used to search on YouTube Music.
/// </summary>
public sealed class SearchService : YouTubeMusicService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SearchService"/> class.
    /// </summary>
    /// <param name="requestHandler"></param>
    /// <param name="logger">The logger used to provide progress and error messages.</param>
    internal SearchService(
        RequestHandler requestHandler,
        ILogger? logger = null) : base(requestHandler, logger) { }
}