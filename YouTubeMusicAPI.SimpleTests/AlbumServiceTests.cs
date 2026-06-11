using FluentAssertions;
using YouTubeMusicAPI.Services.Albums;

namespace YouTubeMusicAPI.SimpleTests;

public class AlbumServiceTests
{
    [Fact]
    public async Task Should_get_albums_from_an_artist()
    {
        var artistService = new YouTubeMusicClient();
        const string beatlesBrowseId = "MPADUC2XdaAVUannpujzv32jcouQ";
        var artistAlbums = await artistService.Albums.GetAllByArtistAsync(beatlesBrowseId,
            AlbumCategory.Albums,
            AlbumSortingOrder.Default,
            TestContext.Current.CancellationToken);

        artistAlbums.Albums.Should().NotBeEmpty();
    }
}
