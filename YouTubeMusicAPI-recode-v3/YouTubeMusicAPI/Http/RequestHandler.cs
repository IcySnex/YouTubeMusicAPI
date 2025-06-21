using Microsoft.Extensions.Logging;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using YouTubeMusicAPI.Authentication;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Http;

/// <summary>
/// Handles all outgoing HTTP requests.
/// </summary>
internal sealed class RequestHandler(
    string geographicalLocation,
    IAuthenticator authenticator,
    HttpClient httpClient,
    ILogger? logger = null)
{
    readonly string geographicalLocation = geographicalLocation;
    readonly IAuthenticator authenticator = authenticator;
    readonly HttpClient httpClient = httpClient;
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
        KeyValuePair<string, object>[]? payload,
        ClientType clientType,
        CancellationToken cancellationToken = default)
    {
        // Prepare
        HttpRequestMessage request = new(method, url);
        Dictionary<string, object> body = payload?.ToDictionary() ?? [];

        if (clientType.Create() is Client client)
        {
            client.Gl = geographicalLocation;
            client.VisitorData = authenticator.VisitorData;
            client.RolloutToken = authenticator.RolloutToken;
            body["client"] = client;

            if (authenticator.ProofOfOriginToken is string poToken)
                body["serviceIntegrityDimensions"] = new { poToken };

            request.Headers.Add("User-Agent", client.UserAgent);
        }

        if (body.Count != 0)
        {
            string json = JsonSerializer.Serialize(body, jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        authenticator.Apply(request);

        // Send
        logger?.LogInformation("[RequestHandler-SendAsync] Sending HTTP reuqest: {method}-{url}.", method, url);
        HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

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
        KeyValuePair<string, object>[]? payload = null,
        ClientType clientType = ClientType.None,
        CancellationToken cancellationToken = default) =>
        SendAsync(url, HttpMethod.Get, payload, clientType, cancellationToken);

    /// <exception cref="AuthenticationException">Occurrs when applying the authentication fails.</exception>
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>"
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public Task<string> PostAsync(
        string url,
        KeyValuePair<string, object>[]? payload = null,
        ClientType clientType = ClientType.None,
        CancellationToken cancellationToken = default) =>
        SendAsync(url, HttpMethod.Post, payload, clientType, cancellationToken);
}