using YouTubeMusicAPI.Models.Shelf;

namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Contains information about a YouTube Music song or video
/// </summary>
/// <param name="name">The name of the song or video</param>
/// <param name="id">The id of the song or video</param>
/// <param name="description">The description of the song or video</param>
/// <param name="artists">The artist of the song or video</param>
/// <param name="duration">The duration of the song or video</param>
/// <param name="isRatingsAllowed">Whether ratings are allowed or not</param>
/// <param name="isPrivate">Whether the song or video is private or not</param>
/// <param name="isUnlisted">Whether the song or video is unlisted or not</param>
/// <param name="isUnpluggedCorpus">Whether the song or video is unplugged corpus or not</param>
/// <param name="isLiveContent">Whether the song or video is live content or not</param>
/// <param name="isFamiliyFriendly">Whether the song or video is family friendly or not</param>
/// <param name="viewsCount">The views count of the song or video</param>
/// <param name="publishedAt">The date when the song or video was published</param>
/// <param name="uploadedAt">The date when the song or video was uploaded</param>
/// <param name="thumbnails">The thumbnails of the song or video</param>
/// <param name="tags">The tags of the song or video</param>
public class SongVideoInfo(
    string name,
    string id,
    string description,
    ShelfItem[] artists,
    TimeSpan duration,
    bool isRatingsAllowed,
    bool isPrivate,
    bool isUnlisted,
    bool isUnpluggedCorpus,
    bool isLiveContent,
    bool isFamiliyFriendly,
    int viewsCount,
    DateTime publishedAt,
    DateTime uploadedAt,
    Thumbnail[] thumbnails,
    string[] tags)
{
    /// <summary>
    /// The name of the song
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of the song
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The description of the song
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// The artist of the song
    /// </summary>
    public ShelfItem[] Artists { get; } = artists;

    /// <summary>
    /// The duration of the song
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// Weither ratings are allowed or not
    /// </summary>
    public bool IsRatingsAllowed { get; } = isRatingsAllowed;

    /// <summary>
    /// Weither the song is private or not
    /// </summary>
    public bool IsPrivate { get; } = isPrivate;

    /// <summary>
    /// Weither the song is unlisted or not
    /// </summary>
    public bool IsUnlisted { get; } = isUnlisted;

    /// <summary>
    /// Weither the song is unplugged corups or not
    /// </summary>
    public bool IsUnpluggedCorpus { get; } = isUnpluggedCorpus;

    /// <summary>
    /// Weither the song is live content or not
    /// </summary>
    public bool IsLiveContent { get; } = isLiveContent;

    /// <summary>
    /// Weither the song is family friendly or not
    /// </summary>
    public bool IsFamiliyFriendly { get; } = isFamiliyFriendly;

    /// <summary>
    /// The views count of the song
    /// </summary>
    public int ViewsCount { get; } = viewsCount;

    /// <summary>
    /// The date when the song was published
    /// </summary>
    public DateTime PublishedAt { get; } = publishedAt;

    /// <summary>
    /// The date when the song was uploaded
    /// </summary>
    public DateTime UploadedAt { get; } = uploadedAt;

    /// <summary>
    /// The thumbnails of the song
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The tags of the song
    /// </summary>
    public string[] Tags { get; } = tags;
}