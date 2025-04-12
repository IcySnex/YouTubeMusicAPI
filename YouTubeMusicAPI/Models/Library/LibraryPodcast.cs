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
    Thumbnail[] thumbnails)
{
    /// <summary>
    /// The name of this podcast
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of this podcast
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The host of this podcast
    /// </summary>
    public NamedEntity Host { get; } = host;

    /// <summary>
    /// The thumbnails of this podcast
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;
}