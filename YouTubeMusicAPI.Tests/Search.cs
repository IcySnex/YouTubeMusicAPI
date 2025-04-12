using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Types;

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
        client = new(logger, TestData.GeographicalLocation, TestData.VisitorData, TestData.PoToken, TestData.Cookies);
    }


    void Test(
        SearchCategory? category)
    {
        IReadOnlyList<SearchResult>? bufferedSearchResults = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<SearchResult> searchResults = client.SearchAsync(TestData.SearchQuery, category);
            bufferedSearchResults = await searchResults.FetchItemsAsync(TestData.FetchOffset, TestData.FetchLimit);
        });
        Assert.That(bufferedSearchResults, Is.Not.Null);
        Assert.That(bufferedSearchResults, Is.Not.Empty);

        // Output
        string readableResults = JsonConvert.SerializeObject(bufferedSearchResults, Formatting.Indented);
        logger.LogInformation("\nSearch Results ({resultsCount}):\n{readableResults}", bufferedSearchResults.Count, readableResults);
    }


    /// <summary>
    /// Search for all shelf items
    /// </summary>
    [Test]
    public void All() =>
        Test(null);


    /// <summary>
    /// Search for songs 
    /// </summary>
    [Test]
    public void Songs() =>
        Test(SearchCategory.Songs);

    /// <summary>
    /// Search for videos 
    /// </summary>
    [Test]
    public void Videos() =>
        Test(SearchCategory.Videos);

    /// <summary>
    /// Search for albums 
    /// </summary>
    [Test]
    public void Albums() =>
        Test(SearchCategory.Albums);

    /// <summary>
    /// Search for community playlists 
    /// </summary>
    [Test]
    public void CommunityPlaylists() =>
        Test(SearchCategory.CommunityPlaylists);

    /// <summary>
    /// Search for artists 
    /// </summary>
    [Test]
    public void Artists() =>
        Test(SearchCategory.Artists);

    /// <summary>
    /// Search for songs 
    /// </summary>
    [Test]
    public void Podcasts() =>
        Test(SearchCategory.Podcasts);

    /// <summary>
    /// Search for podcasts episodes 
    /// </summary>
    [Test]
    public void Episodes() =>
        Test(SearchCategory.Episodes);

    /// <summary>
    /// Search for profiles 
    /// </summary>
    [Test]
    public void Profiles() =>
        Test(SearchCategory.Profiles);
}