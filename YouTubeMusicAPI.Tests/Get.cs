using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models.Info;

namespace YouTubeMusicAPI.Tests;

/// <summary>
/// Get information
/// </summary>
internal class Get
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
        client = new(logger, TestData.HostLanguage, TestData.GeographicalLocation);
    }


    /// <summary>
    /// Get browse id from album or community playlist
    /// </summary>
    [Test]
    public void BrowseId()
    {
        string? albumBrowseId = null;
        string? playlistBrowseId = null;
        Assert.DoesNotThrowAsync(async () =>
        {
            albumBrowseId = await client.GetBrowseIdAsync(TestData.AlbumId);
            playlistBrowseId = await client.GetBrowseIdAsync(TestData.PlaylistId);
        });
        Assert.That(albumBrowseId, Is.Not.Null);
        Assert.That(playlistBrowseId, Is.Not.Null);

        // Output
        logger.LogInformation("\nAlbum Browse Id: {albumBrowseId}\nPlaylist Browse Id: {playlistBrowseId}", albumBrowseId, playlistBrowseId);
    }


    /// <summary>
    /// Get song information
    /// </summary>
    [Test]
    public void Song()
    {
        SongInfo? song = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            song = await client.GetSongInfoAsync(TestData.SongId);
        });
        Assert.That(song, Is.Not.Null);

        // Output
        string readableResults = JsonConvert.SerializeObject(song, Formatting.Indented);
        logger.LogInformation("\nSong Info:\n{readableResults}", readableResults);
    }

    /// <summary>
    /// Get song information
    /// </summary>
    [Test]
    public void Album()
    {
        AlbumInfo? song = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            song = await client.GetAlbumInfoAsync(TestData.AlbumBrowseId);
        });
        Assert.That(song, Is.Not.Null);

        // Output
        string readableResults = JsonConvert.SerializeObject(song, Formatting.Indented);
        logger.LogInformation("\nAlbum Info:\n{readableResults}", readableResults);
    }
}