using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Tests;

/// <summary>
/// Search for shelves
/// </summary>
internal class SearchShelves
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
        client = new(logger, TestData.GeographicalLocation, TestData.VisitorData, TestData.PoToken, TestData.Cookies);
    }


    /// <summary>
    /// Search for shelves
    /// </summary>
    /// <param name="kind">The type of shelf to search for</param>
    void Test(
        YouTubeMusicItemKind? kind)
    {
        IEnumerable<Shelf>? searchResults = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            searchResults = await client.SearchAsync(TestData.SearchQuery, TestData.SearchQueryContinuationToken, kind);
        });
        Assert.That(searchResults, Is.Not.Null);
        Assert.That(searchResults, Is.Not.Empty);

        // Output
        string readableResults = JsonConvert.SerializeObject(searchResults, Formatting.Indented);
        logger.LogInformation("\nSearch Results:\n{readableResults}", readableResults);
    }


    /// <summary>
    /// Search for all shelves
    /// </summary>
    [Test]
    public void All() =>
        Test(null);


    /// <summary>
    /// Search for songs shelf
    /// </summary>
    [Test]
    public void Songs() =>
        Test(YouTubeMusicItemKind.Songs);

    /// <summary>
    /// Search for videos shelf
    /// </summary>
    [Test]
    public void Videos() =>
        Test(YouTubeMusicItemKind.Videos);

    /// <summary>
    /// Search for albums shelf
    /// </summary>
    [Test]
    public void Albums() =>
        Test(YouTubeMusicItemKind.Albums);

    /// <summary>
    /// Search for community playlists shelf
    /// </summary>
    [Test]
    public void CommunityPlaylists() =>
        Test(YouTubeMusicItemKind.CommunityPlaylists);

    /// <summary>
    /// Search for artists shelf
    /// </summary>
    [Test]
    public void Artists() =>
        Test(YouTubeMusicItemKind.Artists);

    /// <summary>
    /// Search for podcasts shelf
    /// </summary>
    [Test]
    public void Podcasts() =>
        Test(YouTubeMusicItemKind.Podcasts);

    /// <summary>
    /// Search for podcasts episodes shelf
    /// </summary>
    [Test]
    public void Episodes() =>
        Test(YouTubeMusicItemKind.Episodes);

    /// <summary>
    /// Search for profiles shelf
    /// </summary>
    [Test]
    public void Profiles() =>
        Test(YouTubeMusicItemKind.Profiles);
}