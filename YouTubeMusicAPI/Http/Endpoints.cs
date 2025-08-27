namespace YouTubeMusicAPI.Http;

/// <summary>
/// Provides constants for all kinds of YouTube URLs.
/// </summary>
internal static class Endpoints
{
    /// <summary>
    /// The base URL for the YouTube Music API.
    /// </summary>
    const string MusicApiUrl = "https://music.youtube.com/youtubei/v1";

    /// <summary>
    /// The base URL for the YouTube Music web page.
    /// </summary>
    const string MusicWebUrl = "https://music.youtube.com";


    /// <summary>
    /// The path to the account menu API.
    /// </summary>
    public const string AccountMenu = MusicApiUrl + "/account/account_menu";

    /// <summary>
    /// The path to the search API.
    /// </summary>
    public const string Search = MusicApiUrl + "/search";

    /// <summary>
    /// The path to the search suggestions API.
    /// </summary>
    public const string SearchSuggestions = MusicApiUrl + "/music/get_search_suggestions";

    /// <summary>
    /// The path to the feedback API.
    /// </summary>
    public const string Feedback = MusicApiUrl + "/feedback";

    /// <summary>
    /// The path to the next API.
    /// </summary>
    public const string Next = MusicApiUrl + "/next";

    /// <summary>
    /// The path to the browse API.
    /// </summary>
    public const string Browse = MusicApiUrl + "/browse";


    /// <summary>
    /// The path to the playlist web page.
    /// </summary>
    public const string Playlists = MusicWebUrl + "/playlist";
}