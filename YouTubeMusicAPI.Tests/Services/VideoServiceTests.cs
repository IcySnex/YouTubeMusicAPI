using YouTubeMusicAPI.Models.MediaItems;
using YouTubeMusicAPI.Models.Videos;

namespace YouTubeMusicAPI.Tests.Services;

public class VideoServiceTests
{
    YouTubeMusicClient client;

    [SetUp]
    public async Task Setup() =>
        client = await TestData.CreateClientAsync();


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
    public void Should_get_lyrics()
    {
        // Act
        Lyrics? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.Videos.GetLyricsAsync(TestData.VideoLyricsBrowseId);
        });

        // Assert
        Assert.That(result, Is.Not.Null);

        TestData.WriteResult(result);
    }
}
