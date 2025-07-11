namespace YouTubeMusicAPI.Http;

/// <summary>
/// Provides constants and methods for constructing YouTube URLs.
/// </summary>
internal static class Endpoints
{
    const string MusicApiUrl = "https://music.youtube.com/youtubei/v1";


    /// <summary>
    /// The path to the search endpoint.
    /// </summary>
    public const string Search = MusicApiUrl + "/search";


    /// <summary>
    /// The path to the search suggestions endpoint.
    /// </summary>
    public const string SearchSuggestions = MusicApiUrl + "/music/get_search_suggestions";
}