namespace YouTubeMusicAPI.Models;

/// <summary>
/// Represents a YouTube Music thumbnail image
/// </summary>
/// <param name="url">The url of this thumbnail image</param>
/// <param name="width">The pixels width of this thumbnail image</param>
/// <param name="height">The pixels height of this thumbnail image</param>
public class Thumbnail(
    string url,
    int width,
    int height)
{
    /// <summary>
    /// The url of this thumbnail image
    /// </summary>
    public string Url { get; } = url;

    /// <summary>
    /// The pixels width of this thumbnail image
    /// </summary>
    public int Width { get; } = width;

    /// <summary>
    /// The pixels height of this thumbnail image
    /// </summary>
    public int Height { get; } = height;
}