using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a YouTube Music video search result
/// </summary>
/// <param name="name">The name of this video</param>
/// <param name="id">The id of this video</param>
/// <param name="artist">The artist of this video</param>
/// <param name="duration">The duration of this video</param>
/// <param name="viewsInfo">The views info of this video</param>
/// <param name="radio">The radio of this video</param>
/// <param name="thumbnails">The thumbnails of this video</param>
public class VideoSearchResult(
    string name,
    string id,
    YouTubeMusicItem artist,
    TimeSpan duration,
    string viewsInfo,
    Radio radio,
    Thumbnail[] thumbnails) : IYouTubeMusicItem
{
    /// <summary>
    /// Gets the url of this video which leads to YouTube music
    /// </summary>
    /// <param name="video">The video to get the url for </param>
    /// <returns>An url of this video which leads to YouTube music</returns>
    public static string GetUrl(
        VideoSearchResult video) =>
        $"https://music.youtube.com/watch?v={video.Id}";


    /// <summary>
    /// The name of this video
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of this video
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The artist of this video
    /// </summary>
    public YouTubeMusicItem Artist { get; } = artist;

    /// <summary>
    /// The duration of this video
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// The views info of this video
    /// </summary>
    public string ViewsInfo { get; } = viewsInfo;

    /// <summary>
    /// The radio of this video
    /// </summary>
    public Radio Radio { get; } = radio;

    /// <summary>
    /// The thumbnails of this video
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;


    /// <summary>
    /// The kind of this YouTube Music item
    /// </summary>
    public YouTubeMusicItemKind Kind => YouTubeMusicItemKind.Videos;
}