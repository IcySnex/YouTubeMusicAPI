namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Contains information about a YouTube Music album of an artist
/// </summary>
/// <param name="name">The name of the album of an artist</param>
/// <param name="id">The id of the album of an artist</param>
/// <param name="releaseYear">The release year of the album of an artist</param>
/// <param name="isSingle">Weither the album of an artist is a single or not</param>
/// <param name="isEp">Weither the album of an artist is an EP or not</param>
/// <param name="isExplicit">Weither the album of an artist is a explicit or not</param>
/// <param name="thumbnails">The thumbnails of the album of an artist</param>
public class ArtistAlbumInfo(
    string name,
    string id,
    int releaseYear,
    bool isSingle,
    bool isEp,
    bool isExplicit,
    Thumbnail[] thumbnails)
{
    /// <summary>
    /// The name of the album of an artist
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of the album of an artist
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The release year of the album of an artist
    /// </summary>
    public int ReleaseYear { get; } = releaseYear;

    /// <summary>
    /// Weither the album of an artist is a single or not
    /// </summary>
    public bool IsSingle { get; } = isSingle;

    /// <summary>
    /// Weither the album of an artist is an EP or not
    /// </summary>
    public bool IsEp { get; } = isEp;

    /// <summary>
    /// Weither the album of an artist is explicit or not
    /// </summary>
    public bool IsExplicit { get; } = isExplicit;

    /// <summary>
    /// The thumbnails of the album of an artist
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

}