namespace YouTubeMusicAPI.Models.Streaming;

/// <summary>
/// Represents YouTube Music media streaming data
/// </summary>
/// <remarks>
/// Creates a new StreamingData
/// </remarks>
/// <param name="streamInfo">The media stream info</param>
/// <param name="isLiveContent">Whether the song or video is live content or not</param>
/// <param name="expiresIn">The amount of time the streams expire in</param>
/// <param name="hlsManifestUrl">The url to the HLS manifest</param>
public class StreamingData(
    StreamInfo[] streamInfo,
    bool isLiveContent,
    TimeSpan expiresIn,
    string? hlsManifestUrl)
{
    /// <summary>
    /// The media stream info
    /// </summary>
    public StreamInfo[] StreamInfo { get; } = streamInfo;

    /// <summary>
    /// Whether the song or video is live content or not
    /// </summary>
    public bool IsLiveContent { get; } = isLiveContent;

    /// <summary>
    /// The amount of time the streams expire in
    /// </summary>
    public TimeSpan ExpiresIn { get; } = expiresIn;

    /// <summary>
    /// The url to the HLS manifest
    /// </summary>
    public string? HlsManifestUrl { get; } = hlsManifestUrl;
}