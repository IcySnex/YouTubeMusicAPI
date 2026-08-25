namespace YouTubeMusicAPI.Internal;

/// <summary>
/// Contains methods to create new YouTube Music request payloads
/// </summary>
public static class Payload
{
    internal const string VisionOsClientName = "VISIONOS";
    internal const string VisionOsClientVersion = "1.02";
    internal const string VisionOsUserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 15_7_3) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/26.0 Safari/605.1.15";

    /// <summary>
    /// Creates a new payload which mimics a YouTube Music web client
    /// </summary>
    /// <param name="geographicalLocation">The region for the payload</param>
    /// <param name="visitorData">The visitor data</param>
    /// <param name="poToken">The proof of origin token</param>
    /// <param name="signatureTimestamp">The player signature timestamp (required when deciphering stream urls)</param>
    /// <param name="items">The items to add to the request payload</param>
    /// <returns>The new payload</returns>
    public static Dictionary<string, object> WebRemix(
        string geographicalLocation,
        string? visitorData,
        string? poToken,
        int? signatureTimestamp,
        (string key, object? value)[] items)
    {
        Dictionary<string, object> payload = new()
        {
            ["context"] = new
            {
                client = new
                {
                    clientName = "WEB_REMIX",
                    clientVersion = "1.20260715.04.00",
                    browserName = "Chrome",
                    browserVersion = "142.0.0.0",
                    osName = "Windows",
                    osVersion = "10.0",
                    userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36,gzip(gfe)",
                    timeZone = "UTC",
                    utcOffsetMinutes = 0,
                    hl = "en",
                    gl = geographicalLocation,
                    visitorData
                }
            },
            ["playbackContext"] = new
            {
                contentPlaybackContext = new
                {
                    signatureTimestamp
                }
            },
            ["serviceIntegrityDimensions"] = new
            {
                poToken
            }
        };
        foreach ((string key, object? value) in items)
            if (value is not null)
                payload[key] = value;

        return payload;
    }

    /// <summary>
    /// Creates a new payload which mimics the YouTube visionOS client
    /// </summary>
    /// <param name="geographicalLocation">The region for the payload</param>
    /// <param name="visitorData">The visitor data</param>
    /// <param name="poToken">The proof of origin token</param>
    /// <param name="items">The items to add to the request payload</param>
    /// <returns>The new payload</returns>
    public static Dictionary<string, object> VisionOs(
        string geographicalLocation,
        string? visitorData,
        string? poToken,
        (string key, object? value)[] items)
    {
        Dictionary<string, object> payload = new()
        {
            ["context"] = new
            {
                client = new
                {
                    clientName = VisionOsClientName,
                    clientVersion = VisionOsClientVersion,
                    deviceMake = "Apple",
                    deviceModel = "RealityDevice17,1",
                    osName = "visionOS",
                    osVersion = "26.5.23O471",
                    userAgent = VisionOsUserAgent,
                    timeZone = "UTC",
                    utcOffsetMinutes = 0,
                    hl = "en",
                    gl = geographicalLocation,
                    visitorData
                }
            },
            ["serviceIntegrityDimensions"] = new
            {
                poToken
            }
        };
        foreach ((string key, object? value) in items)
            if (value is not null)
                payload[key] = value;

        return payload;
    }

    /// <summary>
    /// Creates a new payload which mimics a cipher-less YouTube client
    /// </summary>
    /// <param name="geographicalLocation">The region for the payload</param>
    /// <param name="visitorData">The visitor data</param>
    /// <param name="poToken">The proof of origin token</param>
    /// <param name="items">The items to add to the request payload</param>
    /// <returns>The new payload</returns>
    [Obsolete($"Use {nameof(VisionOs)} instead.")]
    public static Dictionary<string, object> Mobile(
        string geographicalLocation,
        string? visitorData,
        string? poToken,
        (string key, object? value)[] items) =>
        VisionOs(geographicalLocation, visitorData, poToken, items);
}
