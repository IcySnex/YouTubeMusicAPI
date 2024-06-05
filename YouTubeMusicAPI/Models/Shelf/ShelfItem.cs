using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models.Shelf;

/// <summary>
/// Represents an unknown item in a YouTube Music shelf
/// </summary>
/// <param name="name">The name of this item</param>
/// <param name="id">the id of this item</param>
/// <param name="kind">the kind of this shelf item</param>
public class ShelfItem(
    string name,
    string? id,
    ShelfKind kind = ShelfKind.Unknown) : IShelfItem
{
    /// <summary>
    /// The name of this item
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of this item
    /// </summary>
    public string? Id { get; } = id;


    /// <summary>
    /// The kind of this shelf item
    /// </summary>
    public ShelfKind Kind { get; } = kind;
}