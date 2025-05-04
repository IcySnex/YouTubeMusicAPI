using YouTubeMusicAPI.Exceptions;

namespace YouTubeMusicAPI.Authentication;

/// <summary>
/// Handles the authentication of HTTP requests sent to YouTube Music.
/// </summary>
public interface IAuthenticator
{
    /// <summary>
    /// A unique identifier used to authenticate and link YouTube requests to a user.
    /// </summary>
    public string? VisitorData { get; }

    /// <summary>
    /// A unique rollout token used to validate the YouTube client.
    /// </summary>
    public string? RolloutToken { get; }

    /// <summary>
    /// A unique security token used to verify the authenticity of a client for YouTube requests.
    /// </summary>
    /// <remarks>
    /// May be required when fetching streaming data.
    /// </remarks>
    public string? ProofOfOriginToken { get; }


    /// <summary>
    /// Applies the authentication to the given HTTP request.
    /// </summary>
    /// <param name="request">The HTTP request to which the authentication will be applied.</param>
    /// <exception cref="AuthenticationException">Occurrs when applying the authentication fails.</exception>
    public void Apply(
        HttpRequestMessage request);
}