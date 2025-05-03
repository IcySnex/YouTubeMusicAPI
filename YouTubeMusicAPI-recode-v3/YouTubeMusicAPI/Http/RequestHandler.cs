using Microsoft.Extensions.Logging;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using YouTubeMusicAPI.Authentication;

namespace YouTubeMusicAPI.Http;

/// <summary>
/// Handles all outgoing HTTP requests.
/// </summary>
internal sealed class RequestHandler(
    HttpClient client,
    IAuthenticator authenticator,
    ILogger? logger = null)
{
    readonly HttpClient client = client;
    readonly IAuthenticator authenticator = authenticator;
    readonly ILogger? logger = logger;

    readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };


    /// <exception cref="AuthenticationException">Occurrs when applying the authentication fails.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>"
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    async Task<string> SendAsync(
        string url,
        HttpMethod method,
        object? body = null,
        CancellationToken cancellationToken = default)
    {
        // Prepare
        HttpRequestMessage request = new(method, url);
        if (body is not null)
        {
            string json = JsonSerializer.Serialize(body, jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        authenticator.Apply(request);

        // Send
        logger?.LogInformation("[RequestHandler-SendAsync] Sending HTTP reuqest: {method}-{url}.", method, url);
        HttpResponseMessage response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

        string content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        // Validate
        if (!response.IsSuccessStatusCode)
        {
            logger?.LogError("[RequestHandler-SendAsync] HTTP request failed. Statuscode: {statusCode}.", response.StatusCode);
            throw new HttpRequestException($"HTTP request failed. StatusCode: {response.StatusCode}.", new(content));
        }

        return content;
    }


    /// <exception cref="AuthenticationException">Occurrs when applying the authentication fails.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>"
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public Task<string> GetAsync(
        string url,
        object? body = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(url, HttpMethod.Get, body, cancellationToken);

    /// <exception cref="AuthenticationException">Occurrs when applying the authentication fails.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>"
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public Task<string> PostAsync(
        string url,
        object? body = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(url, HttpMethod.Post, body, cancellationToken);
}