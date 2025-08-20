namespace YouTubeMusicAPI.Http.Authentication;

/// <summary>
/// Handles the authentication of HTTP requests sent to YouTube Music.
/// </summary>
public interface IAuthenticator
{
    /// <summary>
    /// A unique identifier used to link and fingerprint YouTube requests to a session.
    /// </summary>
    public string? VisitorData { get; }

    /// <summary>
    /// An opaque token that controls feature flag rollouts and UI experiment states.
    /// </summary>
    public string? RolloutToken { get; }

    /// <summary>
    /// A cryptographically signed token issued by YouTube’s BotGuard challenge system to prove the client is legitimate.
    /// </summary>
    /// <remarks>
    /// May be required when fetching streaming data.
    /// </remarks>
    public string? ProofOfOriginToken { get; }


    /// <summary>
    /// Applies the authentication to the given HTTP request.
    /// </summary>
    /// <param name="request">The HTTP request to authenticate.</param>
    /// <param name="clientType">The type of YouTube Music client used for making the requests.</param>
    /// <returns>Weither the authentication was successfully applied to the request.</returns>
    public bool Apply(
        HttpRequestMessage request,
        ClientType clientType);
}