using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace YouTubeMusicAPI.Internal;

/// <summary>
/// Helper which handles all HTTP requests
/// </summary>
internal class RequestHelper
{
    readonly ILogger? logger;
    readonly HttpClient httpClient;

    /// <summary>
    /// Creates a new request helper
    /// </summary>
    public RequestHelper()
    {
        httpClient = new();
    }

    /// <summary>
    /// Creates a new request helper with extendended logging functions
    /// </summary>
    /// <param name="logger">The optional logger used for logging</param>
    public RequestHelper(
        ILogger logger)
    {
        httpClient = new();

        this.logger = logger;

        logger.LogInformation($"[RequestHelper-.ctor] RequestHelper with extendended logging functions has been initialized.");
    }


    /// <summary>
    /// Sends a new POST request to the given uri with the serializes body
    /// </summary>
    /// <param name="url">The uri the request should be made to</param>
    /// <param name="body">The body which should be serialized</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <exception cref="System.NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="System.InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="System.Net.Http.HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="System.Threading.Tasks.TaskCanceledException">Occurs when The task was cancelled</exception>
    /// <returns>The HTTP response message</returns>
    public Task<HttpResponseMessage> PostBodyAsync(
        string url,
        object body,
        CancellationToken cancellationToken = default)
    {
        // Serialize body
        string serializedContent = JsonConvert.SerializeObject(body, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

        // Create HTTP request
        var request = new HttpRequestMessage()
        {
            Method = HttpMethod.Post,
            RequestUri = new(url),
            Content = new StringContent(serializedContent, Encoding.UTF8, "application/json"),
        };

        // Send HTTP request
        logger?.LogInformation($"[RequestHelper-PostBodyAsync] Sending HTTP reuqest. POST: {url}.");
        return httpClient.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Sends a new POST request to the given uri with the serializes body and validates it
    /// </summary>
    /// <param name="uri">The uri the request should be made to</param>
    /// <param name="body">The body which should be serialized</param>
    /// <param name="cancellationToken">The cancellation token to cancel the action</param>
    /// <exception cref="System.NotSupportedException">May occurs when the json serialization fails</exception>
    /// <exception cref="System.InvalidOperationException">May occurs when sending the web request fails</exception>
    /// <exception cref="System.Net.Http.HttpRequestException">May occurs when sending the web request fails</exception>
    /// <exception cref="System.Threading.Tasks.TaskCanceledException">Occurs when The task was cancelled</exception>
    /// <returns>The validated HTTP response data</returns>
    public async Task<string> PostBodyAndValidateAsync(
        string uri,
        object body,
        CancellationToken cancellationToken = default)
    {
        // Send HTTP request
        HttpResponseMessage httpResponse = await PostBodyAsync(uri, body, cancellationToken).ConfigureAwait(false);

        // Parse HTTP response data
        logger?.LogInformation($"[RequestHelper-PostBodyAndValidateAsync] Parsing HTTP response.");
        string httpResponseData = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

        // Check for exception
        if (!httpResponse.IsSuccessStatusCode)
        {
            logger?.LogError($"[RequestHelper-PostBodyAndValidateAsync] HTTP request failed. Statuscode: {httpResponse.StatusCode}.");
            throw new HttpRequestException($"HTTP request failed. StatusCode: {httpResponse.StatusCode}.", new(httpResponseData));
        }

        // Return response data
        return httpResponseData;
    }
}