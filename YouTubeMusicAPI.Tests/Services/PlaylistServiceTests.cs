using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Services.Playlists;
using YouTubeMusicAPI.Services.Relations;

namespace YouTubeMusicAPI.Tests.Services;

public class PlaylistServiceTests
{
    YouTubeMusicClient client;

    [SetUp]
    public async Task Setup() =>
        client = await TestData.CreateClientAsync();


    [Test]
    public void Should_search_community()
    {
        // Act
        IReadOnlyList<CommunityPlaylistSearchResult>? results = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<CommunityPlaylistSearchResult> response = client.Playlists.SearchCommunityAsync(TestData.SearchQuery, TestData.SearchScope, TestData.SearchIgnoreSpelling);

            results = await response.FetchItemsAsync(TestData.FetchOffset, TestData.FetchLimit);
        });

        // Assert
        Assert.That(results, Is.Not.Null.Or.Empty);

        TestData.WriteResult(results);
    }

    [Test]
    public void Should_search_featured()
    {
        // Act
        IReadOnlyList<FeaturedPlaylistSearchResult>? results = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<FeaturedPlaylistSearchResult> response = client.Playlists.SearchFeaturedAsync(TestData.SearchQuery, TestData.SearchScope, TestData.SearchIgnoreSpelling);

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
            browseId = await client.Playlists.GetBrowseIdAsync(TestData.PlaylistId);
        });

        // Assert
        Assert.That(browseId, Is.Not.Null.Or.Empty);
        Assert.That(browseId, Does.StartWith("VL"));

        TestData.WriteResult(browseId);
    }


    [Test]
    public void Should_get()
    {
        // Act
        PlaylistInfo? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.Playlists.GetAsync(TestData.PlaylistBrowseId);
        });

        // Assert
        Assert.That(result, Is.Not.Null);

        TestData.WriteResult(result);
    }

    [Test]
    public void Should_get_items()
    {
        // Act
        IReadOnlyList<PlaylistItem>? results = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<PlaylistItem> response = client.Playlists.GetItemsAsync(TestData.PlaylistBrowseId);

            results = await response.FetchItemsAsync(TestData.FetchOffset, TestData.FetchLimit);
        });

        // Assert
        Assert.That(results, Is.Not.Null.Or.Empty);

        TestData.WriteResult(results);
    }


    [Test]
    public async Task Should_get_relations()
    {
        // Arrange
        PlaylistInfo playlist = await client.Playlists.GetAsync(TestData.PlaylistBrowseId);

        if (!playlist.IsRelationsAvailable)
            Assert.Inconclusive("The provided playlist does not have available relations.");

        // Act
        PlaylistRelations? result = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            result = await client.Playlists.GetRelationsAsync(playlist);
        });

        // Assert
        Assert.That(result, Is.Not.Null);

        TestData.WriteResult(result.Suggestions, "Suggestions");
        TestData.WriteResult(result.Related, "Related");
    }
}
