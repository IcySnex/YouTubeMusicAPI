using YouTubeMusicAPI.Models.Lyrics;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Models.Songs;
using YouTubeMusicAPI.Pagination;

namespace YouTubeMusicAPI.Tests.Services;

public class SongServiceTests
{
    YouTubeMusicClient client;

    [SetUp]
    public async Task Setup() =>
        client = await TestData.CreateClientAsync();


    [Test]
    public void Should_search()
    {
        // Act
        IReadOnlyList<SongSearchResult>? results = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<SongSearchResult> response = client.Songs.SearchAsync(TestData.SearchQuery, TestData.SearchScope, TestData.SearchIgnoreSpelling);

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
        SongInfo? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.Songs.GetAsync(TestData.SongId);
        });

        // Assert
        Assert.That(result, Is.Not.Null);

        TestData.WriteResult(result);
    }


    [Test]
    public async Task Should_get_credits()
    {
        // Arrange
        SongInfo song = await client.Songs.GetAsync(TestData.SongId);

        if (!song.IsCreditsAvailable)
            Assert.Inconclusive("The provided song does not have available credits.");

        // Act
        SongCredits? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.Songs.GetCreditsAsync(song.Id);
        });

        // Assert
        Assert.That(result, Is.Not.Null);

        TestData.WriteResult(result);
    }

    [Test]
    public async Task Should_get_lyrics()
    {
        // Arrange
        SongInfo song = await client.Songs.GetAsync(TestData.SongId);

        if (!song.IsLyricsAvailable)
            Assert.Inconclusive("The provided song does not have available lyrics.");

        // Act
        Lyrics? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.Songs.GetLyricsAsync(song);
        });

        // Assert
        Assert.That(result, Is.Not.Null);

        TestData.WriteResult(result);
    }

}
