#pragma warning disable CS0162 // Unreachable code detected

using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using YouTubeMusicAPI.Authentication;
using YouTubeSessionGenerator;
using YouTubeSessionGenerator.Js.Environments;

namespace YouTubeMusicAPI.Tests;

internal static class TestData
{
    // Write Output
    static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static void WriteResult<T>(
        T result)
    {
        string json = JsonSerializer.Serialize(result, JsonOptions);
        TestContext.Out.WriteLine("Result: {0}", json);
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
    public const string GeographicalLocation = "US";

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
    public const string SearchQuery = "Pashanim";

    public const int FetchOffset = 0;
    public const int FetchLimit = 20;
}