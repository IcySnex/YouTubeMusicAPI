using YouTubeMusicAPI.Models.Shelf;
using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models;

/// <summary>
/// Represents a YouTube Music album
/// </summary>
/// <param name="name">The name of this album</param>
/// <param name="id">The id of this album</param>
/// <param name="artists">The artists of this album</param>
/// <param name="releaseYear">The release year of this album</param>
/// <param name="isSingle">Weither this album is a single or not</param>
/// <param name="radio">The radio channel of this album</param>
/// <param name="thumbnails">The thumbnails of this album</param>
public class Album(
    string name,
    string id,
    ShelfItem[] artists,
    int releaseYear,
    bool isSingle,
    Radio radio,
    Thumbnail[] thumbnails) : IShelfItem
{
    /// <summary>
    /// Gets the url of this album which leads to YouTube music
    /// </summary>
    /// <param name="album">The album to get the url for </param>
    /// <returns>An url of this album which leads to YouTube music</returns>
    public static string GetUrl(
        Album album) =>
        $"https://music.youtube.com/playlist?list={album.Id}";


    /// <summary>
    /// The name of this album
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of this album
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The artists of this album
    /// </summary>
    public ShelfItem[] Artists { get; } = artists;

    /// <summary>
    /// The release year of this album
    /// </summary>
    public int ReleaseYear { get; } = releaseYear;

    /// <summary>
    /// Weither this album is a single or not
    /// </summary>
    public bool IsSingle { get; } = isSingle;

    /// <summary>
    /// The radio channel of this album
    /// </summary>
    public Radio Radio { get; } = radio;

    /// <summary>
    /// The thumbnails of this album
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;


    /// <summary>
    /// The kind of this shelf item
    /// </summary>
    public ShelfKind Kind => ShelfKind.Albums;
}