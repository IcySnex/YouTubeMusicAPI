namespace YouTubeMusicAPI.Models;

/// <summary>
/// Contains information about playback tracking URLs used by YouTube
/// </summary>
/// <remarks>
/// Creates a new PlaybackTracking
/// </remarks>
/// <param name="videostatsPlaybackUrl">The URL used to report general playback statistics during video playback</param>
/// <param name="videostatsDelayplayUrl">The URL used to report delayed playback stats</param>
/// <param name="videostatsWatchtimeUrl">The URL used to report cumulative watch time</param>
/// <param name="videostatsScheduledFlushWalltimes">The URL used to report cumulative watch time</param>
/// <param name="videostatsDefaultFlushIntervalSeconds">Wall-clock times (relative to playback start) at which stats are flushed and sent to YouTube</param>
/// <param name="playbackTrackingUrl">The default interval between automatic stat flushes, if no specific wall times are given</param>
/// <param name="qualityOfExperienceUrl">URL used to report Quality of Experience (QoE) metrics such as buffering, resolution, and playback smoothness</param>
/// <param name="adTelemetryReportUrl">The URL used to report ad telemetry report (ATR) such as ad viewability and interaction tracking</param>
public class PlaybackTracking(
    string videostatsPlaybackUrl,
    string videostatsDelayplayUrl,
    string videostatsWatchtimeUrl,
    TimeSpan[] videostatsScheduledFlushWalltimes,
    TimeSpan videostatsDefaultFlushIntervalSeconds,
    string playbackTrackingUrl,
    string qualityOfExperienceUrl,
    string adTelemetryReportUrl)
{
    /// <summary>
    /// The URL used to report general playback statistics during video playback
    /// </summary>
    public string VideostatsPlaybackUrl { get; set; } = videostatsPlaybackUrl;

    /// <summary>
    /// The URL used to report delayed playback stats
    /// </summary>
    public string VideostatsDelayplayUrl { get; set; } = videostatsDelayplayUrl;

    /// <summary>
    /// The URL used to report cumulative watch time
    /// </summary>
    public string VideostatsWatchtimeUrl { get; set; } = videostatsWatchtimeUrl;

    /// <summary>
    /// Wall-clock times (relative to playback start) at which stats are flushed and sent to YouTube
    /// </summary>
    public TimeSpan[] VideostatsScheduledFlushWalltimes { get; set; } = videostatsScheduledFlushWalltimes;

    /// <summary>
    /// The default interval between automatic stat flushes, if no specific wall times are given
    /// </summary>
    public TimeSpan VideostatsDefaultFlushIntervalSeconds { get; set; } = videostatsDefaultFlushIntervalSeconds;

    /// <summary>
    /// The URL used to report playback tracking (ptracking), including metrics for view count attribution and visibility
    /// </summary>
    public string PlaybackTrackingUrl { get; set; } = playbackTrackingUrl;

    /// <summary>
    /// URL used to report Quality of Experience (QoE) metrics such as buffering, resolution, and playback smoothness
    /// </summary>
    public string QualityOfExperienceUrl { get; set; } = qualityOfExperienceUrl;

    /// <summary>
    /// The URL used to report ad telemetry report (ATR) such as ad viewability and interaction tracking
    /// </summary>
    public string AdTelemetryReportUrl { get; set; } = adTelemetryReportUrl;
}