namespace YouTubeMusicAPI.Models.Library;

/// <summary>
/// Represents a YouTube Music library song
/// </summary>
/// <param name="name">The name of this song</param>
/// <param name="id">The id of this song</param>
/// <param name="artists">The artists of this song</param>
/// <param name="album">The album of this song</param>
/// <param name="duration">The duration of this song</param>
/// <param name="isExplicit">Weither this song is explicit or not</param>
/// <param name="radio">The radio channel of this song</param>
/// <param name="thumbnails">The thumbnails of this song</param>
public class LibrarySong(
    string name,
    string id,
    NamedEntity[] artists,
    NamedEntity album,
    TimeSpan duration,
    bool isExplicit,
    Radio radio,
    Thumbnail[] thumbnails)
{
    /// <summary>
    /// The name of this song
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of this song
    /// </summary>
    public string Id { get; } = id;

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
    /// The radio of this song
    /// </summary>
    public Radio Radio { get; } = radio;

    /// <summary>
    /// The thumbnails of this song
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;
}