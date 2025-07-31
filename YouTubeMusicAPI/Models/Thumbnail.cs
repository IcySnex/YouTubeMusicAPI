using System.Text.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models;

/// <summary>
/// Represents a thumbnail on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="Thumbnail"/> class.
/// </remarks>
/// <param name="url">The URL of this thumbnail.</param>
/// <param name="width">The pixels width of this thumbnail.</param>
/// <param name="height">The pixels height of this thumbnail.</param>
public class Thumbnail(
    string url,
    int width,
    int height)
{
    /// <summary>
    /// Parses a <see cref="JsonElement"/> into a <see cref="Thumbnail"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> to parse.</param>
    internal static Thumbnail Parse(
        JsonElement element)
    {
        string url = element
            .GetProperty("url")
            .GetString()
            .OrThrow();

        int width = element
            .GetProperty("width")
            .GetInt32();

        int height = element
            .GetProperty("height")
            .GetInt32();

        return new(url, width, height);
    }


    /// <summary>
    /// The URL of this thumbnail.
    /// </summary>
    public string Url { get; } = url;

    /// <summary>
    /// The pixels width of this thumbnail.
    /// </summary>
    public int Width { get; } = width;

    /// <summary>
    /// The pixels height of this thumbnail.
    /// </summary>
    public int Height { get; } = height;
}