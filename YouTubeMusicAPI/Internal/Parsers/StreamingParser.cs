using Newtonsoft.Json.Linq;
using YouTubeMusicAPI.Models.Streaming;

namespace YouTubeMusicAPI.Internal.Parsers;

/// <summary>
/// Contains methods to parse streams from json tokens
/// </summary>
public static class StreamingParser
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
            streamInfo: jsonToken.SelectStreamInfo(),
            expiresIn: TimeSpan.FromSeconds(jsonToken.SelectObject<int>("streamingData.expiresInSeconds")),
            hlsManifestUrl: jsonToken.SelectObject<string>("streamingData.hlsManifestUrl"),
            aspectRatio: jsonToken.SelectObject<double>("streamingData.aspectRatio"));
    }
}