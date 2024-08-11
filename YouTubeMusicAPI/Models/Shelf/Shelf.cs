using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models.Shelf;

/// <summary>
/// Represents a YouTube Music shelf
/// </summary>
/// <param name="nextContinuationToken">The next continuation token to get further elemnts from this search</param>
/// <param name="items">The shelf items of this shelf</param>
/// <param name="kind">The kind of this shelf</param>
public class Shelf(
    string? nextContinuationToken,
    IShelfItem[] items,
    ShelfKind kind)
{
    /// <summary>
    /// The next continuation token to get further elemnts from this search
    /// </summary>
    public string? NextContinuationToken { get; } = nextContinuationToken;

    /// <summary>
    /// The shelf items of this shelf
    /// </summary>
    public IShelfItem[] Items { get; } = items;


    /// <summary>
    /// The kind of this shelf
    /// </summary>
    public ShelfKind Kind { get; } = kind;
}