namespace YouTubeMusicAPI.Models.Streaming;

/// <summary>
/// Represents YouTube Music audio stream info
/// </summary>
/// <remarks>
/// Creates a new AudioStreamInfo
/// </remarks>
/// <param name="itag">The encoding identifier for the stream</param>
/// <param name="url">The URL to access the stream</param>
/// <param name="container">The container details for the stream</param>
/// <param name="lastModifedAt">The last modified date of the stream</param>
/// <param name="duration">The approximated duration of the stream</param>
/// <param name="contentLenght">The content length of the stream in bytes</param>
/// <param name="bitrate">The bitrate of the stream</param>
/// <param name="quality">The quality of the audio stream</param>
/// <param name="sampleRate">The sample rate of the audio stream</param>
/// <param name="channels">The number of audio channels</param>
/// <param name="loudnessDb">The loudness of the audio stream in decibels</param>
public class AudioStreamInfo(
    int itag,
    string url,
    MediaContainer container,
    DateTime lastModifedAt,
    TimeSpan duration,
    long contentLenght,
    int bitrate,
    string quality,
    int sampleRate,
    int channels,
    double loudnessDb) : StreamInfo(itag, url, container, lastModifedAt, duration, contentLenght, bitrate)
{
    /// <summary>
    /// The quality of the audio stream
    /// </summary>
    public string Quality { get; } = quality;

    /// <summary>
    /// The sample rate of the audio stream
    /// </summary>
    public int SampleRate { get; } = sampleRate;

    /// <summary>
    /// The number of audio channels
    /// </summary>
    public int Channels { get; } = channels;

    /// <summary>
    /// The loudness of the audio stream in decibels
    /// </summary>
    public double LoudnessDb { get; } = loudnessDb;
}