namespace YouTubeMusicAPI.Models.Streaming;

/// <summary>
/// Represents a YouTube Music media stream container
/// </summary>
/// <remarks>
/// Creates a new MediaContainer
/// </remarks>
/// <param name="containsAudio">Whether the container contains audio</param>
/// <param name="containsVideo">Whether the container contains video</param>
/// <param name="format">The format of the media container, e.g., "mp4"</param>
/// <param name="codecs">The codecs used in the media container</param>
public class MediaContainer(
    bool containsAudio,
    bool containsVideo,
    string format,
    string codecs)
{
    /// <summary>
    /// Whether the container contains audio
    /// </summary>
    public bool ContainsAudio { get; } = containsAudio;

    /// <summary>
    /// Whether the container contains video
    /// </summary>
    public bool ContainsVideo { get; } = containsVideo;

    /// <summary>
    /// The media format of the container
    /// </summary>
    public string Format { get; } = format;

    /// <summary>
    /// The codecs used in the media container
    /// </summary>
    public string Codecs { get; } = codecs;
}