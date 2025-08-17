using YouTubeMusicAPI.Json;
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
    /// Parses a <see cref="JElement"/> into a <see cref="Thumbnail"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> to parse.</param>
    /// <returns>A <see cref="Thumbnail"/> representing the <see cref="JElement"/>.</returns>
    internal static Thumbnail Parse(
        JElement element)
    {
        string url = element
            .Get("url")
            .AsString()
            .OrThrow();

        int width = element
            .Get("width")
            .AsInt32()
            .OrThrow();

        int height = element
            .Get("height")
            .AsInt32()
            .OrThrow();

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