using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using YouTubeMusicAPI.Internal;

namespace YouTubeMusicAPI.Client;

/// <summary>
/// Client for all low level YouTube Music API calls
/// </summary>
internal class YouTubeMusicBase
{
    readonly ILogger? logger;
    readonly RequestHelper requestHelper;

    /// <summary>
    /// Creates a new base client
    /// </summary>
    /// <param name="requestHelper">The HTTP request helper</param>
    public YouTubeMusicBase(
        RequestHelper requestHelper)
    {
        this.requestHelper = requestHelper;
    }

    /// <summary>
    /// Creates a new base client with extendended logging functions
    /// </summary>
    /// <param name="logger">The optional logger used for logging</param>
    /// <param name="requestHelper">The HTTP request helper</param>
    public YouTubeMusicBase(
        ILogger logger,
        RequestHelper requestHelper)
    {
        this.logger = logger;
        this.requestHelper = requestHelper;

        logger.LogInformation($"[BaseClient-.ctor] BaseClient with with extendended logging functions has been initialized.");
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
        string url = Endpoints.MusicWebUrl + apiEndpoint;

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
    /// <param name="payload">The request payload</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <exception cref="NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="TaskCanceledException">Occurs when The task was cancelled</exception>
    /// <returns>The json object containing the response data</returns>
    public async Task<JObject> SendRequestAsync(
        string apiEndpoint,
        Dictionary<string, object> payload,
        CancellationToken cancellationToken = default)
    {
        string url = Endpoints.MusicApiUrl + apiEndpoint;

        // Send request
        string response = await requestHelper.PostAndValidateAsync(url, payload, null, cancellationToken);

        // Parsing response
        logger?.LogInformation($"[YouTubeMusicBase-SendRequestAsync] Parsing response.");
        return JObject.Parse(response);
    }


    /// <summary>
    /// Gets the current visitor data from the YouTube web page
    /// </summary>
    /// <param name="cancellationToken">The token to cancel this action</param>
    /// <returns>The visitor data</returns>
    /// <exception cref="Exception">Occurrs when the visitor data could not be extracted because no Regex matches were found</exception>
    public async Task<string> GetVisitorDataAsync(
        CancellationToken cancellationToken = default)
    {
        string url = Endpoints.YouTubeWebUrl + Endpoints.Embed("um0ETkJABmI");

        // Send request
        string response = await requestHelper.GetAndValidateAsync(url, null, cancellationToken);

        // Extracting visitor data
        logger?.LogInformation($"[YouTubeMusicBase-GetVisitorDataAsync] Extracting visitor data from html.");
        Match match = Regex.Match(response, "\"visitorData\":\"([^\"]+)");
        if (!match.Success)
            throw new Exception("Visitor data could not be extracted from the html: Found no Regex match");

        return match.Groups[1].Value;
    }
}