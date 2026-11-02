namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Contains navigation endpoint information (browse ID and optional params) to browse full YouTube Music content
/// </summary>
/// <remarks>
/// Encapsulates YouTube Music's InnerTube navigation endpoint (<c>browseEndpoint</c>).
/// The <see cref="BrowseId"/> identifies the target resource or shelf (such as an artist's full discography page),
/// while <see cref="Params"/> provides protobuf-encoded filter or continuation options.
/// For example, on artist discography shelves, YouTube Music shares the same artist discography browse ID
/// but supplies distinct <c>params</c> values to filter the view specifically to albums versus singles and EPs.
/// Omitting or passing <see langword="null"/> for <c>params</c> fetches the full, unfiltered discography view.
/// </remarks>
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
    /// <remarks>
    /// Serves as a filter or continuation parameter for the browse endpoint.
    /// When querying artist discography endpoints, specifying this parameter restricts the returned grid
    /// to specific release types (e.g. albums only or singles and EPs only).
    /// If <see langword="null"/> or empty, YouTube Music returns all releases without category filtering.
    /// </remarks>
    public string? Params { get; } = @params;
}
