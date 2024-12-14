namespace YouTubeMusicAPI.Models.Streams;

/// <summary>
/// Represents a YouTube Music media stream container
/// </summary>
public class MediaContainer
{
    /// <summary>
    /// Creates a new MediaContainer
    /// </summary>
    /// <param name="isAudio">Whether the container is for audio</param>
    /// <param name="isVideo">Whether the container is for video</param>
    /// <param name="format">The format of the media container, e.g., "mp4"</param>
    /// <param name="codecs">The codecs used in the media container</param>
    public MediaContainer(
        bool isAudio,
        bool isVideo,
        string format,
        string codecs)
    {
        IsAudio = isAudio;
        IsVideo = isVideo;
        Format = format;
        Codecs = codecs;
    }

    /// <summary>
    /// Whether the container is for audio
    /// </summary>
    public bool IsAudio { get; }

    /// <summary>
    /// Whether the container is for video
    /// </summary>
    public bool IsVideo { get; }

    /// <summary>
    /// The media format of the container
    /// </summary>
    public string Format { get; }

    /// <summary>
    /// The codecs used in the media container
    /// </summary>
    public string Codecs { get; }
}