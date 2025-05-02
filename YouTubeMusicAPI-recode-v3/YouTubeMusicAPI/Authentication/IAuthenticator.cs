namespace YouTubeMusicAPI.Authentication;

/// <summary>
/// Handles the authentication of HTTP requests sent to YouTube Music.
/// </summary>
public interface IAuthenticator
{
    /// <summary>
    /// Represents a unique identifier used to authenticate and link YouTube requests to a user.
    /// </summary>
    public string? VisitorData { get; }

    /// <summary>
    /// Represents a unique security token used to verify the authenticity of a client for YouTube requests.
    /// </summary>
    /// <remarks>
    /// May be required when fetching streaming data.
    /// </remarks>
    public string? ProoOfOriginToken { get; }

    /// <summary>
    /// Represents the API key used to validate the YouTube client.
    /// </summary>
    public string ApiKey { get; }

    /// <summary>
    /// Represents the user agent sent with the request to identify the client making the YouTube request.
    /// </summary>
    public string UserAgent { get; }


    /// <summary>
    /// Applies the authentication to the given HTTP request.
    /// </summary>
    /// <param name="request">The HTTP request to which the authentication will be applied.</param>
    public void Apply(
        HttpRequestMessage request);
}