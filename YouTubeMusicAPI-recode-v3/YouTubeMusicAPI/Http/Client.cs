namespace YouTubeMusicAPI.Http;

/// <summary>
/// Represents the client used for making requests to the YouTube API.
/// </summary>
/// <param name="hl">The HL parameter used for setting the language of the request.</param>
/// <param name="gl">The GL parameter used for setting the region of the request.</param>
/// <param name="clientName">The name of the client making the request.</param>
/// <param name="clientVersion">The version of the client making the request.</param>
/// <param name="osName">The operating system name of the client.</param>
/// <param name="osVersion">The version of the operating system of the client.</param>
/// <param name="platform">The platform on which the client is running (e.g., Android, iOS).</param>
/// <param name="timeZone">The time zone of the client.</param>
/// <param name="userAgent">The user agent string for the client.</param>
/// <param name="deviceMake">The make of the device on which the client is running.</param>
/// <param name="deviceModel">The model of the device on which the client is running.</param>
/// <param name="utcOffsetMinutes">The UTC offset in minutes for the client's time zone.</param>
internal sealed class Client(
    string hl,
    string gl,
    string platform,
    string clientName,
    string clientVersion,
    string deviceMake,
    string deviceModel,
    string osName,
    string osVersion,
    string userAgent,
    string timeZone,
    int utcOffsetMinutes)
{
    /// <summary>
    /// Creates a new <see cref="Client"/> instance which represents the YouTube Music Web client.
    /// </summary>
    /// <remarks>
    /// Notes:<br/>
    /// - Proof of Origin Token for streaming required.
    /// </remarks>
    /// <returns>A <see cref="Client"/> which represents the YouTube Music Web client.</returns>
    public static Client WebMusic() =>
        new(hl: "en",
            gl: "US",
            platform: "DESKTOP",
            clientName: "WEB_REMIX",
            clientVersion: "1.20250428.03.00",
            deviceMake: "",
            deviceModel: "",
            osName: "Windows",
            osVersion: "10.0",
            userAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/137.0.7151.6 Safari/537.36",
            timeZone: "UTC",
            utcOffsetMinutes: 0)
        {
            BrowserName = "Chrome",
            BrowserVersion = "137.0.7151.6",
            OriginalUrl = "https://music.youtube.com/"
        };

    /// <summary>
    /// Creates a new <see cref="Client"/> instance which represents the YouTube iOS client.
    /// </summary>
    /// <remarks>
    /// Notes:<br/>
    /// - Account cookies not supported.<br/>
    /// - Provides HLS (m3u8) streaming formats .
    /// </remarks>
    /// <returns>A <see cref="Client"/> which represents the YouTube iOS client.</returns>
    public static Client IOS() =>
        new(hl: "en",
            gl: "US",
            platform: "MOBILE",
            clientName: "iOS",
            clientVersion: "20.11.6",
            deviceMake: "Apple",
            deviceModel: "iPhone16,2",
            osName: "iOS",
            osVersion: "18.1.0.22B83",
            userAgent: "com.google.ios.youtube/20.11.6 (iPhone16,2; U; CPU iOS 18_1_0 like Mac OS X; US)",
            timeZone: "UTC",
            utcOffsetMinutes: 0);

    /// <summary>
    /// Creates a new <see cref="Client"/> instance which represents the YouTube TV client.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <returns>A <see cref="Client"/> which represents the YouTube TV client.</returns>
    public static Client Tv() =>
        new(hl: "en",
            gl: "US",
            platform: "DESKTOP",
            clientName: "TVHTML5",
            clientVersion: "7.20250430.13.00",
            deviceMake: "",
            deviceModel: "",
            osName: "",
            osVersion: "",
            userAgent: "Mozilla/5.0 (ChromiumStylePlatform) Cobalt/Version",
            timeZone: "UTC",
            utcOffsetMinutes: 0)
        {
            OriginalUrl = "https://www.youtube.com/tv",
        };


    /// <summary>
    /// The HL parameter used for setting the language of the request.
    /// </summary>
    public string Hl { get; set; } = hl;

    /// <summary>
    /// The GL parameter used for setting the region of the request.
    /// </summary>
    public string Gl { get; set; } = gl;

    /// <summary>
    /// The platform on which the client is running (e.g., Android, iOS).
    /// </summary>
    public string Platform { get; set; } = platform;

    /// <summary>
    /// The name of the client making the request.
    /// </summary>
    public string ClientName { get; set; } = clientName;

    /// <summary>
    /// The version of the client making the request.
    /// </summary>
    public string ClientVersion { get; set; } = clientVersion;

    /// <summary>
    /// The client form factor (e.g., mobile, tablet, desktop).
    /// </summary>
    public string ClientFormFactor { get; set; } = "UNKNOWN_FORM_FACTOR";

    /// <summary>
    /// The make of the device on which the client is running.
    /// </summary>
    public string DeviceMake { get; set; } = deviceMake;

    /// <summary>
    /// The model of the device on which the client is running.
    /// </summary>
    public string DeviceModel { get; set; } = deviceModel;

    /// <summary>
    /// The device experiment ID, if any.
    /// </summary>
    public string? DeviceExperimentId { get; set; }

    /// <summary>
    /// The operating system name of the client.
    /// </summary>
    public string OsName { get; set; } = osName;

    /// <summary>
    /// The version of the operating system of the client.
    /// </summary>
    public string OsVersion { get; set; } = osVersion;

    /// <summary>
    /// The user agent string for the client.
    /// </summary>
    public string UserAgent { get; set; } = userAgent;

    /// <summary>
    /// The browser name used by the client.
    /// </summary>
    public string? BrowserName { get; set; }

    /// <summary>
    /// The version of the browser used by the client.
    /// </summary>
    public string? BrowserVersion { get; set; }

    /// <summary>
    /// The remote host for the client making the request.
    /// </summary>
    public string? RemoteHost { get; set; }

    /// <summary>
    /// The original URL from which the client request was initiated.
    /// </summary>
    public string? OriginalUrl { get; set; }

    /// <summary>
    /// The visitor data associated with the request.
    /// </summary>
    public string? VisitorData { get; set; }

    /// <summary>
    /// The rollout token.
    /// </summary>
    public string? RolloutToken { get; set; }

    /// <summary>
    /// The screen height in points (vertical resolution).
    /// </summary>
    public int? ScreenHeightPoints { get; set; } = 1440;

    /// <summary>
    /// The screen width in points (horizontal resolution).
    /// </summary>
    public int? ScreenWidthPoints { get; set; } = 2560;

    /// <summary>
    /// The screen density as a floating-point value.
    /// </summary>
    public float? ScreenDensityFloat { get; set; } = 1;

    /// <summary>
    /// The screen pixel density (DPI).
    /// </summary>
    public int? ScreenPixelDensity { get; set; } = 1;

    /// <summary>
    /// The user interface theme of the client.
    /// </summary>
    public string? UserInterfaceTheme { get; set; } = "USER_INTERFACE_THEME_DARK";

    /// <summary>
    /// The time zone of the client.
    /// </summary>
    public string TimeZone { get; set; } = timeZone;

    /// <summary>
    /// The UTC offset in minutes for the client's time zone.
    /// </summary>
    public int UtcOffsetMinutes { get; set; } = utcOffsetMinutes;
}
