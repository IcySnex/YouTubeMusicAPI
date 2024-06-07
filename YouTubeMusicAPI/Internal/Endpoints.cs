namespace YouTubeMusicAPI.Internal;

/// <summary>
/// Contains endpoints for the YouTube Music internal API
/// </summary>
internal abstract class Endpoints
{
    /// <summary>
    /// The base url to the YouTube Music internal API
    /// </summary>
    public const string BaseUrl = "https://music.youtube.com/youtubei/v1";


    /// <summary>
    /// The path the search endpoint
    /// </summary>
    public const string Search = "/search";

    /// <summary>
    /// The path the song info endpoint
    /// </summary>
    public const string SongInfo = "/player";
}