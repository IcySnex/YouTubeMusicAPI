using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Authentication;

/// <summary>
/// Represents an anonymous session used to authenticate HTTP requests sent to YouTube Music.
/// </summary>
/// <param name="visitorData">A unique identifier used to authenticate and link YouTube requests to a user. Leave this <see langword="null"/> to use randomly generated visitor data.</param>
/// <param name="rolloutToken">A unique rollout token used to validate the YouTube client.</param>
/// <param name="prooOfOriginToken">A unique security token used to verify the authenticity of a client for YouTube requests. May be required when fetching streaming data.</param>
public class AnonymousAuthenticator(
    string? visitorData = null,
    string? rolloutToken = null,
    string? prooOfOriginToken = null) : IAuthenticator
{
    /// <summary>
    /// A unique identifier used to authenticate and link YouTube requests to a user.
    /// </summary>
    /// <remarks>
    /// Leave this <see langword="null"/> to use randomly generated visitor data.
    /// </remarks>
    public string? VisitorData { get; } = visitorData;

    /// <summary>
    /// A unique rollout token used to validate the YouTube client.
    /// </summary>
    public string? RolloutToken { get; } = rolloutToken;

    /// <summary>
    /// A unique security token used to verify the authenticity of a client for YouTube requests.
    /// </summary>
    /// <remarks>
    /// May be required when fetching streaming data.
    /// </remarks>
    public string? ProofOfOriginToken { get; } = prooOfOriginToken;


    /// <summary>
    /// Applies the authentication to the given HTTP request.
    /// </summary>
    /// <param name="request">The HTTP request to which the authentication will be applied.</param>
    /// <exception cref="ArgumentNullException">Occurrs when the RequestUri of the request is <see langword="null"/>./></exception>
    public virtual void Apply(
        HttpRequestMessage request)
    {
        Ensure.NotNull(request.RequestUri, nameof(request.RequestUri));

        request.Headers.Add("Origin", request.RequestUri.Scheme + Uri.SchemeDelimiter + request.RequestUri.Host);
        request.Headers.Add("Cookie", "SOCS=CAI");
    }
}