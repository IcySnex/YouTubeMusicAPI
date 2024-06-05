using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models.Shelf;

/// <summary>
/// Represents a YouTube Music shelf
/// </summary>
/// <param name="query">The request query associated with this shelf</param>
/// <param name="params">The request params associated with this shelf</param>
/// <param name="items">The shelf items of this shelf</param>
/// <param name="kind">The kind of this shelf</param>
public class Shelf(
    string query,
    string @params,
    IShelfItem[] items,
    ShelfKind kind)
{
    /// <summary>
    /// The request query associated with this shelf
    /// </summary>
    public string Query { get; } = query;

    /// <summary>
    /// The request params associated with this shelf
    /// </summary>
    public string Params { get; } = @params;

    /// <summary>
    /// The shelf items of this shelf
    /// </summary>
    public IShelfItem[] Items { get; } = items;


    /// <summary>
    /// The kind of this shelf
    /// </summary>
    public ShelfKind Kind { get; } = kind;
}