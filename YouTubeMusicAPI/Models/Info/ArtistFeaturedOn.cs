namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Contains information about a YouTube Music artist featured on playlist
/// </summary>
/// <param name="name">The name of the artist featured on playlist</param>
/// <param name="id">The id of the artist featured on playlist</param>
/// <param name="creator">The album of the artist featured on playlist</param>
/// <param name="thumbnails">The thumbnails of the artist featured on playlist</param>
public class ArtistFeaturedOn(
    string name,
    string id,
    NamedEntity creator,
    Thumbnail[] thumbnails) : NamedEntity(name, id)
{
    /// <summary>
    /// The id of this entity
    /// </summary>
    public override string Id { get; } = id;


    /// <summary>
    /// The creator of the artist featured on playlist
    /// </summary>
    public NamedEntity Creator { get; } = creator;

    /// <summary>
    /// The thumbnails of the artist featured on playlist
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

}