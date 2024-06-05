namespace YouTubeMusicAPI.Models;

/// <summary>
/// Represents a YouTube Music radio channel
/// </summary>
/// <param name="playlistId">The id of the playlist associated with this radio channel</param>
/// <param name="videoId">The id of the starting video associated with this radio channel</param>
public class Radio(
    string playlistId,
    string? videoId)
{
    /// <summary>
    /// Gets the url of this radio channel which leads to YouTube music
    /// </summary>
    /// <param name="radio">The radio channel to get the url for </param>
    /// <returns>An url of this radio channel which leads to YouTube music</returns>
    public static string GetUrl(
        Radio radio) =>
        radio.VideoId is null ? $"https://music.youtube.com/watch?list={radio.PlaylistId}" : $"https://music.youtube.com/watch?v={radio.VideoId}&list={radio.PlaylistId}";


    /// <summary>
    /// The id of the playlist associated with this radio channel
    /// </summary>
    public string PlaylistId { get; } = playlistId;

    /// <summary>
    /// The id of the starting video associated with this radio channel
    /// </summary>
    public string? VideoId { get; } = videoId;
}