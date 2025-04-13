namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Contains information about a YouTube Music album
/// </summary>
/// <param name="name">The name of the album</param>
/// <param name="id">The id of the album</param>
/// <param name="description">The description of the album</param>
/// <param name="artists">The artist of the album</param>
/// <param name="duration">The total duration of all tracks in the album</param>
/// <param name="songCount">The count of songs in the album</param>
/// <param name="releaseYear">The release year of the album</param>
/// <param name="isSingle">Weither the album is a single or not</param>
/// <param name="isEp">Weither the album is an EP or not</param>
/// <param name="thumbnails">The thumbnails of the album</param>
/// <param name="songs">The info of all songs in album</param>
public class AlbumInfo(
    string name,
    string id,
    string? description,
    NamedEntity[] artists,
    TimeSpan duration,
    int songCount,
    int releaseYear,
    bool isSingle,
    bool isEp,
    Thumbnail[] thumbnails,
    AlbumSong[] songs)
{
    /// <summary>
    /// The name of the album
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of the album
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The description of the album
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// The artist of the album
    /// </summary>
    public NamedEntity[] Artists { get; } = artists;

    /// <summary>
    /// The total duration of all tracks in the album
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// The count of songs in the album
    /// </summary>
    public int SongCount { get; } = songCount;

    /// <summary>
    /// The release year of the album
    /// </summary>
    public int ReleaseYear { get; } = releaseYear;

    /// <summary>
    /// Weither the album is a single or not
    /// </summary>
    public bool IsSingle { get; } = isSingle;

    /// <summary>
    /// Weither the album is an EP or not
    /// </summary>
    public bool IsEp { get; } = isEp;

    /// <summary>
    /// The thumbnails of the album
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The info of all songs in album
    /// </summary>
    public AlbumSong[] Songs { get; } = songs;
}