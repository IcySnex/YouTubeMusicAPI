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
    /// <param name="items">The items to add to the request payload</param>
    /// <returns>The new payload</returns>
    public static Dictionary<string, object> Web(
        string geographicalLocation,
        (string key, object? value)[] items)
    {
        Dictionary<string, object> payload = new()
        {
            ["context"] = new
            {
                client = new
                {
                    clientName = "WEB_REMIX",
                    clientVersion = "1.20240918.01.00",
                    browserName = "Chrome",
                    browserVersion = "130.0.0.0",
                    osName = "Windows",
                    osVersion = "10.0",
                    userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36,gzip(gfe)",
                    timeZone = "UTC",
                    utcOffsetMinutes = 0,
                    hl = "en",
                    gl = geographicalLocation,
                }
            }
        };
        foreach ((string key, object? value) in items)
            if (value is not null)
                payload[key] = value;

        return payload;
    }

    /// <summary>
    /// Creates a new payload which mimics a YouTube Music mobile client
    /// </summary>
    /// <param name="geographicalLocation">The region for the payload</param>
    /// <param name="items">The items to add to the request payload</param>
    /// <returns>The new payload</returns>
    public static Dictionary<string, object> Mobile(
        string geographicalLocation,
        (string key, object? value)[] items)
    {
        Dictionary<string, object> payload = new()
        {
            ["context"] = new
            {
                client = new
                {
                    clientName = "IOS",
                    clientVersion = "19.29.1",
                    deviceMake = "Apple",
                    deviceModel = "iPhone16,2",
                    osName = "iPhone",
                    osVersion  = "17.5.1.21F90",
                    userAgent = "com.google.ios.youtube/19.29.1 (iPhone16,2; U; CPU iOS 17_5_1 like Mac OS X;)",
                    timeZone = "UTC",
                    utcOffsetMinutes = 0,
                    hl = "en",
                    gl = geographicalLocation,
                }
            }
        };
        foreach ((string key, object? value) in items)
            if (value is not null)
                payload[key] = value;

        return payload;
    }
}