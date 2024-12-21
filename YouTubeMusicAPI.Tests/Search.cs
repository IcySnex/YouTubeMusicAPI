using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Search;

namespace YouTubeMusicAPI.Tests;

/// <summary>
/// Search for shelf items
/// </summary>
internal class Search
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
        client = new(logger, TestData.GeographicalLocation);
    }


    /// <summary>
    /// Search for shelf items
    /// </summary>
    /// <typeparam name="T">The type of shelf items to search for</typeparam>
    void Test<T>() where T : IYouTubeMusicItem
    {
        IEnumerable<T>? searchResults = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            searchResults = await client.SearchAsync<T>(TestData.SearchQuery, TestData.SearchQueryLimit);
        });
        Assert.That(searchResults, Is.Not.Null);
        Assert.That(searchResults, Is.Not.Empty);

        // Output
        string readableResults = JsonConvert.SerializeObject(searchResults, Formatting.Indented);
        logger.LogInformation("\nSearch Results ({resultsCount}):\n{readableResults}", searchResults.Count(), readableResults);
    }


    /// <summary>
    /// Search for all shelf items
    /// </summary>
    [Test]
    public void All() =>
        Test<IYouTubeMusicItem>();


    /// <summary>
    /// Search for songs 
    /// </summary>
    [Test]
    public void Songs() =>
        Test<SongSearchResult>();

    /// <summary>
    /// Search for videos 
    /// </summary>
    [Test]
    public void Videos() =>
        Test<VideoSearchResult>();

    /// <summary>
    /// Search for albums 
    /// </summary>
    [Test]
    public void Albums() =>
        Test<AlbumSearchResult>();

    /// <summary>
    /// Search for community playlists 
    /// </summary>
    [Test]
    public void CommunityPlaylists() =>
        Test<CommunityPlaylistSearchResult>();

    /// <summary>
    /// Search for artists 
    /// </summary>
    [Test]
    public void Artists() =>
        Test<ArtistSearchResult>();

    /// <summary>
    /// Search for songs 
    /// </summary>
    [Test]
    public void Podcasts() =>
        Test<PodcastSearchResult>();

    /// <summary>
    /// Search for podcasts episodes 
    /// </summary>
    [Test]
    public void Episodes() =>
        Test<EpisodeSearchResult>();

    /// <summary>
    /// Search for profiles 
    /// </summary>
    [Test]
    public void Profiles() =>
        Test<ProfileSearchResult>();
}