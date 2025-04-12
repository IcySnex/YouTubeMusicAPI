using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a search result on YouTube Music
/// </summary>
/// <param name="name">The name of this search result</param>
/// <param name="id">The id of this search result</param>
/// <param name="thumbnails">The thumbnails of this search result</param>
/// <param name="category">The category of this search result</param>
public abstract class SearchResult(
    string name,
    string id,
    Thumbnail[] thumbnails,
    SearchCategory category) : NamedEntity(name, id)
{
    /// <summary>
    /// The id of this search result
    /// </summary>
    public override string Id { get; } = id;


    /// <summary>
    /// The thumbnails of this search result
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The category of this search result
    /// </summary>
    public SearchCategory Category { get; } = category;
}