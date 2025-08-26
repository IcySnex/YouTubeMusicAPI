namespace YouTubeMusicAPI.Common;

/// <summary>
/// Represents a radio channel on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="Radio"/> class.
/// </remarks>
/// <param name="playlistId">The id of the auto-generated playlist that serves as the radio.</param>
/// <param name="songVideoId">The id of the song/video that the radio is based on.</param>
public class Radio(
    string playlistId,
    string? songVideoId = null)
{
    /// <summary>
    /// The id of the auto-generated playlist that serves as the radio.
    /// </summary>
    public string PlaylistId { get; } = playlistId;

    /// <summary>
    /// The id of the song/video that the radio is based on.
    /// </summary>
    public string? SongVideoId { get; } = songVideoId;
}