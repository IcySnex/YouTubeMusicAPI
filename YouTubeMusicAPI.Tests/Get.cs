using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models.Info;
using YouTubeMusicAPI.Pagination;

namespace YouTubeMusicAPI.Tests;

/// <summary>
/// Get information
/// </summary>
internal class Get
{
    ILogger logger;
    YouTubeMusicClient client;

    [SetUp]
    public void Setup()
    {
        ILoggerFactory factory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        logger = factory.CreateLogger<Search>();
        client = new(logger, TestData.GeographicalLocation, TestData.VisitorData, TestData.PoToken, TestData.Cookies, TestData.PlayerId);
    }


    /// <summary>
    /// Get browse id from an album
    /// </summary>
    [Test]
    public void AlbumBrowseId()
    {
        string? browseId = null;
        Assert.DoesNotThrowAsync(async () =>
        {
            browseId = await client.GetAlbumBrowseIdAsync(TestData.AlbumId);
        });
        Assert.That(browseId, Is.Not.Null);

        // Output
        logger.LogInformation("\nAlbum Browse Id: {browseId} ", browseId);
    }
    
    /// <summary>
    /// Get browse id from a playlist
    /// </summary>
    [Test]
    public void CommunityPlaylistBrowseId()
    {
        string? browseId = null;
        Assert.DoesNotThrow(() =>
        {
            browseId = client.GetCommunityPlaylistBrowseId(TestData.PlaylistId);
        });
        Assert.That(browseId, Is.Not.Null);

        // Output
        logger.LogInformation("\nPlaylist Browse Id: {browseId} ", browseId);
    }


    /// <summary>
    /// Get song information
    /// </summary>
    [Test]
    public void SongVideo()
    {
        SongVideoInfo? song = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            song = await client.GetSongVideoInfoAsync(TestData.SongVideoId);
        });
        Assert.That(song, Is.Not.Null);

        // Output
        string readableResults = JsonConvert.SerializeObject(song, Formatting.Indented);
        logger.LogInformation("\nSong Info:\n{readableResults}", readableResults);
    }

    /// <summary>
    /// Get album information
    /// </summary>
    [Test]
    public void Album()
    {
        AlbumInfo? album = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            album = await client.GetAlbumInfoAsync(TestData.AlbumBrowseId);
        });
        Assert.That(album, Is.Not.Null);

        // Output
        string readableResults = JsonConvert.SerializeObject(album, Formatting.Indented);
        logger.LogInformation("\nAlbum Info:\n{readableResults}", readableResults);
    }

    /// <summary>
    /// Get community playlist information
    /// </summary>
    [Test]
    public void CommunityPlaylist()
    {
        CommunityPlaylistInfo? communityPlaylist = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            communityPlaylist = await client.GetCommunityPlaylistInfoAsync(TestData.PlaylistBrowseId);
        });
        Assert.That(communityPlaylist, Is.Not.Null);

        // Output
        string readableResults = JsonConvert.SerializeObject(communityPlaylist, Formatting.Indented);
        logger.LogInformation("\nPlaylist Info:\n{readableResults}", readableResults);
    }
    /// <summary>
    /// Get community playlist information
    /// </summary>
    [Test]
    public void CommunityPlaylistSongs()
    {
        IReadOnlyList<CommunityPlaylistSong>? bufferedSongs = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            PaginatedAsyncEnumerable<CommunityPlaylistSong> songs = client.GetCommunityPlaylistSongsAsync(TestData.PlaylistBrowseId);
            bufferedSongs = await songs.FetchItemsAsync(TestData.FetchOffset, TestData.FetchLimit);
        });
        Assert.That(bufferedSongs, Is.Not.Null);
        Assert.That(bufferedSongs, Is.Not.Empty);

        // Output
        string readableResults = JsonConvert.SerializeObject(bufferedSongs, Formatting.Indented);
        logger.LogInformation("\nPlaylist Songs ({resultsCount}):\n{readableResults}", bufferedSongs.Count, readableResults);
    }

    /// <summary>
    /// Get artist information
    /// </summary>
    [Test]
    public void Artist()
    {
        ArtistInfo? artist = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            artist = await client.GetArtistInfoAsync(TestData.ArtistBrowseId);
        });
        Assert.That(artist, Is.Not.Null);

        // Output
        string readableResults = JsonConvert.SerializeObject(artist, Formatting.Indented);
        logger.LogInformation("\nArtist Info:\n{readableResults}", readableResults);
    }

    /// <summary>
    /// Get artist information and verify discography endpoints are populated
    /// </summary>
    [Test]
    public void Artist_OverviewWithDiscographyEndpoints()
    {
        ArtistInfo? artist = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            artist = await client.GetArtistInfoAsync(TestData.ArtistBrowseId);
        });
        Assert.That(artist, Is.Not.Null);
        Assert.That(artist!.Albums, Is.Not.Empty);
        Assert.That(artist.AlbumsEndpoint, Is.Not.Null, "Expected AlbumsEndpoint to be populated from the More button");
        Assert.That(artist.AlbumsEndpoint!.BrowseId, Is.Not.Null.And.Not.Empty);
        Assert.That(artist.SinglesEndpoint, Is.Not.Null, "Expected SinglesEndpoint to be populated from the More button");
        Assert.That(artist.SinglesEndpoint!.BrowseId, Is.Not.Null.And.Not.Empty);

        logger.LogInformation("\nArtist preview albums count: {count}", artist.Albums.Length);
        logger.LogInformation("AlbumsEndpoint: browseId={browseId}, params={params}", artist.AlbumsEndpoint.BrowseId, artist.AlbumsEndpoint.Params);
    }

    /// <summary>
    /// Get full artist albums discography using GetArtistAlbumsAsync
    /// </summary>
    [Test]
    public void Artist_GetArtistAlbumsAsync_ReturnsFullDiscography()
    {
        ArtistInfo? artist = null;
        ArtistAlbum[]? albums = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            artist = await client.GetArtistInfoAsync(TestData.ArtistBrowseId);
            albums = await client.GetArtistAlbumsAsync(artist!);
        });

        Assert.That(albums, Is.Not.Null);
        Assert.That(albums!.Length, Is.GreaterThan(TestData.ExpectedMinDiscographyCount));
        Assert.That(albums.Any(album => album.Name.Contains(TestData.ExpectedAlbumName)), Is.True, $"Expected discography to include '{TestData.ExpectedAlbumName}'");

        logger.LogInformation("\nArtist full albums count: {count}", albums.Length);
    }

    /// <summary>
    /// Get full artist singles and EPs discography using GetArtistSinglesAsync
    /// </summary>
    [Test]
    public void Artist_GetArtistSinglesAsync_ReturnsFullSingles()
    {
        ArtistInfo? artist = null;
        ArtistAlbum[]? singles = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            artist = await client.GetArtistInfoAsync(TestData.ArtistBrowseId);
            singles = await client.GetArtistSinglesAsync(artist!);
        });

        Assert.That(singles, Is.Not.Null);
        Assert.That(singles!.Length, Is.GreaterThan(TestData.ExpectedMinDiscographyCount));

        logger.LogInformation("\nArtist full singles count: {count}", singles.Length);
    }
}