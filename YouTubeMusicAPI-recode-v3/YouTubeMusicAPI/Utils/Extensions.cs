using YouTubeMusicAPI.Exceptions;
using YouTubeMusicAPI.Http;

namespace YouTubeMusicAPI.Utils;

internal static class Extensions
{
    public static Client? Create(
        this ClientType type) =>
        type switch
        {
            ClientType.None => null,
            ClientType.WebMusic => Client.WebMusic(),
            ClientType.IOS => Client.IOS(),
            ClientType.Tv => Client.Tv(),
            _ => throw new UnknownClientException(type)
        };
}