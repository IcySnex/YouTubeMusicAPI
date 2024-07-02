using YouTubeMusicAPI.Models.Shelf;
using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Contains information about a YouTube Music song
/// </summary>
/// <param name="name">The name of the song</param>
/// <param name="id">The id of the song</param>
/// <param name="description">The description of the song</param>
/// <param name="artists">The artist of the song</param>
/// <param name="duration">The duration of the song</param>
/// <param name="isOwnerViewing">Whether the owner is viewing or not</param>
/// <param name="isCrawlable">Whether the song is crawlable or not</param>
/// <param name="isRatingsAllowed">Whether ratings are allowed or not</param>
/// <param name="isPrivate">Whether the song is private or not</param>
/// <param name="isUnlisted">Whether the song is unlisted or not</param>
/// <param name="isUnpluggedCorpus">Whether the song is unplugged corpus or not</param>
/// <param name="isLiveContent">Whether the song is live content or not</param>
/// <param name="isFamiliyFriendly">Whether the song is family friendly or not</param>
/// <param name="viewsCount">The views count of the song</param>
/// <param name="publishedAt">The date when the song was published</param>
/// <param name="uploadedAt">The date when the song was uploaded</param>
/// <param name="thumbnails">The thumbnails of the song</param>
/// <param name="tags">The tags of the song</param>
/// <param name="availableCountries">The available countries for the song</param>
public class SongInfo(
    string name,
    string id,
    string description,
    ShelfItem[] artists,
    TimeSpan duration,
    bool isOwnerViewing,
    bool isCrawlable,
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
    string[] tags,
    string[] availableCountries)
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
    /// Weither the owner is viewing or not
    /// </summary>
    public bool IsOwnerViewing { get; } = isOwnerViewing;

    /// <summary>
    /// Weither the song is crawlable or not
    /// </summary>
    public bool IsCrawlable { get; } = isCrawlable;

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

    /// <summary>
    /// The available countries for the song
    /// </summary>
    public string[] AvailableCountries { get; } = availableCountries;
}