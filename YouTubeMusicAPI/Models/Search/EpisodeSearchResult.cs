using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a YouTube Music podcast episode search result
/// </summary>
/// <param name="name">The name of this search result</param>
/// <param name="id">The id of this search result</param>
/// <param name="podcast">The podcast of this podcast episode</param>
/// <param name="releasedAt">The date when this podcast episode was released</param>
/// <param name="isLikesAllowed">Weither likes for this podcast episode are allowed or not</param>
/// <param name="thumbnails">The thumbnails of this search result</param>
public class EpisodeSearchResult(
    string name,
    string id,
    NamedEntity podcast,
    DateTime releasedAt,
    bool isLikesAllowed,
    Thumbnail[] thumbnails) : SearchResult(name, id, thumbnails, SearchCategory.Episodes)
{
    /// <summary>
    /// The podcast of this podcast episode
    /// </summary>
    public NamedEntity Podcast { get; } = podcast;

    /// <summary>
    /// The date when this podcast episode was released
    /// </summary>
    public DateTime ReleasedAt { get; } = releasedAt;

    /// <summary>
    /// Weither likes for this podcast episode are allowed or not
    /// </summary>
    public bool IsLikesAllowed { get; } = isLikesAllowed;
}