using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models;

/// <summary>
/// Represents an item in YouTube Music
/// </summary>
public interface IYouTubeMusicItem
{
    /// <summary>
    /// The name of this item
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The id of this item
    /// </summary>
    public string? Id { get; }


    /// <summary>
    /// The kind of this shelf item
    /// </summary>
    public YouTubeMusicItemKind Kind { get; }
}