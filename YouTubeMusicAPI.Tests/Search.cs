using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Shelf;

namespace YouTubeMusicAPI.Tests;

/// <summary>
/// Search for shelf items
/// </summary>
public class Search
{
    ILogger logger;
    SearchClient client;

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
    /// Search for shelf items
    /// </summary>
    /// <typeparam name="T">The type of shelf items to search for</typeparam>
    void Test<T>() where T : IShelfItem
    {
        IEnumerable<T>? searchResults = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            searchResults = await client.SearchAsync<T>(TestData.Query, TestData.HostLanguage, TestData.GeographicalLocation);
        });
        Assert.That(searchResults, Is.Not.Null);
        Assert.That(searchResults, Is.Not.Empty);

        // Output
        string readableResults = JsonConvert.SerializeObject(searchResults, Formatting.Indented);
        logger.LogInformation("\nSearch Results:\n{readableResults}", readableResults);
    }


    /// <summary>
    /// Search for all shelf items
    /// </summary>
    [Test]
    public void All() =>
        Test<IShelfItem>();


    /// <summary>
    /// Search for songs 
    /// </summary>
    [Test]
    public void Songs() =>
        Test<Song>();

    /// <summary>
    /// Search for videos 
    /// </summary>
    [Test]
    public void Videos() =>
        Test<Video>();

    /// <summary>
    /// Search for albums 
    /// </summary>
    [Test]
    public void Albums() =>
        Test<Album>();

    /// <summary>
    /// Search for community playlists 
    /// </summary>
    [Test]
    public void CommunityPlaylists() =>
        Test<CommunityPlaylist>();

    /// <summary>
    /// Search for artists 
    /// </summary>
    [Test]
    public void Artists() =>
        Test<Artist>();

    /// <summary>
    /// Search for songs 
    /// </summary>
    [Test]
    public void Podcasts() =>
        Test<Podcast>();

    /// <summary>
    /// Search for podcasts episodes 
    /// </summary>
    [Test]
    public void Episodes() =>
        Test<Episode>();

    /// <summary>
    /// Search for profiles 
    /// </summary>
    [Test]
    public void Profiles() =>
        Test<Profile>();
}