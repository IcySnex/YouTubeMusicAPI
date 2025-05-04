using System.Text.RegularExpressions;
using YouTubeMusicAPI.Http;

namespace YouTubeMusicAPI.Authentication;

/// <summary>
/// Contains static methods used to generate valid authentication data for YouTube Music.
/// </summary>
public static partial class AuthenticationDataGenerator
{
    /// <exception cref="InvalidDataException">Occurs when the visitor data could not be extracted from the HTML content.</exception>"
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>"
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    static async Task<string> ExtractContextPropertyAsync(
        string property,
        CancellationToken cancellationToken = default)
    {
        using HttpClient client = new();

        string url = Endpoints.YouTubeUrl + Endpoints.Embed("um0ETkJABmI");
        HttpRequestMessage request = new(HttpMethod.Get, url);

        HttpResponseMessage respone = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        respone.EnsureSuccessStatusCode();

        string htmlContent = await respone.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        Match match = Regex.Match(htmlContent, $"\"{property}\":\"([^\"]+)");
        if (!match.Success)
            throw new InvalidDataException("Visitor data could not be extracted from the HTML content: Found no Regex match.");

        return match.Groups[1].Value;
    }


    /// <summary>
    /// Generates valid visitor data used to authenticate YouTube requests.
    /// </summary>
    /// <param name="cancellationToken">The token to cancel this task.</param>
    /// <returns>A string representing the visitor data.</returns>
    /// <exception cref="InvalidDataException">Occurs when the visitor data could not be extracted from the HTML content.</exception>"
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>"
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public static Task<string> VisitorDataAsync(
        CancellationToken cancellationToken = default) =>
        ExtractContextPropertyAsync("visitorData", cancellationToken);

    /// <summary>
    /// Generates a valid rollout token used to validate the YouTube client.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>A string representing the rollout token.</returns>
    /// <exception cref="InvalidDataException">Occurs when the visitor data could not be extracted from the HTML content.</exception>"
    /// <exception cref="HttpRequestException">Occurs when the HTTP request fails.</exception>"
    /// <exception cref="OperationCanceledException">Occurs when this task was cancelled.</exception>
    public static Task<string> RolloutTokenAsync(
        CancellationToken cancellationToken = default) =>
        ExtractContextPropertyAsync("rolloutToken", cancellationToken);

    /// <summary>
    /// Generates a valid proof of origin token used to verify the authenticity of the client for YouTube requests.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>A string representing the proof of origin token.</returns>
    [Obsolete("Currently not supported. Please check https://github.com/yt-dlp/yt-dlp/wiki/PO-Token-Guide to manually fetch a proof of origin token.", true)]
    public static Task<string> ProofOfOriginTokenAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(string.Empty);
    }
}