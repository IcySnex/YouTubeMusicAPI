namespace YouTubeMusicAPI.Models;

/// <summary>
/// Contains information about playback tracking URLs used by YouTube
/// </summary>
public class PlaybackTracking
{
    const string CpnCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";

    readonly static Random CpnRandom = new();


    /// <summary>
    /// Creates a new PlaybackTracking
    /// </summary>
    /// <param name="videostatsPlaybackUrl">The URL used to report general playback statistics during video playback</param>
    /// <param name="videostatsDelayplayUrl">The URL used to report delayed playback stats</param>
    /// <param name="videostatsWatchtimeUrl">The URL used to report cumulative watch time</param>
    /// <param name="videostatsScheduledFlushWalltimes">The URL used to report cumulative watch time</param>
    /// <param name="videostatsDefaultFlushIntervalSeconds">Wall-clock times (relative to playback start) at which stats are flushed and sent to YouTube</param>
    /// <param name="playbackTrackingUrl">The default interval between automatic stat flushes, if no specific wall times are given</param>
    /// <param name="qualityOfExperienceUrl">URL used to report Quality of Experience (QoE) metrics such as buffering, resolution, and playback smoothness</param>
    /// <param name="adTelemetryReportUrl">The URL used to report ad telemetry report (ATR) such as ad viewability and interaction tracking</param>
    public PlaybackTracking(
        string videostatsPlaybackUrl,
        string videostatsDelayplayUrl,
        string videostatsWatchtimeUrl,
        TimeSpan[] videostatsScheduledFlushWalltimes,
        TimeSpan videostatsDefaultFlushIntervalSeconds,
        string playbackTrackingUrl,
        string qualityOfExperienceUrl,
        string adTelemetryReportUrl)
    {
        VideostatsPlaybackUrl = videostatsPlaybackUrl;
        VideostatsDelayplayUrl = videostatsDelayplayUrl;
        VideostatsWatchtimeUrl = videostatsWatchtimeUrl;
        VideostatsScheduledFlushWalltimes = videostatsScheduledFlushWalltimes;
        VideostatsDefaultFlushIntervalSeconds = videostatsDefaultFlushIntervalSeconds;
        PlaybackTrackingUrl = playbackTrackingUrl;
        QualityOfExperienceUrl = qualityOfExperienceUrl;
        AdTelemetryReportUrl = adTelemetryReportUrl;

        // Generate client playback nonce
        for (int i = 0; i < 16; i++)
            ClientPlaybackNonce += CpnCharacters[CpnRandom.Next(0, 256) & 63];
    }


    /// <summary>
    /// The URL used to report general playback statistics during video playback
    /// </summary>
    public string VideostatsPlaybackUrl { get; set; }

    /// <summary>
    /// The URL used to report delayed playback stats
    /// </summary>
    public string VideostatsDelayplayUrl { get; set; }

    /// <summary>
    /// The URL used to report cumulative watch time
    /// </summary>
    public string VideostatsWatchtimeUrl { get; set; }

    /// <summary>
    /// Wall-clock times (relative to playback start) at which stats are flushed and sent to YouTube
    /// </summary>
    public TimeSpan[] VideostatsScheduledFlushWalltimes { get; set; }

    /// <summary>
    /// The default interval between automatic stat flushes, if no specific wall times are given
    /// </summary>
    public TimeSpan VideostatsDefaultFlushIntervalSeconds { get; set; }

    /// <summary>
    /// The URL used to report playback tracking (ptracking), including metrics for view count attribution and visibility
    /// </summary>
    public string PlaybackTrackingUrl { get; set; }

    /// <summary>
    /// URL used to report Quality of Experience (QoE) metrics such as buffering, resolution, and playback smoothness
    /// </summary>
    public string QualityOfExperienceUrl { get; set; }

    /// <summary>
    /// The URL used to report ad telemetry report (ATR) such as ad viewability and interaction tracking
    /// </summary>
    public string AdTelemetryReportUrl { get; set; }


    /// <summary>
    /// The randomly generated client playback nonce
    /// </summary>
    public string ClientPlaybackNonce { get; } = "";
}