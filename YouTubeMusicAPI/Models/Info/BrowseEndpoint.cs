namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Contains navigation endpoint information (browse ID and optional params) to browse full content
/// </summary>
/// <param name="browseId">The browse ID of the endpoint</param>
/// <param name="params">The optional browse parameters</param>
public class BrowseEndpoint(
    string browseId,
    string? @params = null)
{
    /// <summary>
    /// The browse ID of the endpoint
    /// </summary>
    public string BrowseId { get; } = browseId;

    /// <summary>
    /// The optional browse parameters
    /// </summary>
    public string? Params { get; } = @params;
}
