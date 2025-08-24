using YouTubeMusicAPI.Models.Lyrics;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Models.Videos;
using YouTubeMusicAPI.Pagination;

namespace YouTubeMusicAPI.Tests.Services;

public class VideoServiceTests
{
    YouTubeMusicClient client;

    [SetUp]
    public async Task Setup() =>
        client = await TestData.CreateClientAsync();


    [Test]
    public void Should_search()
    {
        // Act
        IReadOnlyList<VideoSearchResult>? results = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<VideoSearchResult> response = client.Videos.SearchAsync(TestData.SearchQuery, TestData.SearchScope, TestData.SearchIgnoreSpelling);

            results = await response.FetchItemsAsync(TestData.FetchOffset, TestData.FetchLimit);
        });

        // Assert
        Assert.That(results, Is.Not.Null.Or.Empty);

        TestData.WriteResult(results);
    }


    [Test]
    public void Should_get()
    {
        // Act
        VideoInfo? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.Videos.GetAsync(TestData.VideoId);
        });

        // Assert
        Assert.That(result, Is.Not.Null);

        TestData.WriteResult(result);
    }


    [Test]
    public async Task Should_get_lyrics()
    {
        // Arrange
        VideoInfo video = await client.Videos.GetAsync(TestData.VideoId);

        if (!video.IsLyricsAvailable)
            Assert.Inconclusive("The provided video does not have available lyrics.");

        // Act
        Lyrics? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.Videos.GetLyricsAsync(video);
        });

        // Assert
        Assert.That(result, Is.Not.Null);

        TestData.WriteResult(result);
    }
}
