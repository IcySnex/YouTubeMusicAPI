using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models;

/// <summary>
/// Represents an unknown item in YouTube Music
/// </summary>
/// <param name="name">The name of this item</param>
/// <param name="id">the id of this item</param>
/// <param name="kind">the kind of this shelf item</param>
public class YouTubeMusicItem(
    string name,
    string? id,
    YouTubeMusicItemKind kind = YouTubeMusicItemKind.Unknown) : IYouTubeMusicItem
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
    /// The kind of this YouTube Music item
    /// </summary>
    public YouTubeMusicItemKind Kind { get; } = kind;
}