using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a YouTube Music search result shelf
/// </summary>
/// <param name="nextContinuationToken">The next continuation token to get further elemnts from this search</param>
/// <param name="items">The YouTube Music items of this shelf</param>
/// <param name="kind">The kind of this shelf</param>
public class Shelf(
    string? nextContinuationToken,
    IYouTubeMusicItem[] items,
    YouTubeMusicItemKind kind)
{
    /// <summary>
    /// The next continuation token to get further elemnts from this search
    /// </summary>
    public string? NextContinuationToken { get; } = nextContinuationToken;

    /// <summary>
    /// The shelf items of this shelf
    /// </summary>
    public IYouTubeMusicItem[] Items { get; } = items;


    /// <summary>
    /// The kind of this shelf
    /// </summary>
    public YouTubeMusicItemKind Kind { get; } = kind;
}