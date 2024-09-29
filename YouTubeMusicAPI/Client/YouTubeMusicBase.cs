using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Web;
using YouTubeMusicAPI.Internal;

namespace YouTubeMusicAPI.Client;

/// <summary>
/// Client for all low level YouTube Music API calls
/// </summary>
public class YouTubeMusicBase
{
    /// <summary>
    /// Creates a new YouTube Music request payload
    /// </summary>
    /// <param name="hostLanguage">The language for the payload</param>
    /// <param name="geographicalLocation">The region for the payload</param>
    /// <returns>The validated HTTP response data</returns>
    public static Dictionary<string, object> CreatePayload(
        string hostLanguage = "en",
        string geographicalLocation = "US")
    {
        Dictionary<string, object> payload = new()
        {
            ["context"] = new
            {
                client = new
                {
                    clientName = "WEB_REMIX",
                    clientVersion = "1.20240918.01.00",
                    hl = hostLanguage,
                    gl = geographicalLocation,
                }
            }
        };
        return payload;
    }


    readonly ILogger? logger;
    readonly RequestHelper requestHelper = new();

    /// <summary>
    /// Creates a new base client
    /// </summary>
    public YouTubeMusicBase()
    {
    }

    /// <summary>
    /// Creates a new base client with extendended logging functions
    /// </summary>
    /// <param name="logger">The optional logger used for logging</param>
    public YouTubeMusicBase(
        ILogger logger)
    {
        this.logger = logger;

        logger.LogInformation($"[BaseClient-.ctor] BaseClient with with extendended logging functions has been initialized.");
    }


    /// <summary>
    /// Consents cookies for YouTube Music
    /// </summary>
    /// <param name="hostLanguage">The language for the parameters</param>
    /// <param name="geographicalLocation">The region for the parameters</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    async Task ConsentCookiesAsync(
        string hostLanguage = "en",
        string geographicalLocation = "US",
        CancellationToken cancellationToken = default)
    {
        string url = Endpoints.Cookies + Endpoints.Save;

        // Create parameters
        logger?.LogInformation($"[YouTubeMusicBase-GenerateCookiesAsync] Creating parameters.");
        NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);
        parameters.Set("hl", hostLanguage);
        parameters.Set("gl", geographicalLocation);
        parameters.Set("pc", "ytm");
        parameters.Set("continue", "https://music.youtube.com/?cbrd=1");
        parameters.Set("x", "6");
        parameters.Set("bl", "boq_identityfrontenduiserver_20240617.06_p0");
        parameters.Set("set_eom", "true");

        // Send request
        await requestHelper.PostAsync(url, null, parameters.ToString(), cancellationToken);
    }


    /// <summary>
    /// Gets the web content of the given YouTube Music API endpoint with parameter queries
    /// </summary>
    /// <param name="apiEndpoint">The specific API endpoint for the request</param>
    /// <param name="parameterItems">The items to add to the query parameters</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    /// <returns>The json object containing the response data</returns>
    public async Task<string> GetWebContentAsync(
        string apiEndpoint,
        (string key, string? value)[] parameterItems,
        CancellationToken cancellationToken = default)
    {
        string url = Endpoints.WebUrl + apiEndpoint;

        // Cookies
        if (requestHelper.Cookies.GetCookies(new(Endpoints.Cookies)).Count < 1)
            await ConsentCookiesAsync();

        // Create parameters
        logger?.LogInformation($"[YouTubeMusicBase-GetWebContentAsync] Creating parameters.");
        NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);
        foreach ((string key, string? value) in parameterItems)
            if (value is not null)
                parameters[key] = value;

        // Send request
        string response = await requestHelper.GetAndValidateAsync(url, parameters.ToString(), cancellationToken);

        return response;
    }

    /// <summary>
    /// Sends a new request to the YouTube Music API with a payload
    /// </summary>
    /// <param name="apiEndpoint">The specific API endpoint for the request</param>
    /// <param name="payloadItems">The items to add to the request payload</param>
    /// <param name="hostLanguage">The language for the payload</param>
    /// <param name="geographicalLocation">The region for the payload</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    /// <returns>The json object containing the response data</returns>
    public async Task<JObject> SendRequestAsync(
        string apiEndpoint,
        (string key, object? value)[] payloadItems,
        string hostLanguage = "en",
        string geographicalLocation = "US",
        CancellationToken cancellationToken = default)
    {
        string url = Endpoints.BaseUrl + apiEndpoint;

        // Create payload
        logger?.LogInformation($"[YouTubeMusicBase-SendRequestAsync] Creating payload.");
        Dictionary<string, object> payload = CreatePayload(hostLanguage, geographicalLocation);
        foreach ((string key, object? value) in payloadItems)
            if (value is not null)
                payload[key] = value;

        // Send request
        string response = await requestHelper.PostAndValidateAsync(url, payload, null, cancellationToken);

        // Parsing response
        logger?.LogInformation($"[YouTubeMusicBase-SendRequestAsync] Parsing response.");
        return JObject.Parse(response);
    }
}