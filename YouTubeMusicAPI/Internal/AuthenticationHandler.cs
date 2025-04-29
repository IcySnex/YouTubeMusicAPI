using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace YouTubeMusicAPI.Internal;

/// <summary>
/// Handles authenticaion via cookies for YouTube Music requests
/// </summary>
public class AuthenticationHandler
{
    readonly CookieContainer cookieContainer = new();

    /// <summary>
    /// Creates a new AuthenticationHandler
    /// </summary>
    /// <param name="cookies">The initial cookies</param>
    public AuthenticationHandler(
        IEnumerable<Cookie>? cookies = null)
    {
        if (cookies is not null)
            foreach (var cookie in cookies)
                cookieContainer.Add(cookie);

        cookieContainer.Add(new Cookie("SOCS", "CAI") { Domain = ".youtube.com" });
    }


    /// <summary>
    /// Tries to generate the auth header value for a given uri
    /// </summary>
    /// <param name="uri">The uri to generate the auth header value for</param>
    /// <returns>The genereated auth header value</returns>
    string? TryGenerateAuthHeaderValue(
        Uri uri)
    {
        IEnumerable<Cookie> cookies = cookieContainer.GetCookies(uri).OfType<Cookie>();

        string? sessionId = cookies.FirstOrDefault(c => string.Equals(c.Name, "__Secure-3PAPISID", StringComparison.Ordinal))?.Value ?? cookies.FirstOrDefault(c => string.Equals(c.Name, "SAPISID", StringComparison.Ordinal))?.Value;
        if (string.IsNullOrWhiteSpace(sessionId))
            return null;

        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string domain = uri.Scheme + Uri.SchemeDelimiter + uri.Host;

        string token = $"{timestamp} {sessionId} {domain}";

        using SHA1 sha1 = SHA1.Create();
        byte[] tokenHashData = sha1.ComputeHash(Encoding.UTF8.GetBytes(token));

        StringBuilder tokenHashBuffer = new(2 * tokenHashData.Length);
        foreach (var b in tokenHashData)
            tokenHashBuffer.Append(b.ToString("X2", CultureInfo.InvariantCulture));

        return $"SAPISIDHASH {timestamp}_{tokenHashBuffer}";
    }


    /// <summary>
    /// Prepares an HTTP request for the inner handler to send to the server
    /// </summary>
    /// <param name="request">The HTTP request message to prepare</param>
    public void Prepare(
        HttpRequestMessage request)
    {
        string requestUrl = request.RequestUri.OriginalString;
        request.RequestUri = new(requestUrl + (requestUrl.Contains('?') ? '&' : '?') + "key=AIzaSyA8eiZmM1FaDVjRy-df2KTyQ_vz_yYM39w");

        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
        request.Headers.Add("Origin", request.RequestUri.Scheme + Uri.SchemeDelimiter + request.RequestUri.Host);

        if (TryGenerateAuthHeaderValue(request.RequestUri) is string authHeaderValue)
            request.Headers.Add("Authorization", authHeaderValue);

        if (cookieContainer.Count > 0)
            request.Headers.Add("Cookie", cookieContainer.GetCookieHeader(request.RequestUri));
    }
}