namespace YouTubeMusicAPI.Models.Library;

/// <summary>
/// Represents a YouTube Music library podcast
/// </summary>
/// <param name="name">The name of this podcast</param>
/// <param name="id">The id of this podcast</param>
/// <param name="host">The host of this podcast</param>
/// <param name="thumbnails">The thumbnails of this album</param>
public class LibraryPodcast(
    string name,
    string id,
    NamedEntity host,
    Thumbnail[] thumbnails) : LibraryEntity(name, id, thumbnails)
{
    /// <summary>
    /// The host of this podcast
    /// </summary>
    public NamedEntity Host { get; } = host;
}