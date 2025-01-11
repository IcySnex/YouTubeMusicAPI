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

    /// <summary>
    /// Get songs
    /// </summary>
    [Test]
    public void Songs()
    {
        IEnumerable<LibrarySong>? songs = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            songs = await client.GetLibrarySongsAsync();
        });
        Assert.That(songs, Is.Not.Null);
        Assert.That(songs, Is.Not.Empty);

        // Output
        string readableResults = JsonConvert.SerializeObject(songs, Formatting.Indented);
        logger.LogInformation("\nSongs:\n{readableResults}", readableResults);
    }

    /// <summary>
    /// Get albums
    /// </summary>
    [Test]
    public void Albums()
    {
        IEnumerable<LibraryAlbum>? albums = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            albums = await client.GetLibraryAlbumsAsync();
        });
        Assert.That(albums, Is.Not.Null);
        Assert.That(albums, Is.Not.Empty);

        // Output
        string readableResults = JsonConvert.SerializeObject(albums, Formatting.Indented);
        logger.LogInformation("\nAlbums:\n{readableResults}", readableResults);
    }

    /// <summary>
    /// Get artists
    /// </summary>
    [Test]
    public void Artists()
    {
        IEnumerable<LibraryArtist>? artists = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            artists = await client.GetLibraryArtistsAsync();
        });
        Assert.That(artists, Is.Not.Null);
        Assert.That(artists, Is.Not.Empty);

        // Output
        string readableResults = JsonConvert.SerializeObject(artists, Formatting.Indented);
        logger.LogInformation("\nArtists:\n{readableResults}", readableResults);
    }

    /// <summary>
    /// Get subscriptions
    /// </summary>
    [Test]
    public void Subscriptions()
    {
        IEnumerable<LibrarySubscription>? subscriptions = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            subscriptions = await client.GetLibrarySubscriptionsAsync();
        });
        Assert.That(subscriptions, Is.Not.Null);
        Assert.That(subscriptions, Is.Not.Empty);

        // Output
        string readableResults = JsonConvert.SerializeObject(subscriptions, Formatting.Indented);
        logger.LogInformation("\nSubscriptions:\n{readableResults}", readableResults);
    }

    /// <summary>
    /// Get podcasts
    /// </summary>
    [Test]
    public void Podcasts()
    {
        IEnumerable<LibraryPodcast>? podcasts = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            podcasts = await client.GetLibraryPodcastsAsync();
        });
        Assert.That(podcasts, Is.Not.Null);
        Assert.That(podcasts, Is.Not.Empty);

        // Output
        string readableResults = JsonConvert.SerializeObject(podcasts, Formatting.Indented);
        logger.LogInformation("\nPodcasts:\n{readableResults}", readableResults);
    }
}