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
        client = new(logger, TestData.GeographicalLocation, TestData.VisitorData, TestData.PoToken, TestData.Cookies);
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
}