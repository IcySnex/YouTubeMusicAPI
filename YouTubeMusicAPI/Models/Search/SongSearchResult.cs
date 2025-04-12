using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a YouTube Music song search result
/// </summary>
/// <param name="name">The name of this search result</param>
/// <param name="id">The id of this search result</param>
/// <param name="artists">The artists of this song</param>
/// <param name="album">The album of this song</param>
/// <param name="duration">The duration of this song</param>
/// <param name="isExplicit">Weither this song is explicit or not</param>
/// <param name="playsInfo">The plays Info of this song</param>
/// <param name="radio">The radio channel of this song</param>
/// <param name="thumbnails">The thumbnails of this search result</param>
public class SongSearchResult(
    string name,
    string id,
    NamedEntity[] artists,
    NamedEntity album,
    TimeSpan duration,
    bool isExplicit,
    string playsInfo,
    Radio? radio,
    Thumbnail[] thumbnails) : SearchResult(name, id, thumbnails, SearchCategory.Songs)
{
    /// <summary>
    /// The artist of this song
    /// </summary>
    public NamedEntity[] Artists { get; } = artists;

    /// <summary>
    /// The album of this song
    /// </summary>
    public NamedEntity Album { get; } = album;

    /// <summary>
    /// The duration of this song
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// Weither this song is explicit or not
    /// </summary>
    public bool IsExplicit { get; } = isExplicit;

    /// <summary>
    /// The plays info of this song
    /// </summary>
    public string PlaysInfo { get; } = playsInfo;

    /// <summary>
    /// The radio of this song
    /// </summary>
    public Radio? Radio { get; } = radio;
}