using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Services.Artists;

namespace YouTubeMusicAPI.Tests.Services;

public class ArtistServiceTests
{
    YouTubeMusicClient client;

    [SetUp]
    public async Task Setup() =>
        client = await TestData.CreateClientAsync();


    [Test]
    public void Should_search()
    {
        // Act
        IReadOnlyList<ArtistSearchResult>? results = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<ArtistSearchResult> response = client.Artists.SearchAsync(TestData.SearchQuery, TestData.SearchScope, TestData.SearchIgnoreSpelling);

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
        ArtistInfo? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.Artists.GetAsync(TestData.ArtistBrowseId);
        });

        // Assert
        Assert.That(result, Is.Not.Null);

        TestData.WriteResult(result);
    }

    [Test]
    public async Task Should_get_albums_from_an_artist()
    {
        var artistAlbums = await client.Artists.GetAlbumsAsync(
            TestData.BeatlesBrowseId,
            TestData.BeatlesAlbumParams);

        Assert.That(artistAlbums.Albums, Is.Not.Null.Or.Empty);
    }

    [Test]
    public async Task Should_get_albums_from_an_artist_with_ordering()
    {
        var artistAlbums = await client.Artists.GetAlbumsAsync(
            TestData.BeatlesBrowseId,
            TestData.BeatlesAlbumParams,
            AlbumSortingOrder.Popularity);

        Assert.That(artistAlbums.Albums, Is.Not.Null.Or.Empty);
    }
}
