namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Represents a full entity on YouTube Music.
/// </summary>
/// <param name="name">The name of this full entity.</param>
/// <param name="id">The ID of this full entity.</param>
/// <param name="browseId">The browse ID of this full entity.</param>
/// <param name="thumbnails">The thumbnails of this full entity.</param>
public abstract class EntityInfo(
    string name,
    string id,
    string? browseId,
    Thumbnail[] thumbnails) : YouTubeMusicEntity(name, id, browseId)
{
    /// <summary>
    /// The ID of this full entity.
    /// </summary>
    public override string Id { get; } = id;


    /// <summary>
    /// The thumbnails of this full entity.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;
}