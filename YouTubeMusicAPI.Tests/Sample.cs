using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Common;
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
        {
            Console.WriteLine($"{item.Category}: {item.Name} - {item.Id}");
        }
    }

    public static async Task SearchSongsAsync(
        string query)
    {
        YouTubeMusicClient client = new();

        PaginatedAsyncEnumerable<SearchResult> searchResults = client.SearchAsync(query, SearchCategory.Songs);
        IReadOnlyList<SearchResult> bufferedSearchResults = await searchResults.FetchItemsAsync(0, 20);

        foreach (SongSearchResult song in bufferedSearchResults.Cast<SongSearchResult>())
        {
            Console.WriteLine($"{song.Name}, {string.Join(", ", song.Artists.Select(artist => artist.Name))} - {song.Album.Name}");
        }
    }

    public static async Task GetSongVideoInfoAsync(
        string id)
    {
        YouTubeMusicClient client = new();

        SongVideoInfo info = await client.GetSongVideoInfoAsync(id);
        Console.WriteLine($"{info.Name}, {string.Join(", ", info.Artists.Select(artist => artist.Name))} - {info.Description}");
    }

    public static async Task GetAlbumInfoAsync(
        string id)
    {
        YouTubeMusicClient client = new();

        string browseId = await client.GetAlbumBrowseIdAsync(id);

        AlbumInfo info = await client.GetAlbumInfoAsync(browseId);
        foreach (AlbumSongInfo song in info.Songs)
            Console.WriteLine($"{song.Name}, {song.SongNumber}/{info.SongCount}");
    }

    public static async Task GetCommunityPlaylistInfoAsync(
        string id)
    {
        YouTubeMusicClient client = new();

        string browseId = client.GetCommunityPlaylistBrowseId(id);

        CommunityPlaylistInfo info = await client.GetCommunityPlaylistInfoAsync(browseId);
        foreach (CommunityPlaylistSongInfo song in info.Songs)
            Console.WriteLine($"{song.Name}, {string.Join(", ", song.Artists.Select(artist => artist.Name))} - {song.Album?.Name}");
    }

    public static async Task GetArtistInfoAsync(
        string id)
    {
        YouTubeMusicClient client = new();

        ArtistInfo info = await client.GetArtistInfoAsync(id);
        foreach (ArtistSongInfo song in info.Songs)
            Console.WriteLine($"{song.Name}, {string.Join(", ", song.Artists.Select(artist => artist.Name))} - {song.Album?.Name}");
    }
}