namespace YouTubeMusicAPI.Models.Library;

/// <summary>
/// Represents a YouTube Music library entity
/// </summary>
/// <param name="name">The name of this entity</param>
/// <param name="id">The id of this entity</param>
/// <param name="thumbnails">The thumbnails of this entity</param>
public class LibraryEntity(
    string name,
    string id,
    Thumbnail[] thumbnails) : NamedEntity(name, id)
{
    /// <summary>
    /// The id of this entity
    /// </summary>
    public override string Id { get; } = id;


    /// <summary>
    /// The thumbnails of this entity
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;
}