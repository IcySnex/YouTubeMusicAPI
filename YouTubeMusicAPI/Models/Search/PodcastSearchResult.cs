using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a YouTube Music podcast search result
/// </summary>
/// <param name="name">The name of this podcast</param>
/// <param name="id">The id of this podcast</param>
/// <param name="host">The host of this podcast</param>
/// <param name="thumbnails">The thumbnails of this podcast</param>
public class PodcastSearchResult(
    string name,
    string id,
    YouTubeMusicItem host,
    Thumbnail[] thumbnails) : IYouTubeMusicItem
{
    /// <summary>
    /// Gets the url of this podcast which leads to YouTube music
    /// </summary>
    /// <param name="podcast">The podcast to get the url for </param>
    /// <returns>An url of this podcast which leads to YouTube music</returns>
    public static string GetUrl(
        PodcastSearchResult podcast) =>
        $"https://music.youtube.com/playlist?list={podcast.Id}";


    /// <summary>
    /// The name of this podcast
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of this podcast
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The host of this podcast
    /// </summary>
    public YouTubeMusicItem Host { get; } = host;

    /// <summary>
    /// The thumbnails of this podcast
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;


    /// <summary>
    /// The kind of this YouTube Music item
    /// </summary>
    public YouTubeMusicItemKind Kind => YouTubeMusicItemKind.Podcasts;
}