namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Contains information about a YouTube Music song or video
/// </summary>
/// <param name="name">The name of the song or video</param>
/// <param name="id">The id of the song or video</param>
/// <param name="browseId">The browse id of the song or video</param>
/// <param name="description">The description of the song or video</param>
/// <param name="artists">The artist of the song or video</param>
/// <param name="album">The album of the song or video</param>
/// <param name="duration">The duration of the song or video</param>
/// <param name="radio">The radio channel of the song or video (null when live content is true)</param>
/// <param name="playabilityStatus">The playability status of the song or video</param>
/// <param name="isRatingsAllowed">Whether ratings are allowed or not</param>
/// <param name="isPrivate">Whether the song or video is private or not</param>
/// <param name="isUnlisted">Whether the song or video is unlisted or not</param>
/// <param name="isLiveContent">Whether the song or video is live content or not</param>
/// <param name="isExplicit">Whether the song or video is explicit or not</param>
/// <param name="isFamiliyFriendly">Whether the song or video is family friendly or not</param>
/// <param name="viewsCount">The views count of the song or video</param>
/// <param name="publishedAt">The date when the song or video was published</param>
/// <param name="uploadedAt">The date when the song or video was uploaded</param>
/// <param name="thumbnails">The thumbnails of the song or video</param>
/// <param name="tags">The tags of the song or video</param>
public class SongVideoInfo(
    string name,
    string id,
    string browseId,
    string description,
    YouTubeMusicItem[] artists,
    YouTubeMusicItem? album,
    TimeSpan duration,
    Radio? radio,
    PlayabilityStatus playabilityStatus,
    bool isRatingsAllowed,
    bool isPrivate,
    bool isUnlisted,
    bool isLiveContent,
    bool isFamiliyFriendly,
    bool isExplicit,
    long viewsCount,
    DateTime publishedAt,
    DateTime uploadedAt,
    Thumbnail[] thumbnails,
    string[] tags)
{
    /// <summary>
    /// The name of the song or video
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of the song or video
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The browse id of the song or video
    /// </summary>
    public string BrowseId { get; } = browseId;

    /// <summary>
    /// The description of the song or video
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// The artist of the song or video
    /// </summary>
    public YouTubeMusicItem[] Artists { get; } = artists;

    /// <summary>
    /// The album of the song or video
    /// </summary>
    public YouTubeMusicItem? Album { get; } = album;

    /// <summary>
    /// The duration of the song or video
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// The radio channel of the song or video (null when live content is true)
    /// </summary>
    public Radio? Radio { get; } = radio;

    /// <summary>
    /// The playability status of the song or video
    /// </summary>
    public PlayabilityStatus PlayabilityStatus { get; } = playabilityStatus;

    /// <summary>
    /// Weither ratings for the song or video are allowed or not
    /// </summary>
    public bool IsRatingsAllowed { get; } = isRatingsAllowed;

    /// <summary>
    /// Weither the song or video is private or not
    /// </summary>
    public bool IsPrivate { get; } = isPrivate;

    /// <summary>
    /// Weither the song or video is unlisted or not
    /// </summary>
    public bool IsUnlisted { get; } = isUnlisted;

    /// <summary>
    /// Weither the song or video is live content or not
    /// </summary>
    public bool IsLiveContent { get; } = isLiveContent;

    /// <summary>
    /// Weither the song or video is family friendly or not
    /// </summary>
    public bool IsFamiliyFriendly { get; } = isFamiliyFriendly;

    /// <summary>
    /// Weither the song or video is explicit or not
    /// </summary>
    public bool IsExplicit { get; } = isExplicit;

    /// <summary>
    /// The views count of the song or video
    /// </summary>
    public long ViewsCount { get; } = viewsCount;

    /// <summary>
    /// The date when the song or video was published
    /// </summary>
    public DateTime PublishedAt { get; } = publishedAt;

    /// <summary>
    /// The date when the song or video was uploaded
    /// </summary>
    public DateTime UploadedAt { get; } = uploadedAt;

    /// <summary>
    /// The thumbnails of the song or video
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The tags of the song or video
    /// </summary>
    public string[] Tags { get; } = tags;
}
