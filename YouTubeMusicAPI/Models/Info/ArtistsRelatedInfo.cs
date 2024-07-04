namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Contains information about a YouTube Music related artist
/// </summary>
/// <param name="name">The name of the related artist</param>
/// <param name="id">The id of the related artist</param>
/// <param name="subscribersInfo">The subscribers info of the related artist</param>
/// <param name="thumbnails">The thumbnails of the related artist</param>
public class ArtistsRelatedInfo(
    string name,
    string id,
    string subscribersInfo,
    Thumbnail[] thumbnails)
{
    /// <summary>
    /// The name of the related artist
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of the related artist
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The subscribers info of the related artist
    /// </summary>
    public string SubscribersInfo { get; } = subscribersInfo;

    /// <summary>
    /// The thumbnails of the related artist
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

}