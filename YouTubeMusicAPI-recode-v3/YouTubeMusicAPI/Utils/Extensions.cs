using YouTubeMusicAPI.Exceptions;
using YouTubeMusicAPI.Http;

namespace YouTubeMusicAPI.Utils;

/// <summary>
/// Contains extension methods for various use cases.
/// </summary>
internal static class Extensions
{
    /// <summary>
    /// Creates a new instance of a <see cref="Client"/> based on the specified <see cref="ClientType"/>.
    /// </summary>
    /// <param name="type">The type of client to create.</param>
    /// <returns>A new <see cref="Client"/> instance.</returns>
    /// <exception cref="UnknownClientException">Occurs when an invalid client type is passed.</exception>
    public static Client? Create(
        this ClientType type) =>
        type switch
        {
            ClientType.None => null,
            ClientType.WebMusic => Client.WebMusic.Clone(),
            ClientType.IOS => Client.IOS.Clone(),
            ClientType.Tv => Client.Tv.Clone(),
            _ => throw new UnknownClientException(type)
        };
}