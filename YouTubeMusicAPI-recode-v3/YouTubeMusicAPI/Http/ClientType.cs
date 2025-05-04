namespace YouTubeMusicAPI.Http;

/// <summary>
/// Represents the type of YouTube Music client to be used when making API requests.
/// </summary>
internal enum ClientType
{
    /// <summary>
    /// Represents no client.
    /// </summary>
    /// <remarks>
    None,
    
    /// <summary>
    /// Represents the YouTube Music Web client.
    /// </summary>
    /// <remarks>
    /// Notes:<br/>
    /// - Proof of Origin Token for streaming required.
    /// </remarks>
    WebMusic,

    /// <summary>
    /// Represents the YouTube iOS client.
    /// </summary>
    /// <remarks>
    /// Notes:<br/>
    /// - Account cookies not supported.<br/>
    /// - Provides HLS (m3u8) streaming formats .
    /// </remarks>
    IOS,

    /// <summary>
    /// Represents the YouTube TV client.
    /// </summary>
    /// <remarks>
    /// </remarks>
    Tv
}