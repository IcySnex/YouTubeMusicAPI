namespace YouTubeMusicAPI.Http.Authentication;

/// <summary>
/// Represents an anonymous session used to authenticate HTTP requests sent to YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="AnonymousAuthenticator"/> class.
/// </remarks>
/// <param name="visitorData">A unique identifier used to link and fingerprint YouTube requests to a session.</param>
/// <param name="rolloutToken">An opaque token that controls feature flag rollouts and UI experiment states.</param>
/// <param name="prooOfOriginToken">A cryptographically signed token issued by YouTube’s BotGuard challenge system to prove the client is legitimate. May be required when fetching streaming data.</param>
public class AnonymousAuthenticator(
    string? visitorData = null,
    string? rolloutToken = null,
    string? prooOfOriginToken = null) : IAuthenticator
{
    /// <summary>
    /// A unique identifier used to link and fingerprint YouTube requests to a session.
    /// </summary>
    public string? VisitorData { get; } = visitorData;

    /// <summary>
    /// An opaque token that controls feature flag rollouts and UI experiment states.
    /// </summary>
    public string? RolloutToken { get; } = rolloutToken;

    /// <summary>
    /// A cryptographically signed token issued by YouTube’s BotGuard challenge system to prove the client is legitimate.
    /// </summary>
    /// <remarks>
    /// May be required when fetching streaming data.
    /// </remarks>
    public string? ProofOfOriginToken { get; } = prooOfOriginToken;


    /// <summary>
    /// Applies the authentication to the given HTTP request.
    /// </summary>
    /// <param name="request">The HTTP request to authenticate.</param>
    /// <param name="clientType">The type of YouTube Music client used for making the requests.</param>
    /// <returns>Weither the authentication was successfully applied to the request.</returns>
    public virtual bool Apply(
        HttpRequestMessage request,
        ClientType clientType)
    {
        request.Headers.Add("Cookie", "SOCS=CAI");
        return true;
    }
}