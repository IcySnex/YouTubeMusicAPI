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
    public void Should_search_for_all()
    {
        // Act
        IReadOnlyList<SearchResult>? results = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            results = await client.Search.AllAsync(TestData.SearchQuery);
        });

        // Assert
        Assert.That(results, Is.Not.Null.Or.Empty);

        TestData.WriteResult(results);
    }


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

    [Test]
    public void Should_search_for_playlists()
    {
        // Act
        IReadOnlyList<PlaylistSearchResult>? results = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<PlaylistSearchResult> response = client.Search.PlaylistsAsync(TestData.SearchQuery);

            results = await response.FetchItemsAsync(TestData.FetchOffset, TestData.FetchLimit);
        });

        // Assert
        Assert.That(results, Is.Not.Null.Or.Empty);

        TestData.WriteResult(results);
    }

    [Test]
    public void Should_search_for_albums()
    {
        // Act
        IReadOnlyList<AlbumSearchResult>? results = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<AlbumSearchResult> response = client.Search.AlbumsAsync(TestData.SearchQuery);

            results = await response.FetchItemsAsync(TestData.FetchOffset, TestData.FetchLimit);
        });

        // Assert
        Assert.That(results, Is.Not.Null.Or.Empty);

        TestData.WriteResult(results);
    }

    [Test]
    public void Should_search_for_artists()
    {
        // Act
        IReadOnlyList<ArtistSearchResult>? results = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<ArtistSearchResult> response = client.Search.ArtistsAsync(TestData.SearchQuery);

            results = await response.FetchItemsAsync(TestData.FetchOffset, TestData.FetchLimit);
        });

        // Assert
        Assert.That(results, Is.Not.Null.Or.Empty);

        TestData.WriteResult(results);
    }

    [Test]
    public void Should_search_for_profiles()
    {
        // Act
        IReadOnlyList<ProfileSearchResult>? results = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<ProfileSearchResult> response = client.Search.ProfilesAsync(TestData.SearchQuery);

            results = await response.FetchItemsAsync(TestData.FetchOffset, TestData.FetchLimit);
        });

        // Assert
        Assert.That(results, Is.Not.Null.Or.Empty);

        TestData.WriteResult(results);
    }

    [Test]
    public void Should_search_for_podcasts()
    {
        // Act
        IReadOnlyList<PodcastSearchResult>? results = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<PodcastSearchResult> response = client.Search.PodcastsAsync(TestData.SearchQuery);

            results = await response.FetchItemsAsync(TestData.FetchOffset, TestData.FetchLimit);
        });

        // Assert
        Assert.That(results, Is.Not.Null.Or.Empty);

        TestData.WriteResult(results);
    }

    [Test]
    public void Should_search_for_episodes()
    {
        // Act
        IReadOnlyList<EpisodeSearchResult>? results = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<EpisodeSearchResult> response = client.Search.EpisodesAsync(TestData.SearchQuery);

            results = await response.FetchItemsAsync(TestData.FetchOffset, TestData.FetchLimit);
        });

        // Assert
        Assert.That(results, Is.Not.Null.Or.Empty);

        TestData.WriteResult(results);
    }
}
