using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Services.Albums;

namespace YouTubeMusicAPI.Tests.Services;

public class AlbumServiceTests
{
    YouTubeMusicClient client;

    [SetUp]
    public async Task Setup() =>
        client = await TestData.CreateClientAsync();


    [Test]
    public void Should_search()
    {
        // Act
        IReadOnlyList<AlbumSearchResult>? results = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<AlbumSearchResult> response = client.Albums.SearchAsync(TestData.SearchQuery, TestData.SearchScope, TestData.SearchIgnoreSpelling);

            results = await response.FetchItemsAsync(TestData.FetchOffset, TestData.FetchLimit);
        });

        // Assert
        Assert.That(results, Is.Not.Null.Or.Empty);

        TestData.WriteResult(results);
    }


    [Test]
    public void Should_get_browse_id()
    {
        // Act
        string? browseId = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            browseId = await client.Albums.GetBrowseIdAsync(TestData.AlbumId);
        });

        // Assert
        Assert.That(browseId, Is.Not.Null.Or.Empty);
        Assert.That(browseId, Does.StartWith("MPRE"));

        TestData.WriteResult(browseId);
    }


    [Test]
    public void Should_get()
    {
        // Act
        AlbumInfo? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.Albums.GetAsync(TestData.AlbumBrowseId);
        });

        // Assert
        Assert.That(result, Is.Not.Null);

        TestData.WriteResult(result);
    }
}
