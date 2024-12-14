namespace YouTubeMusicAPI.Models.Streams;

/// <summary>
/// Represents a YouTube Music media stream
/// </summary>
public abstract class MediaStream
{
    /// <summary>
    /// Creates a new MediaStream
    /// </summary>
    /// <param name="itag">The encoding identifier for the media stream</param>
    /// <param name="url">The URL to access the media stream</param>
    /// <param name="container">The container details for the media stream</param>
    /// <param name="lastModifedAt">The last modified date of the media stream</param>
    /// <param name="approximatedDuration">The approximated duration of the stream</param>
    /// <param name="contentLenght">The content length of the media stream in bytes</param>
    /// <param name="bitrate">The bitrate of the stream</param>
    /// <param name="averageBitrate">The average bitrate of the stream</param>
    protected MediaStream(
        int itag,
        string url,
        MediaContainer container,
        DateTime lastModifedAt,
        TimeSpan approximatedDuration,
        long contentLenght,
        long bitrate,
        long averageBitrate)
    {
        Itag = itag;
        Url = url;
        Container = container;
        LastModifedAt = lastModifedAt;
        ApproximatedDuration = approximatedDuration;
        ContentLenght = contentLenght;
        Bitrate = bitrate;
        AverageBitrate = averageBitrate;
    }

    /// <summary>
    /// The encoding identifier for the media stream
    /// </summary>
    public int Itag { get; }

    /// <summary>
    /// The URL to access the media stream
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// The container details for the media stream
    /// </summary>
    public MediaContainer Container { get; }

    /// <summary>
    /// The last modified date of the media stream
    /// </summary>
    public DateTime LastModifedAt { get; }

    /// <summary>
    /// The approximated duration of the stream
    /// </summary>
    public TimeSpan ApproximatedDuration { get; }

    /// <summary>
    /// The content length of the media stream in bytes
    /// </summary>
    public long ContentLenght { get; }

    /// <summary>
    /// The bitrate of the stream
    /// </summary>
    public long Bitrate { get; }

    /// <summary>
    /// The average bitrate of the stream
    /// </summary>
    public long AverageBitrate { get; }
}