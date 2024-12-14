namespace YouTubeMusicAPI.Models.Streams;

/// <summary>
/// Represents a YouTube Music audio stream
/// </summary>
public class AudioStream : MediaStream
{
    /// <summary>
    /// Creates a new AudioStream
    /// </summary>
    /// <param name="itag">The encoding identifier for the media stream</param>
    /// <param name="url">The URL to access the media stream</param>
    /// <param name="container">The container details for the media stream</param>
    /// <param name="lastModifedAt">The last modified date of the media stream</param>
    /// <param name="approximatedDuration">The approximated duration of the stream</param>
    /// <param name="contentLenght">The content length of the media stream in bytes</param>
    /// <param name="bitrate">The bitrate of the stream</param>
    /// <param name="averageBitrate">The average bitrate of the stream</param>
    /// <param name="quality">The quality of the audio stream</param>
    /// <param name="sampleRate">The sample rate of the audio stream</param>
    /// <param name="channels">The number of audio channels</param>
    /// <param name="loudnessDb">The loudness of the audio stream in decibels</param>
    public AudioStream(
        int itag,
        string url,
        MediaContainer container,
        DateTime lastModifedAt,
        TimeSpan approximatedDuration,
        long contentLenght,
        long bitrate,
        long averageBitrate,
        string quality,
        int sampleRate,
        int channels,
        double loudnessDb) : base(itag, url, container, lastModifedAt, approximatedDuration, contentLenght, bitrate, averageBitrate)
    {
        Quality = quality;
        SampleRate = sampleRate;
        Channels = channels;
        LoudnessDb = loudnessDb;
    }

    /// <summary>
    /// The quality of the audio stream
    /// </summary>
    public string Quality { get; }

    /// <summary>
    /// The sample rate of the audio stream
    /// </summary>
    public int SampleRate { get; }

    /// <summary>
    /// The number of audio channels
    /// </summary>
    public int Channels { get; }

    /// <summary>
    /// The loudness of the audio stream in decibels
    /// </summary>
    public double LoudnessDb { get; }
}