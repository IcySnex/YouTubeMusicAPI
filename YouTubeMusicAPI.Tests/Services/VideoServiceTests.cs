using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Services.Lyrics;
using YouTubeMusicAPI.Services.Relations;
using YouTubeMusicAPI.Services.Videos;

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
    public async Task Should_get_relations()
    {
        // Arrange
        VideoInfo video = await client.Videos.GetAsync(TestData.VideoId);

        // Act
        SongVideoRelations? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.Videos.GetRelationsAsync(video);
        });

        // Assert
        Assert.That(result, Is.Not.Null);

        TestData.WriteResult(result.YouMightAlsoLike, "You might also like");
        TestData.WriteResult(result.RecommendedPlaylists, "Recommended playlists");
        TestData.WriteResult(result.OtherPerformances, "Other performances");
        TestData.WriteResult(result.SimilarArtists, "Similar artists");
        TestData.WriteResult(result.MoreFrom, "More from");
        TestData.WriteResult(result.AboutTheArtist, "About the artist");
    }

    [Test]
    public async Task Should_get_lyrics()
    {
        // Arrange
        VideoInfo video = await client.Videos.GetAsync(TestData.VideoId);

        if (!video.IsLyricsAvailable)
            Assert.Inconclusive("The provided video does not have available lyrics.");

        // Act
        SongVideoLyrics? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.Videos.GetLyricsAsync(video);
        });

        // Assert
        Assert.That(result, Is.Not.Null);

        TestData.WriteResult(result);
    }
}
