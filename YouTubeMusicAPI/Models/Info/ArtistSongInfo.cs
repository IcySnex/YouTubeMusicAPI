namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Contains information about a YouTube Music song of an artist
/// </summary>
/// <param name="name">The name of the song of an artist</param>
/// <param name="id">The id of the song of an artist</param>
/// <param name="artists">The artist of this song of an artist</param>
/// <param name="album">The album of this song of an artist</param>
/// <param name="playsinfo">The plays info of this song of an artist</param>
/// <param name="thumbnails">The thumbnails of the song of an artist</param>
public class ArtistSongInfo(
    string name,
    string id,
    YouTubeMusicItem[] artists,
    YouTubeMusicItem album,
    string playsinfo,
    Thumbnail[] thumbnails)
{
    /// <summary>
    /// The name of the song of an artist
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of the song of an artist
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The artist of this song of an artist
    /// </summary>
    public YouTubeMusicItem[] Artists { get; } = artists;

    /// <summary>
    /// The album of this song of an artist
    /// </summary>
    public YouTubeMusicItem Album { get; } = album;

    /// <summary>
    /// The plays info of this song of an artist
    /// </summary>
    public string Playsinfo { get; } = playsinfo;

    /// <summary>
    /// The thumbnails of the song of an artist
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

}