namespace YouTubeMusicAPI.Models.Library;

/// <summary>
/// Represents a YouTube Music library artist
/// </summary>
/// <param name="name">The name of this artist</param>
/// <param name="id">The id of this artist</param>
/// <param name="songCount">The count of songs by the artist which are saved in the library</param>
/// <param name="radio">The radio channel of this artist</param>
/// <param name="thumbnails">The thumbnails of this artist</param>
public class LibraryArtist(
    string name,
    string id,
    int songCount,
    Radio? radio,
    Thumbnail[] thumbnails) : LibraryEntity(name, id, thumbnails)
{
    /// <summary>
    /// The count of songs by the artist which are saved in the library
    /// </summary>
    public int SongCount { get; } = songCount;

    /// <summary>
    /// The radio channel of this artist
    /// </summary>
    public Radio? Radio { get; } = radio;
}