using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using YouTubeMusicAPI.Exceptions;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Authentication;

/// <summary>
/// Represents an authenticated user session via cookies used to authenticate HTTP requests sent to YouTube Music.
/// </summary>
public class CookieAuthenticator : AnonymousAuthenticator, IAuthenticator
{
    /// <summary>
    /// The container used to store cookies for authentication.
    /// </summary>
    public CookieContainer Container { get; } = new();

    /// <summary>
    /// Creates a new instance of the <see cref="CookieAuthenticator"/> class.
    /// </summary>
    /// <param name="visitorData">A unique identifier used to authenticate and link YouTube requests to a user. Leave this <see langword="null"/> to use randomly generated visitor data.</param>
    /// <param name="rolloutToken">A unique rollout token used to validate the YouTube client.</param>
    /// <param name="prooOfOriginToken">A unique security token used to verify the authenticity of a client for YouTube requests. May be required when fetching streaming data.</param>
    /// <param name="apiKey">The API key used to validate the YouTube client.</param>
    /// <param name="userAgent">The user agent sent with the request to identify the client making the YouTube request.</param>
    public CookieAuthenticator(
        IEnumerable<Cookie> cookies,
        string? visitorData = null,
        string? rolloutToken = null,
        string? prooOfOriginToken = null,
        string apiKey = DefaultApiKey,
        string userAgent = DefaultUserAgent) : base(visitorData, rolloutToken, prooOfOriginToken, apiKey, userAgent)
    {
        foreach (Cookie cookie in cookies)
            Container.Add(cookie);

        Container.Add(new Cookie("SOCS", "CAI") { Domain = ".youtube.com" });
    }


    /// <exception cref="AuthenticationException">Occurrs when provided cookies cookies do not contain a session id.</exception>
    string GenerateAuthHeaderValue(
        Uri uri)
    {
        IReadOnlyCollection<Cookie> cookies = Container.GetCookies(uri);

        string? sessionId = (cookies.FirstOrDefault(c => string.Equals(c.Name, "__Secure-3PAPISID", StringComparison.Ordinal)) ?? cookies.FirstOrDefault(c => string.Equals(c.Name, "SAPISID", StringComparison.Ordinal)))?.Value;
        if (string.IsNullOrEmpty(sessionId))
            throw new AuthenticationException("The provided cookies do not contain a session id (__Secure-3PAPISID or SAPISID).");

        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string domain = uri.Scheme + Uri.SchemeDelimiter + uri.Host;

        string token = $"{timestamp} {sessionId} {domain}";
        byte[] tokenHashData = SHA1.HashData(Encoding.UTF8.GetBytes(token));

        StringBuilder tokenHashBuffer = new(2 * tokenHashData.Length);
        foreach (byte value in tokenHashData)
            tokenHashBuffer.Append(value.ToString("X2", CultureInfo.InvariantCulture));

        return $"SAPISIDHASH {timestamp}_{tokenHashBuffer}";
    }


    /// <summary>
    /// Applies the authentication to the given HTTP request.
    /// </summary>
    /// <param name="request">The HTTP request to which the authentication will be applied.</param>
    /// <exception cref="AuthenticationException">Occurrs when provided cookies cookies do not contain a session id.</exception>
    /// <exception cref="ArgumentNullException">Occurrs when the RequestUri of the request is <see langword="null"/>./></exception>
    /// <exception cref="ArgumentException">Occurrs when the provided cookies do not contain a session id.</exception>
    public override void Apply(
        HttpRequestMessage request)
    {
        base.Apply(request);

        Ensure.NotNull(request.RequestUri, nameof(request.RequestUri));

        request.Headers.Remove("Cookie");
        request.Headers.Add("Cookie", Container.GetCookieHeader(request.RequestUri));
        request.Headers.Add("Authorization", GenerateAuthHeaderValue(request.RequestUri));
    }
}