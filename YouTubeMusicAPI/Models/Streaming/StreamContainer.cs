namespace YouTubeMusicAPI.Models.Streaming;

/// <summary>
/// Represents a YouTube Music stream container
/// </summary>
/// <remarks>
/// Creates a new StreamContainer
/// </remarks>
/// <param name="containsAudio">Whether the stream container contains audio</param>
/// <param name="containsVideo">Whether the stream container contains video</param>
/// <param name="format">The format of the stream container, e.g., "mp4"</param>
/// <param name="codecs">The codecs used in the stream container</param>
public class StreamContainer(
    bool containsAudio,
    bool containsVideo,
    string format,
    string codecs)
{
    /// <summary>
    /// Whether the stream container contains audio
    /// </summary>
    public bool ContainsAudio { get; } = containsAudio;

    /// <summary>
    /// Whether the stream container contains video
    /// </summary>
    public bool ContainsVideo { get; } = containsVideo;

    /// <summary>
    /// The media format of the stream container
    /// </summary>
    public string Format { get; } = format;

    /// <summary>
    /// The codecs used in the stream container
    /// </summary>
    public string Codecs { get; } = codecs;
}