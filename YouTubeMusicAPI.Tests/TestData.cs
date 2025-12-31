using System.Net;

namespace YouTubeMusicAPI.Tests;

/// <summary>
/// Contains test data for tests
/// </summary>
internal abstract class TestData
{
    /// <summary>
    /// Test geographical location for requests
    /// </summary>
    public const string GeographicalLocation = "US";

    /// <summary>
    /// Test visitor data for requests
    /// </summary>
    public const string? VisitorData = null;

    /// <summary>
    /// Test po token for requests
    /// </summary>
    public const string? PoToken = null;
        
    /// <summary>
    /// Test cookies for authentication
    /// </summary>
    public static IEnumerable<Cookie>? Cookies
    {
        get
        {
            string? cookies = null;
                
            return cookies?
                .Split(';')
                .Select(cookieString =>
                {
                    string[] parts = cookieString.Split("=");
                    return new Cookie(parts[0], parts[1]) { Domain = ".youtube.com" };
                }) ?? null;
        }
    }


    /// <summary>
    /// Test query for search requests
    /// </summary>
    public const string SearchQuery = "Pashanim";


    /// <summary>
    /// Test offset from which items should be returned for fetch requests
    /// </summary>
    public const int FetchOffset = 0;

    /// <summary>
    /// Test maximum items of items which should be returned for fetch requests
    /// </summary>
    public const int FetchLimit = 20;


    /// <summary>
    /// Test song or video id
    /// </summary>
    public const string SongVideoId = "MLKBxET5Lzg";

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
    public const string AlbumBrowseId = "MPREb_H2RWN4XY0Ny";

    /// <summary>
    /// Test playlist browse id
    /// </summary>
    public const string PlaylistBrowseId = "RDAMVMnP4Q-iszqb0";

    /// <summary>
    /// Test artist browse id
    /// </summary>
    public const string ArtistBrowseId = "UC5_H9CxsfukWbjYeYepADsw";


    /// <summary>
    /// Test watch time for update requests
    /// </summary>
    public static TimeSpan WatchTime = TimeSpan.FromMinutes(1.2);


    /// <summary>
    /// File path to download test media stream
    /// </summary>
    public static readonly string FilePath = @$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\test.mp4";
}
