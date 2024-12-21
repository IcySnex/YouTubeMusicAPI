namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Contains information about a YouTube Music artist featured on playlist
/// </summary>
/// <param name="name">The name of the artist featured on playlist</param>
/// <param name="id">The id of the artist featured on playlist</param>
/// <param name="creator">The album of the artist featured on playlist</param>
/// <param name="thumbnails">The thumbnails of the artist featured on playlist</param>
public class ArtistFeaturedOnInfo(
    string name,
    string id,
    YouTubeMusicItem creator,
    Thumbnail[] thumbnails)
{
    /// <summary>
    /// The name of the artist featured on playlist
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of the artist featured on playlist
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The creator of the artist featured on playlist
    /// </summary>
    public YouTubeMusicItem Creator { get; } = creator;

    /// <summary>
    /// The thumbnails of the artist featured on playlist
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

}