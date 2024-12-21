using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a YouTube Music podcast episode search result
/// </summary>
/// <param name="name">The name of this podcast episode</param>
/// <param name="id">The id of this podcast episode</param>
/// <param name="podcast">The podcast of this podcast episode</param>
/// <param name="releasedAt">The date when this podcast episode was released</param>
/// <param name="isLikesAllowed">Weither likes for this podcast episode are allowed or not</param>
/// <param name="thumbnails">The thumbnails of this podcast episode</param>
public class EpisodeSearchResult(
    string name,
    string id,
    YouTubeMusicItem podcast,
    DateTime releasedAt,
    bool isLikesAllowed,
    Thumbnail[] thumbnails) : IYouTubeMusicItem
{
    /// <summary>
    /// Gets the url of this podcast episode which leads to YouTube music
    /// </summary>
    /// <param name="episode">The podcast episode to get the url for </param>
    /// <returns>An url of this podcast episode which leads to YouTube music</returns>
    public static string GetUrl(
        EpisodeSearchResult episode) =>
        $"https://music.youtube.com/watch?v={episode.Id}";


    /// <summary>
    /// The name of this podcast episode
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of this podcast episode
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The podcast of this podcast episode
    /// </summary>
    public YouTubeMusicItem Podcast { get; } = podcast;

    /// <summary>
    /// The date when this podcast episode was released
    /// </summary>
    public DateTime ReleasedAt { get; } = releasedAt;

    /// <summary>
    /// Weither likes for this podcast episode are allowed or not
    /// </summary>
    public bool IsLikesAllowed { get; } = isLikesAllowed;

    /// <summary>
    /// The thumbnails of this podcast episode
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;


    /// <summary>
    /// The kind of this YouTube Music item
    /// </summary>
    public YouTubeMusicItemKind Kind => YouTubeMusicItemKind.Episodes;
}