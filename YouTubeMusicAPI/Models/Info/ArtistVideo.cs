namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Contains information about a YouTube Music video of an artist
/// </summary>
/// <param name="name">The name of the video of an artist</param>
/// <param name="id">The id of the video of an artist</param>
/// <param name="artists">The artist of this video of an artist</param>
/// <param name="viewsInfo">The views info of this video of an artist</param>
/// <param name="thumbnails">The thumbnails of the video of an artist</param>
public class ArtistVideo(
    string name,
    string id,
    NamedEntity[] artists,
    string viewsInfo,
    Thumbnail[] thumbnails) : NamedEntity(name, id)
{
    /// <summary>
    /// The id of this entity
    /// </summary>
    public override string Id { get; } = id;


    /// <summary>
    /// The artist of this video of an artist
    /// </summary>
    public NamedEntity[] Artists { get; } = artists;

    /// <summary>
    /// The views info of this video of an artist
    /// </summary>
    public string ViewsInfo { get; } = viewsInfo;

    /// <summary>
    /// The thumbnails of the video of an artist
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;
}