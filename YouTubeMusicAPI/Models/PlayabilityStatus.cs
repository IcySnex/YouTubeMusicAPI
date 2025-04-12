namespace YouTubeMusicAPI.Models;

/// <summary>
/// Contains information about a YouTube Music song/video playability status
/// </summary>
/// <param name="isOkay">Whether the playability status is okay</param>
/// <param name="reasonForNotOkay">The reason why the playability status is not okay (null if 'IsOkay' is true)</param>
public class PlayabilityStatus(
    bool isOkay,
    string? reasonForNotOkay)
{
    /// <summary>
    /// Whether the playability status is okay
    /// </summary>
    public bool IsOkay { get; } = isOkay;

    /// <summary>
    /// The reason why the playability status is not okay
    /// (null if 'IsOkay' is true)
    /// </summary>
    public string? ReasonForNotOkay { get; } = reasonForNotOkay;
}