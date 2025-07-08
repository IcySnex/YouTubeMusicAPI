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
    public void Should_search_for_songs()
    {
        // Act
        IReadOnlyList<SongSearchResult>? results = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<SongSearchResult> response = client.Search.SongsAsync(TestData.SearchQuery);

            results = await response.FetchItemsAsync(TestData.FetchOffset, TestData.FetchLimit);
        });

        // Assert
        Assert.That(results, Is.Not.Null.Or.Empty);

        TestData.WriteResult(results);
    }

    [Test]
    public void Should_search_for_videos()
    {
        // Act
        IReadOnlyList<VideoSearchResult>? results = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<VideoSearchResult> response = client.Search.VideosAsync(TestData.SearchQuery);

            results = await response.FetchItemsAsync(TestData.FetchOffset, TestData.FetchLimit);
        });

        // Assert
        Assert.That(results, Is.Not.Null.Or.Empty);

        TestData.WriteResult(results);
    }
}
