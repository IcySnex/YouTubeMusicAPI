using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models.Shelf;

/// <summary>
/// Represents an unknown item in a YouTube Music shelf
/// </summary>
public interface IShelfItem
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
    public ShelfKind Kind { get; }
}