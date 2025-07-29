using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Info;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Pagination;

namespace YouTubeMusicAPI.Tests.Services;

public class InfoServiceTests
{
    YouTubeMusicClient client;

    [SetUp]
    public async Task Setup() =>
        client = await TestData.CreateClientAsync();


    [Test]
    public void Should_get_song()
    {
        // Act
        SongInfo? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.Info.GetSongAsync(TestData.SongId);
        });

        // Assert
        Assert.That(result, Is.Not.Null);

        TestData.WriteResult(result);
    }

    [Test]
    public void Should_get_song_credits()
    {
        // Act
        SongCredits? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.Info.GetSongCreditsAsync(TestData.SongId);
        });

        // Assert
        Assert.That(result, Is.Not.Null);

        TestData.WriteResult(result);
    }

}
