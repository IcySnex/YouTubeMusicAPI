using YouTubeMusicAPI.Http;

namespace YouTubeMusicAPI.Exceptions;

/// <summary>
/// Represents errors that occur when trying to create an unknown YouTube client.
/// </summary>
internal class UnknownClientException : Exception
{
    /// <summary>
    /// Creates a new instance of the <see cref="UnknownClientException"/> class.
    /// </summary>
    /// <param name="clientType">The type of the client which was tried to create that caused the exception.</param>
    public UnknownClientException(
        ClientType clientType)
    {
        ClientType = clientType;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UnknownClientException"/> with a specified error message.
    /// </summary>
    /// <param name="clientType">The type of the client which was tried to create that caused the exception.</param>
    /// <param name="message">The message that describes the error.</param>
    public UnknownClientException(
        ClientType clientType,
        string message) : base(message)
    {
        ClientType = clientType;
    }

    /// <summary>
    /// Creates a new instance of <see cref="UnknownClientException"/> with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="clientType">The type of the client which was tried to create that caused the exception.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public UnknownClientException(
        ClientType clientType,
        string message,
        Exception innerException) : base(message, innerException)
    {
        ClientType = clientType;
    }


    /// <summary>
    /// The type of the client which was tried to create that caused the exception.
    /// </summary>
    public ClientType ClientType { get; }
}