using YouTubeMusicAPI.Models.Shelf;
using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models;

/// <summary>
/// Represents a YouTube Music song
/// </summary>
/// <param name="name">The name of this song</param>
/// <param name="id">The id of this song</param>
/// <param name="artists">The artists of this song</param>
/// <param name="album">The album of this song</param>
/// <param name="duration">The duration of this song</param>
/// <param name="isExplicit">Weither this song is explicit or not</param>
/// <param name="playsInfo">The plays Info of this song</param>
/// <param name="radio">The radio channel of this song</param>
/// <param name="thumbnails">The thumbnails of this song</param>
public class Song(
    string name,
    string id,
    ShelfItem[] artists,
    ShelfItem album,
    TimeSpan duration,
    bool isExplicit,
    string playsInfo,
    Radio radio,
    Thumbnail[] thumbnails) : IShelfItem
{
    /// <summary>
    /// Gets the url of this song which leads to YouTube music
    /// </summary>
    /// <param name="song">The song to get the url for </param>
    /// <returns>An url of this song which leads to YouTube music</returns>
    public static string GetUrl(
        Song song) =>
        $"https://music.youtube.com/watch?v={song.Id}";


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
    public ShelfItem[] Artists { get; } = artists;

    /// <summary>
    /// The album of this song
    /// </summary>
    public ShelfItem Album { get; } = album;

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
    public Radio Radio { get; } = radio;

    /// <summary>
    /// The thumbnails of this song
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;


    /// <summary>
    /// The kind of this shelf item
    /// </summary>
    public ShelfKind Kind => ShelfKind.Songs;
}