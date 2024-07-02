namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Contains information about a YouTube Music song in an album
/// </summary>
/// <param name="name">The name of the song</param>
/// <param name="id">The id of the song</param>
/// <param name="isExplicit">Weither the song is explicit or not</param>
/// <param name="playsInfo">The plays info of the song</param>
/// <param name="duration">The duration of the song</param>
/// <param name="songNumber">The song number of the song</param>
public class AlbumSongInfo(
    string name,
    string? id,
    bool isExplicit,
    string? playsInfo,
    TimeSpan duration,
    int? songNumber)
{
    /// <summary>
    /// The name of the song
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of the song
    /// </summary>
    public string? Id { get; } = id;

    /// <summary>
    /// Weither the song is explicit or not
    /// </summary>
    public bool IsExplicit { get; } = isExplicit;

    /// <summary>
    /// The plays info of the song
    /// </summary>
    public string? PlaysInfo { get; } = playsInfo;

    /// <summary>
    /// The duration of the song
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// The song number of the song
    /// </summary>
    public int? SongNumber { get; } = songNumber;

}