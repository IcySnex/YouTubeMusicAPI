namespace YouTubeMusicAPI.Models.Streams;

/// <summary>
/// Represents a YouTube Music video stream
/// </summary>
public class VideoStream : MediaStream
{
    /// <summary>
    /// Creates a new VideoStream
    /// </summary>
    /// <param name="itag">The encoding identifier for the media stream</param>
    /// <param name="url">The URL to access the media stream</param>
    /// <param name="container">The container details for the media stream</param>
    /// <param name="lastModifedAt">The last modified date of the media stream</param>
    /// <param name="approximatedDuration">The approximated duration of the stream</param>
    /// <param name="contentLenght">The content length of the media stream in bytes</param>
    /// <param name="bitrate">The bitrate of the stream</param>
    /// <param name="averageBitrate">The average bitrate of the stream</param>
    /// <param name="quality">The quality of the video stream</param>
    /// <param name="qualityLabel">The quality label of the video stream</param>
    /// <param name="width">The width of the video stream</param>
    /// <param name="height">The height of the video stream</param>
    public VideoStream(
        int itag,
        string url,
        MediaContainer container,
        DateTime lastModifedAt,
        TimeSpan approximatedDuration,
        long contentLenght,
        long bitrate,
        long averageBitrate,
        string quality,
        string qualityLabel,
        int width,
        int height) : base(itag, url, container, lastModifedAt, approximatedDuration, contentLenght, bitrate, averageBitrate)
    {
        Quality = quality;
        QualityLabel = qualityLabel;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// The quality of the video stream
    /// </summary>
    public string Quality { get; }

    /// <summary>
    /// The quality label of the video stream
    /// </summary>
    public string QualityLabel { get; }

    /// <summary>
    /// The width of the video stream
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// The height of the video stream
    /// </summary>
    public int Height { get; }
}