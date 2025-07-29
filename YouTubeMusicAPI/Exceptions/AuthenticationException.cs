namespace YouTubeMusicAPI.Exceptions;

/// <summary>
/// Represents errors that occur during authentication processes within the YouTube Music API.
/// </summary>
public class AuthenticationException : Exception
{
    /// <summary>
    /// Creates a new instance of the <see cref="AuthenticationException"/> class.
    /// </summary>
    public AuthenticationException() { }

    /// <summary>
    /// Creates a new instance of the <see cref="AuthenticationException"/> with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public AuthenticationException(string message)
        : base(message) { }

    /// <summary>
    /// Creates a new instance of <see cref="AuthenticationException"/> with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public AuthenticationException(string message, Exception innerException)
        : base(message, innerException) { }
}