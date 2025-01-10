using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models.Info;
using YouTubeMusicAPI.Models.Library;

namespace YouTubeMusicAPI.Tests;

/// <summary>
/// Get library
/// </summary>
internal class Library
{
    ILogger logger;
    YouTubeMusicClient client;

    [SetUp]
    public void Setup()
    {
        ILoggerFactory factory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        logger = factory.CreateLogger<Search>();
        //client = new(logger, TestData.GeographicalLocation);

        IEnumerable<Cookie> parsedCookies = TestData.CookiesString
            .Split(';')
            .Select(cookieString =>
            {
                string[] parts = cookieString.Split("=");
                return new Cookie(parts[0], parts[1]) { Domain = ".youtube.com" };
            });
        client = new(logger, TestData.GeographicalLocation, parsedCookies);
    }


    /// <summary>
    /// Get community playlists
    /// </summary>
    [Test]
    public void CommunityPlaylists()
    {
        IEnumerable<LibraryCommunityPlaylist>? communityPlaylists = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            communityPlaylists = await client.GetLibraryCommunityPlaylistsAsync();
        });
        Assert.That(communityPlaylists, Is.Not.Null);
        Assert.That(communityPlaylists, Is.Not.Empty);

        // Output
        string readableResults = JsonConvert.SerializeObject(communityPlaylists, Formatting.Indented);
        logger.LogInformation("\nCommunity Playlists:\n{readableResults}", readableResults);
    }
}