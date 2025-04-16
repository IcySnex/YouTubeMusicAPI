namespace YouTubeMusicAPI.Models;

/// <summary>
/// Represents a YouTube Music radio channel
/// </summary>
/// <param name="playlistId">The id of the playlist associated with this radio channel</param>
/// <param name="videoId">The id of the starting video associated with this radio channel</param>
public class Radio(
    string? playlistId,
    string? videoId)
{
    /// <summary>
    /// The id of the playlist associated with this radio channel
    /// </summary>
    public string? PlaylistId { get; } = playlistId;

    /// <summary>
    /// The id of the starting video associated with this radio channel
    /// </summary>
    public string? VideoId { get; } = videoId;
}