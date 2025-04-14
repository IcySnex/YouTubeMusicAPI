using Newtonsoft.Json.Linq;
using YouTubeMusicAPI.Models.Streaming;

namespace YouTubeMusicAPI.Internal.Parsers;

/// <summary>
/// Contains methods to parse streams from json tokens
/// </summary>
internal static class StreamingParser
{
    /// <summary>
    /// Parses streaming data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>The streaming data</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static StreamingData GetData(
        JObject jsonToken)
    {
        if (jsonToken.SelectObject<string>("playabilityStatus.status") != "OK")
            throw new InvalidOperationException($"This media stream is not playable: {jsonToken.SelectObjectOptional<string>("playabilityStatus.reason") ?? "Unknown reason"}");
        return new(
            streamInfo: jsonToken.SelectStreamInfo(content => content.SelectObject<string>("url")),
            isLiveContent: jsonToken.SelectObject<bool>("videoDetails.isLiveContent"),
            expiresIn: TimeSpan.FromSeconds(jsonToken.SelectObject<int>("streamingData.expiresInSeconds")),
            hlsManifestUrl: jsonToken.SelectObjectOptional<string>("streamingData.hlsManifestUrl"));
    }

    /// <summary>
    /// Parses streaming data from the json token and decipher urls
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <param name="player">The player for deciphering</param>
    /// <returns>The streaming data</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static StreamingData GetData(
        JObject jsonToken,
        Player player)
    {
        if (jsonToken.SelectObject<string>("playabilityStatus.status") != "OK")
            throw new InvalidOperationException($"This media stream is not playable: {jsonToken.SelectObjectOptional<string>("playabilityStatus.reason") ?? "Unknown reason"}");

        return new(
            streamInfo: jsonToken.SelectStreamInfo(content =>
            {
                string? url = content.SelectObjectOptional<string>("url");
                string? signatureCipher = content.SelectObjectOptional<string>("signatureCipher");
                string? cipher = content.SelectObjectOptional<string>("cipher");

                return player.Decipher(url, signatureCipher, cipher);
            }),
            isLiveContent: jsonToken.SelectObject<bool>("videoDetails.isLiveContent"),
            expiresIn: TimeSpan.FromSeconds(jsonToken.SelectObject<int>("streamingData.expiresInSeconds")),
            hlsManifestUrl: jsonToken.SelectObjectOptional<string>("streamingData.hlsManifestUrl"));
    }
}