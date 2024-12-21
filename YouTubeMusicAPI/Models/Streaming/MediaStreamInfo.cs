using YouTubeMusicAPI.Internal;

namespace YouTubeMusicAPI.Models.Streaming;

/// <summary>
/// Represents YouTube Music media stream info
/// </summary>
/// <remarks>
/// Creates a new MediaStreamInfo
/// </remarks>
/// <param name="itag">The encoding identifier for the media stream</param>
/// <param name="url">The URL to access the media stream</param>
/// <param name="container">The container details for the media stream</param>
/// <param name="lastModifedAt">The last modified date of the media stream</param>
/// <param name="duration">The approximated duration of the stream</param>
/// <param name="contentLenght">The content length of the media stream in bytes</param>
/// <param name="bitrate">The bitrate of the stream</param>
public abstract class MediaStreamInfo(
    int itag,
    string url,
    MediaContainer container,
    DateTime lastModifedAt,
    TimeSpan duration,
    long contentLenght,
    int bitrate)
{
    /// <summary>
    /// The encoding identifier for the media stream
    /// </summary>
    public int Itag { get; } = itag;

    /// <summary>
    /// The URL to access the media stream
    /// </summary>
    public string Url { get; } = url;

    /// <summary>
    /// The container details for the media stream
    /// </summary>
    public MediaContainer Container { get; } = container;

    /// <summary>
    /// The last modified date of the media stream
    /// </summary>
    public DateTime LastModifedAt { get; } = lastModifedAt;

    /// <summary>
    /// The approximated duration of the stream
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// The content length of the media stream in bytes
    /// </summary>
    public long ContentLenght { get; } = contentLenght;

    /// <summary>
    /// The bitrate of the stream
    /// </summary>
    public int Bitrate { get; } = bitrate;


    /// <summary>
    /// Gets the stream from the media URL
    /// </summary>
    /// <param name="cancellationToken">The token to cancel this action</param>
    /// <returns>A stream</returns>
    public async Task<Stream> GetStreamAsync(
        CancellationToken cancellationToken = default)
    {
        MediaStream stream = new(this);
        await stream.InitializeAsync(cancellationToken);

        return stream;
    }
}