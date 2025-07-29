namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a YouTube Music video search result
/// </summary>
/// <param name="name">The name of this search result</param>
/// <param name="id">The id of this search result</param>
/// <param name="artists">The artists of this video</param>
/// <param name="duration">The duration of this video</param>
/// <param name="viewsInfo">The views info of this video</param>
/// <param name="radio">The radio of this video</param>
/// <param name="thumbnails">The thumbnails of this search result</param>
public class VideoSearchResult(
    string name,
    string id,
    NamedEntity[] artists,
    TimeSpan duration,
    string viewsInfo,
    Radio? radio,
    Thumbnail[] thumbnails) : SearchResult(name, id, thumbnails, SearchCategory.Videos)
{
    /// <summary>
    /// The artist of this video
    /// </summary>
    public NamedEntity[] Artists { get; } = artists;

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
    public Radio? Radio { get; } = radio;
}