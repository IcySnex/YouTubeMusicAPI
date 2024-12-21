namespace YouTubeMusicAPI.Models.Streaming;

/// <summary>
/// Represents YouTube Music media streaming data
/// </summary>
/// <remarks>
/// Creates a new StreamingData
/// </remarks>
/// <param name="streamInfo">The media stream info</param>
/// <param name="expiresIn">The amount of time the streams expire in</param>
/// <param name="hlsManifestUrl">The url to the HLS manifest</param>
/// <param name="aspectRatio">The aspect ratio of the streams</param>
public class StreamingData(
    MediaStreamInfo[] streamInfo,
    TimeSpan expiresIn,
    string hlsManifestUrl,
    double aspectRatio)
{
    /// <summary>
    /// The media stream info
    /// </summary>
    public MediaStreamInfo[] StreamInfo { get; } = streamInfo;

    /// <summary>
    /// The amount of time the streams expire in
    /// </summary>
    public TimeSpan ExpiresIn { get; } = expiresIn;

    /// <summary>
    /// The url to the HLS manifest
    /// </summary>
    public string HlsManifestUrl { get; } = hlsManifestUrl;

    /// <summary>
    /// The aspect ratio of the streams
    /// </summary>
    public double AspectRatio { get; } = aspectRatio;
}