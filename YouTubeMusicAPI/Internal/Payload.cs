namespace YouTubeMusicAPI.Internal;

/// <summary>
/// Contains methods to create new YouTube Music request payloads
/// </summary>
public static class Payload
{
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
                    clientVersion = "1.20211213.00.00",
                    browserName = "Chrome",
                    browserVersion = "130.0.0.0",
                    osName = "Windows",
                    osVersion = "10.0",
                    userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36,gzip(gfe)",
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
    /// Creates a new payload which mimics a YouTube mobile client
    /// </summary>
    /// <param name="geographicalLocation">The region for the payload</param>
    /// <param name="visitorData">The visitor data</param>
    /// <param name="poToken">The proof of origin token</param>
    /// <param name="items">The items to add to the request payload</param>
    /// <returns>The new payload</returns>
    public static Dictionary<string, object> Mobile(
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
                    clientName = "iOS",
                    clientVersion = "19.45.4",
                    deviceMake = "Apple",
                    deviceModel = "iPhone16,2",
                    osName = "IOS",
                    platform = "MOBILE",
                    osVersion = "18.1.0.22B83",
                    userAgent = "com.google.ios.youtube/19.45.4 (iPhone16,2; U; CPU iOS 18_1_0 like Mac OS X; US)",
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

    
}