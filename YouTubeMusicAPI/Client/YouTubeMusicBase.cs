using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
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
                    clientVersion = "0.1",
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
        logger?.LogInformation($"[RequestHelper-SendRequestAsync] Creating payload.");
        Dictionary<string, object> payload = CreatePayload(hostLanguage, geographicalLocation);
        foreach ((string key, object? value) in payloadItems)
            if (value is not null)
                payload[key] = value;

        // Send request
        string response = await requestHelper.PostBodyAndValidateAsync(url, payload, cancellationToken);

        // Parsing response
        logger?.LogInformation($"[RequestHelper-SendRequestAsync] Parsing response.");
        return JObject.Parse(response);
    }
}