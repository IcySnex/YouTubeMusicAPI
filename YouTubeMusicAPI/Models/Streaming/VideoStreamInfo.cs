namespace YouTubeMusicAPI.Models.Streaming;

/// <summary>
/// Represents YouTube Music video stream info
/// </summary>
/// <remarks>
/// Creates a new VideoStreamInfo
/// </remarks>
/// <param name="itag">The encoding identifier for the media stream</param>
/// <param name="url">The URL to access the media stream</param>
/// <param name="container">The container details for the media stream</param>
/// <param name="lastModifedAt">The last modified date of the media stream</param>
/// <param name="duration">The approximated duration of the stream</param>
/// <param name="contentLenght">The content length of the media stream in bytes</param>
/// <param name="bitrate">The bitrate of the stream</param>
/// <param name="averageBitrate">The average bitrate of the stream</param>
/// <param name="framerate">The framerate of the video stream</param>
/// <param name="quality">The quality of the video stream</param>
/// <param name="qualityLabel">The quality label of the video stream</param>
/// <param name="width">The width of the video stream</param>
/// <param name="height">The height of the video stream</param>
public class VideoStreamInfo(
    int itag,
    string url,
    MediaContainer container,
    DateTime lastModifedAt,
    TimeSpan duration,
    long contentLenght,
    int bitrate,
    int averageBitrate,
    int framerate,
    string quality,
    string qualityLabel,
    int width,
    int height) : MediaStreamInfo(itag, url, container, lastModifedAt, duration, contentLenght, bitrate, averageBitrate)
{
    /// <summary>
    /// The framerate of the video stream
    /// </summary>
    public int Framerate { get; } = framerate;

    /// <summary>
    /// The quality of the video stream
    /// </summary>
    public string Quality { get; } = quality;

    /// <summary>
    /// The quality label of the video stream
    /// </summary>
    public string QualityLabel { get; } = qualityLabel;

    /// <summary>
    /// The width of the video stream
    /// </summary>
    public int Width { get; } = width;

    /// <summary>
    /// The height of the video stream
    /// </summary>
    public int Height { get; } = height;
}