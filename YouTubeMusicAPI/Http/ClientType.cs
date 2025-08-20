namespace YouTubeMusicAPI.Http;

/// <summary>
/// Represents the type of YouTube Music client used for making API requests.
/// </summary>
public enum ClientType
{
    /// <summary>
    /// Represents no client.
    /// </summary>
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
    /// Represents the YouTube Music iOS client.
    /// </summary>
    /// <remarks>
    /// Notes:<br/>
    /// - Account cookies not supported.<br/>
    /// - Provides HLS (m3u8) streaming formats.
    /// </remarks>
    IOSMusic,
}