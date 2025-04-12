using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a YouTube Music podcast search result
/// </summary>
/// <param name="name">The name of this search result</param>
/// <param name="id">The id of this search result</param>
/// <param name="host">The host of this podcast</param>
/// <param name="thumbnails">The thumbnails of this search result</param>
public class PodcastSearchResult(
    string name,
    string id,
    NamedEntity host,
    Thumbnail[] thumbnails) : SearchResult(name, id, thumbnails, SearchCategory.Podcasts)
{
    /// <summary>
    /// The host of this podcast
    /// </summary>
    public NamedEntity Host { get; } = host;
}