using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Pagination;

namespace YouTubeMusicAPI.Tests.Services;

public class SearchServiceTests
{
    YouTubeMusicClient client;

    [SetUp]
    public async Task Setup() =>
        client = await TestData.CreateClientAsync();


    [Test]
    [TestCase(SearchCategory.Songs)]
    [TestCase(SearchCategory.Videos)]
    [TestCase(SearchCategory.CommunityPlaylists)]
    [TestCase(SearchCategory.FeaturedPlaylists)]
    [TestCase(SearchCategory.Albums)]
    [TestCase(SearchCategory.Artists)]
    [TestCase(SearchCategory.Profiles)]
    [TestCase(SearchCategory.Podcasts)]
    [TestCase(SearchCategory.Episodes)]
    public void Should_search_by_category(
        SearchCategory category)
    {
        // Act
        IReadOnlyList<SearchResult>? results = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<SearchResult> response = client.Search.ByCategoryAsync(TestData.SearchQuery, category, TestData.SearchScope, TestData.SearchIgnoreSpelling);

            results = await response.FetchItemsAsync(TestData.FetchOffset, TestData.FetchLimit);
        });

        // Assert
        Assert.That(results, Is.Not.Null.Or.Empty);

        TestData.WriteResult(results);
    }

    [Test]
    public void Should_search_for_all()
    {
        // Act
        SearchPage? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.Search.AllAsync(TestData.SearchQuery, TestData.SearchScope, TestData.SearchIgnoreSpelling);
        });

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Results, Is.Not.Null.Or.Empty);

        TestData.WriteResult(result.TopResult, "Top Result");
        TestData.WriteResult(result.RelatedTopResults, "Related Top Results");
        TestData.WriteResult(result.Results);
    }


    [Test]
    public void Should_get_suggestions()
    {
        // Act
        SearchSuggestions? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.Search.GetSuggestionsAsync(TestData.SearchQuery);
        });

        // Assert
        Assert.That(result, Is.Not.Null);

        TestData.WriteResult(result.Search, "Search");
        TestData.WriteResult(result.History, "History");
        TestData.WriteResult(result.Results);
    }

    [Test]
    public void Should_remove_suggestion()
    {
        // Act
        Assert.DoesNotThrowAsync(async () =>
        {
            await client.Search.RemoveSuggestionAsync(TestData.SearchQuery);
        });
    }
}
