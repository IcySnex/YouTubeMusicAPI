namespace YouTubeMusicAPI.Tests;

/// <summary>
/// Contains test data for tests
/// </summary>
internal abstract class TestData
{
    /// <summary>
    /// Test query for search requests
    /// </summary>
    public const string SearchQuery = "Pashanim";

    /// <summary>
    /// Test search query continuation token to get further elemnts from a pervious search
    /// </summary>
    public const string SearchQueryContinuationToken = null;

    /// <summary>
    /// Test search query limit of items to return
    /// </summary>
    public const int SearchQueryLimit = 200;


    /// <summary>
    /// Test song or video id
    /// </summary>
    public const string SongVideoId = "Waaa9VBoVpI";
    
    /// <summary>
    /// Test album id
    /// </summary>
    public const string AlbumId = "OLAK5uy_muEnh0WPCqRdkgV3Qg24ttvmZTP1_RBTo";
    
    /// <summary>
    /// Test playlist id
    /// </summary>
    public const string PlaylistId = "PLuvXOFt0CoEbwWSQj5LmzPhIVKS0SvJ-1";
    

    /// <summary>
    /// Test album browse id
    /// </summary>
    public const string AlbumBrowseId = "MPREb_6NKg8kkJLf7";

    /// <summary>
    /// Test playlist browse id
    /// </summary>
    public const string PlaylistBrowseId = "VLPLuvXOFt0CoEbwWSQj5LmzPhIVKS0SvJ-1";
    
    /// <summary>
    /// Test artist browse id
    /// </summary>
    public const string ArtistBrowseId = "UCESdnJ-8tBDMqqkPpjHOuMg";
    

    /// <summary>
    /// Test geographical location for requests
    /// </summary>
    public const string GeographicalLocation = "US";

    /// <summary>
    /// Test cookies for authentication
    /// 
    /// To parse, use:
    /// IEnumerable<Cookie> parsedCookies = cookiesString
    ///     .Split(';')
    ///     .Select(cookieString =>
    ///     {
    ///         string[] parts = cookieString.Split("=");
    ///         return new Cookie(parts[0], parts[1]) { Domain = ".youtube.com" };
    ///     });
    /// 
    /// </summary>
    public const string CookiesString = "wide=...;SOCS=...;_gcl_au=...;PREF=...;LOGIN_INFO=...;__Secure-1PSIDTS=...;__Secure-3PSIDTS=...;__Secure-1PSID=...;__Secure-3PSID=...;HSID=...;SSID=...;APISID=...;SAPISID=...;__Secure-1PAPISID=...;__Secure-3PAPISID=...;__Secure-YEC=...;SIDCC=...;__Secure-1PSIDCC=...;__Secure-3PSIDCC=...";


    /// <summary>
    /// File path to download test media stream
    /// </summary>
    public const string FilePath = @"C:\Users\User69\Desktop\test.mp4";
}