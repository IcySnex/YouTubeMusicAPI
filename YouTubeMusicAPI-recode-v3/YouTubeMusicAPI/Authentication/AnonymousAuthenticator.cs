using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Authentication;

/// <summary>
/// Represents an anonymous session used to authenticate HTTP requests sent to YouTube Music.
/// </summary>
/// <param name="visitorData">Represents a unique identifier used to authenticate and link YouTube requests to a user. Leave this <see langword="null"/> to use randomly generated visitor data.</param>
/// <param name="prooOfOriginToken">Represents a unique security token used to verify the authenticity of a client for YouTube requests. May be required when fetching streaming data.</param>
/// <param name="apiKey">Represents the API key used to validate the YouTube client.</param>
/// <param name="userAgent">Represents the user agent sent with the request to identify the client making the YouTube request.</param>
public class AnonymousAuthenticator(
    string? visitorData = null,
    string? prooOfOriginToken = null,
    string apiKey = AnonymousAuthenticator.DefaultApiKey,
    string userAgent = AnonymousAuthenticator.DefaultUserAgent) : IAuthenticator
{
    internal const string DefaultApiKey = "AIzaSyA8eiZmM1FaDVjRy-df2KTyQ_vz_yYM39w";

    internal const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36";


    /// <summary>
    /// Represents a unique identifier used to authenticate and link YouTube requests to a user.
    /// </summary>
    /// <remarks>
    /// Leave this <see langword="null"/> to use randomly generated visitor data.
    /// </remarks>
    public string? VisitorData { get; } = visitorData;

    /// <summary>
    /// Represents a unique security token used to verify the authenticity of a client for YouTube requests.
    /// </summary>
    /// <remarks>
    /// May be required when fetching streaming data.
    /// </remarks>
    public string? ProofOfOriginToken { get; } = prooOfOriginToken;

    /// <summary>
    /// Represents the API key used to validate the YouTube client.
    /// </summary>
    public string ApiKey { get; } = apiKey;

    /// <summary>
    /// Represents the user agent sent with the request to identify the client making the YouTube request.
    /// </summary>
    public string UserAgent { get; } = userAgent;


    /// <summary>
    /// Applies the authentication to the given HTTP request.
    /// </summary>
    /// <param name="request">The HTTP request to which the authentication will be applied.</param>
    /// <exception cref="ArgumentNullException">Occurrs when the RequestUri of the request is <see langword="null"/>./></exception>
    public virtual void Apply(
        HttpRequestMessage request)
    {
        Ensure.NotNull(request.RequestUri, nameof(request.RequestUri));

        request.RequestUri = new(Url.SetQueryParameter(request.RequestUri.OriginalString, "key", ApiKey));

        request.Headers.Add("User-Agent", UserAgent);
        request.Headers.Add("Origin", request.RequestUri.Scheme + Uri.SchemeDelimiter + request.RequestUri.Host);
        request.Headers.Add("Cookie", "SOCS=CAI");
    }
}