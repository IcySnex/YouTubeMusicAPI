using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models;

namespace YouTubeMusicAPI.Tests;

/// <summary>
/// Get info
/// </summary>
internal class Info
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
        client = new(logger);
    }


    /// <summary>
    /// Get song info
    /// </summary>
    [Test]
    public void GetSong()
    {
        SongInfo? song = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            song = await client.GetSongInfoAsync(TestData.SongId, TestData.HostLanguage, TestData.GeographicalLocation);
        });
        Assert.That(song, Is.Not.Null);

        // Output
        string readableResults = JsonConvert.SerializeObject(song, Formatting.Indented);
        logger.LogInformation("\nSong Info:\n{readableResults}", readableResults);
    }
}