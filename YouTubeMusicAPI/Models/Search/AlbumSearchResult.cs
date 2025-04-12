using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a YouTube Music album search result
/// </summary>
/// <param name="name">The name of this search result</param>
/// <param name="id">The id of this search result</param>
/// <param name="artists">The artists of this album</param>
/// <param name="releaseYear">The release year of this album</param>
/// <param name="isSingle">Weither this album is a single or not</param>
/// <param name="isEp">Weither this album is an EP or not</param>
/// <param name="radio">The radio channel of this album</param>
/// <param name="thumbnails">The thumbnails of this search result</param>
public class AlbumSearchResult(
    string name,
    string id,
    NamedEntity[] artists,
    int releaseYear,
    bool isSingle,
    bool isEp,
    Radio radio,
    Thumbnail[] thumbnails) : SearchResult(name, id, thumbnails, SearchCategory.Albums)
{
    /// <summary>
    /// The artists of this album
    /// </summary>
    public NamedEntity[] Artists { get; } = artists;

    /// <summary>
    /// The release year of this album
    /// </summary>
    public int ReleaseYear { get; } = releaseYear;

    /// <summary>
    /// Weither this album is a single or not
    /// </summary>
    public bool IsSingle { get; } = isSingle;
    
    /// <summary>
    /// Weither this album is an EP or not
    /// </summary>
    public bool IsEp { get; } = isEp;

    /// <summary>
    /// The radio channel of this album
    /// </summary>
    public Radio Radio { get; } = radio;
}