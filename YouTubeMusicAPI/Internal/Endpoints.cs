﻿namespace YouTubeMusicAPI.Internal;

/// <summary>
/// Contains endpoints for the YouTube Music internal API
/// </summary>
internal abstract class Endpoints
{
    /// <summary>
    /// The base url to the YouTube Music internal API
    /// </summary>
    public const string MusicApiUrl = "https://music.youtube.com/youtubei/v1";

    /// <summary>
    /// The web url to YouTube Music
    /// </summary>
    public const string MusicWebUrl = "https://music.youtube.com";

    /// <summary>
    /// The web url to YouTube
    /// </summary>
    public const string YouTubeWebUrl = "https://www.youtube.com";

    /// <summary>
    /// The url to consent to Googles cookies
    /// </summary>
    public const string CookiesUrl = "https://consent.youtube.com";


    /// <summary>
    /// The path the search endpoint
    /// </summary>
    public const string Search = "/search";

    /// <summary>
    /// The path the player endpoint
    /// </summary>
    public const string Player = "/player";

    /// <summary>
    /// The path the next endpoint
    /// </summary>
    public const string Next = "/next";

    /// <summary>
    /// The path the browse endpoint
    /// </summary>
    public const string Browse = "/browse";

    /// <summary>
    /// The path the playlist endpoint
    /// </summary>
    public const string Playlist = "/playlist";

    /// <summary>
    /// The path the save Googles cookies
    /// </summary>
    public const string Save = "/save";

    /// <summary>
    /// The path the iframe api
    /// </summary>
    public const string Iframe = "/iframe_api";


    /// <summary>
    /// The path to the embed player
    /// </summary>
    public static string Embed(
        string videoId) =>
        $"/embed/{videoId}";

    /// <summary>
    /// The path to the player source for a specific version
    /// </summary>
    public static string PlayerSource(
        string version) =>
        $"/s/player/{version}/player_ias.vflset/en_US/base.js";
}