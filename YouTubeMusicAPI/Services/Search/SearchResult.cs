using YouTubeMusicAPI.Common;

namespace YouTubeMusicAPI.Services.Search;

/// <summary>
/// Represents a search result on YouTube Music.
/// </summary>
/// <param name="name">The name of this search result.</param>
/// <param name="id">The ID of this search result.</param>
/// <param name="browseId">The browse ID of this search result.</param>
/// <param name="thumbnails">The thumbnails of this search result.</param>
public abstract class SearchResult(
    string name,
    string id,
    string? browseId,
    Thumbnail[] thumbnails) : YouTubeMusicEntity(name, id, browseId)
{
    /// <summary>
    /// The ID of this search result.
    /// </summary>
    public override string Id { get; } = id;


    /// <summary>
    /// The thumbnails of this search result.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;
}