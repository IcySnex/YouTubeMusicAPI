using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Http.Authentication;

/// <summary>
/// Represents an user session via cookies used to authenticate HTTP requests sent to YouTube Music.
/// </summary>
public class CookieAuthenticator : AnonymousAuthenticator, IAuthenticator
{
    /// <summary>
    /// Creates a new instance of the <see cref="CookieAuthenticator"/> class.
    /// </summary>
    /// <param name="cookies">A collection of cookies containing the required authentication data.</param>
    /// <param name="visitorData">A unique identifier used to link and fingerprint YouTube requests to a session.</param>
    /// <param name="rolloutToken">An opaque token that controls feature flag rollouts and UI experiment states.</param>
    /// <param name="prooOfOriginToken">A cryptographically signed token issued by YouTube’s BotGuard challenge system to prove the client is legitimate. May be required when fetching streaming data.</param>
    /// <exception cref="ArgumentException">Occurs when the provided cookies cookies do not contain a session id (__Secure-3PAPISID or SAPISID).</exception>
    public CookieAuthenticator(
        IEnumerable<Cookie> cookies,
        string? visitorData = null,
        string? rolloutToken = null,
        string? prooOfOriginToken = null) : base(visitorData, rolloutToken, prooOfOriginToken)
    {
        // Add cookies
        foreach (Cookie cookie in cookies)
            Container.Add(cookie);

        Container.Add(new Cookie("SOCS", "CAI") { Domain = ".youtube.com" });

        // Get session id
        Cookie? sessionCookie = cookies.FirstOrDefault(c => string.Equals(c.Name, "__Secure-3PAPISID", StringComparison.Ordinal));
        sessionCookie ??= cookies.FirstOrDefault(c => string.Equals(c.Name, "SAPISID", StringComparison.Ordinal));

        if (sessionCookie is null)
            throw new ArgumentException("The provided cookies do not contain a session id (__Secure-3PAPISID or SAPISID).", nameof(cookies));

        SessionId = sessionCookie.Value;
    }


    /// <summary>
    /// The container used to store cookies for authentication.
    /// </summary>
    public CookieContainer Container { get; } = new();

    /// <summary>
    /// The session id extracted from the provided cookies (__Secure-3PAPISID or SAPISID).
    /// </summary>
    public string SessionId { get; }


    /// <summary>
    /// Generates the value for the Authorization header based on the provided cookies and the request URI.
    /// </summary>
    /// <param name="uri">The request uri.</param>
    /// <returns>The Authorization header.</returns>
    string GenerateAuthHeaderValue(
        Uri uri)
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string domain = uri.Scheme + Uri.SchemeDelimiter + uri.Host;

        string token = $"{timestamp} {SessionId} {domain}";
        byte[] tokenHashData = SHA1.HashData(Encoding.UTF8.GetBytes(token));

        StringBuilder tokenHashBuffer = new(2 * tokenHashData.Length);
        foreach (byte value in tokenHashData)
            tokenHashBuffer.Append(value.ToString("X2", CultureInfo.InvariantCulture));

        return $"SAPISIDHASH {timestamp}_{tokenHashBuffer}";
    }


    /// <summary>
    /// Applies the authentication to the given HTTP request.
    /// </summary>
    /// <remarks>
    /// Only applies to web clients (<see cref="ClientType.WebMusic"/>).
    /// </remarks>
    /// <param name="request">The HTTP request to authenticate.</param>
    /// <param name="clientType">The type of YouTube Music client used for making the requests.</param>
    /// <returns>Weither the authentication was successfully applied to the request.</returns>
    public override bool Apply(
        HttpRequestMessage request,
        ClientType clientType)
    {
        if (request.RequestUri is null ||
            clientType != ClientType.WebMusic)
            return false;

        request.Headers.Add("Cookie", Container.GetCookieHeader(request.RequestUri));
        request.Headers.Add("Authorization", GenerateAuthHeaderValue(request.RequestUri));
        return true;
    }
}