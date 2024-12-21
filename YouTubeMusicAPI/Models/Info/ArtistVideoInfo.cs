namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Contains information about a YouTube Music video of an artist
/// </summary>
/// <param name="name">The name of the video of an artist</param>
/// <param name="id">The id of the video of an artist</param>
/// <param name="artists">The artist of this video of an artist</param>
/// <param name="viewsInfo">The views info of this video of an artist</param>
/// <param name="thumbnails">The thumbnails of the video of an artist</param>
public class ArtistVideoInfo(
    string name,
    string id,
    YouTubeMusicItem[] artists,
    string viewsInfo,
    Thumbnail[] thumbnails)
{
    /// <summary>
    /// The name of the video of an artist
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of the video of an artist
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The artist of this video of an artist
    /// </summary>
    public YouTubeMusicItem[] Artists { get; } = artists;

    /// <summary>
    /// The views info of this video of an artist
    /// </summary>
    public string ViewsInfo { get; } = viewsInfo;

    /// <summary>
    /// The thumbnails of the video of an artist
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

}