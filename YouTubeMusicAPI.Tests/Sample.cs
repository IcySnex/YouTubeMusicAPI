using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Models.Info;
using YouTubeMusicAPI.Models.Search;
using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Tests;

/// <summary>
/// Contains usage samples
/// </summary>
internal class Sample
{
    public static async Task SearchAsync(
        string query)
    {
        YouTubeMusicClient client = new();

        PaginatedAsyncEnumerable<SearchResult> searchResults = client.SearchAsync(query);
        IReadOnlyList<SearchResult> bufferedSearchResults = await searchResults.FetchItemsAsync(0, 20);

        foreach (SearchResult item in bufferedSearchResults)
            Console.WriteLine($"{item.Category}: {item.Name} - {item.Id}");
    }

    public static async Task SearchSongsAsync(
        string query)
    {
        YouTubeMusicClient client = new();

        PaginatedAsyncEnumerable<SearchResult> searchResults = client.SearchAsync(query, SearchCategory.Songs);
        IReadOnlyList<SearchResult> bufferedSearchResults = await searchResults.FetchItemsAsync(0, 20);

        foreach (SongSearchResult song in bufferedSearchResults.Cast<SongSearchResult>())
            Console.WriteLine(song.Name);
    }

    public static async Task GetSongVideoInfoAsync(
        string id)
    {
        YouTubeMusicClient client = new();

        SongVideoInfo song = await client.GetSongVideoInfoAsync(id);
        Console.WriteLine(song.Name);
    }

    public static async Task GetAlbumAsync(
        string id)
    {
        YouTubeMusicClient client = new();

        string browseId = await client.GetAlbumBrowseIdAsync(id);

        AlbumInfo album = await client.GetAlbumInfoAsync(browseId);
        Console.WriteLine("Album: " + album.Name);

        foreach (AlbumSongInfo song in album.Songs)
            Console.WriteLine(song.Name);
    }

    public static async Task GetCommunityPlaylistInfoAsync(
        string id)
    {
        YouTubeMusicClient client = new();

        string browseId = client.GetCommunityPlaylistBrowseId(id);

        CommunityPlaylistInfo playlist = await client.GetCommunityPlaylistInfoAsync(browseId);
        Console.WriteLine("Playlist: " + playlist.Name);

        PaginatedAsyncEnumerable<CommunityPlaylistSong> songs = client.GetCommunityPlaylistSongsAsync(browseId);
        IReadOnlyList<CommunityPlaylistSong> bufferedSongs = await songs.FetchItemsAsync(0, 100);

        foreach (CommunityPlaylistSong song in bufferedSongs)
            Console.WriteLine(song.Name);
    }

    public static async Task GetArtistInfoAsync(
        string id)
    {
        YouTubeMusicClient client = new();

        ArtistInfo artist = await client.GetArtistInfoAsync(id);
        Console.WriteLine("Artist: " + artist.Name);

        foreach (ArtistSong song in artist.Songs)
            Console.WriteLine(song.Name);
    }
}