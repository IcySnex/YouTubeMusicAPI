namespace YouTubeMusicAPI.Services;

/// <summary>
/// Represents an identifiable named entity on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="YouTubeMusicEntity"/> class.
/// </remarks>
/// <param name="name">The name of this entity.</param>
/// <param name="id">The ID of this entity.</param>
/// <param name="browseId">The browse ID of this entity.</param>
public class YouTubeMusicEntity(
    string name,
    string? id,
    string? browseId)
{
    /// <summary>
    /// The name of this entity.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The ID of this entity.
    /// </summary>
    public virtual string? Id { get; } = id;

    /// <summary>
    /// The browse ID of this entity.
    /// </summary>
    public virtual string? BrowseId { get; } = browseId;
}