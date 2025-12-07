#pragma warning disable CS0162 // Unreachable code detected

using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using YouTubeMusicAPI.Http.Authentication;
using YouTubeMusicAPI.Services.Search;
using YouTubeSessionGenerator;
using YouTubeSessionGenerator.Js.Environments;

namespace YouTubeMusicAPI.Tests;

internal static class TestData
{
    // Write Output
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public static void WriteResult<T>(
        T? result,
        string name = "Result")
    {
        string json = JsonSerializer.Serialize<object?>(result, JsonOptions);
        TestContext.Out.WriteLine("{0}: {1}", name, json);
    }

    public static void WriteResult<T>(
        IReadOnlyList<T>? results,
        string name = "Results")
    {
        string json = JsonSerializer.Serialize<object?>(results?.Cast<object?>().ToList(), JsonOptions);
        TestContext.Out.WriteLine("{0} (Count: {1}): {2}", name, results?.Count, json);
    }


    // Random
    public static string RandomString() =>
        Guid.NewGuid().ToString();

    public static Cookie[] RandomCookies() =>
        [
            new Cookie("__Secure-3PAPISID", RandomString()) { Domain = ".youtube.com" },
            new Cookie("SAPISID", RandomString()) { Domain = ".youtube.com" },
            new Cookie("SSID", RandomString()) { Domain = ".youtube.com" }
            //...
        ];


    // Session
    public const bool GenerateSession = false;

    public const string? VisitorData = null;

    public const string? RolloutToken = null;

    public const string? ProofOfOriginToken = null;

    public const string? Cookies = null;


    // Client
    public const string GeographicalLocation = "DE";

    public static async Task<YouTubeMusicClient> CreateClientAsync()
    {
        // Logging
        ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger logger = factory.CreateLogger<Program>();

        // Session
        IAuthenticator authenticator;

        if (GenerateSession)
        {
            using NodeEnvironment nodeEnvironment = new();

            YouTubeSessionConfig sessionConfig = new()
            {
                Logger = logger,
                JsEnvironment = nodeEnvironment
            };
            YouTubeSessionCreator sessionCreator = new(sessionConfig);

            string visitorData = await sessionCreator.VisitorDataAsync();
            string rolloutTOken = await sessionCreator.RolloutTokenAsync();
            string proofOfOriginToken = await sessionCreator.ProofOfOriginTokenAsync(visitorData);

            authenticator = new AnonymousAuthenticator(visitorData, rolloutTOken, proofOfOriginToken);
        }
        else
        {
            IEnumerable<Cookie>? cookies = Cookies
                ?.Split(';')
                .Select(cookieString =>
                {
                    string[] parts = cookieString.Split("=");
                    return new Cookie(parts[0], parts[1]) { Domain = ".youtube.com" };
                });

            authenticator = cookies is null
                ? new AnonymousAuthenticator(VisitorData, RolloutToken, ProofOfOriginToken)
                : new CookieAuthenticator(cookies, VisitorData, RolloutToken, ProofOfOriginToken);
        }

        // Client
        YouTubeMusicConfig clientConfig = new()
        {
            GeographicalLocation = GeographicalLocation,
            Logger = logger,
            Authenticator = authenticator
        };
        return new(clientConfig);
    }


    // Test Data
    public const int FetchOffset = 0;
    public const int FetchLimit = 20;

    public const string SearchQuery = "relax";
    public const SearchScope SearchScope = YouTubeMusicAPI.Services.Search.SearchScope.Global;
    public const bool SearchIgnoreSpelling = false;

    public const string SongId = "dhxjg3SKDUw";

    public const string VideoId = "s70j3BHPt9w";

    public const string PlaylistId = "PLUoI0V1lAvcLY47xdnLWGdf9zyUWKsulx";
    public const string PlaylistBrowseId = "VLPLUoI0V1lAvcLY47xdnLWGdf9zyUWKsulx";

    public const string AlbumId = "OLAK5uy_kncRLJuxDsO1_nh_xSEkUCEYguyHljIgY";
    public const string AlbumBrowseId = "MPREb_78cZiOmrPmB";

    public const string ArtistBrowseId = "UCUTgzHNGIufzRPkRHdEJUNQ";

    public const string ProfileBrowseId = "UCJLHPUBD6KvubhoJ0mxmGiw";
}