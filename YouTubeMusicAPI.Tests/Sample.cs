using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models;
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
        string query,
        int limit)
    {
        YouTubeMusicClient client = new();

        IEnumerable<IYouTubeMusicItem> searchResults = await client.SearchAsync<IYouTubeMusicItem>(query, limit);
        foreach (IYouTubeMusicItem item in searchResults)
            Console.WriteLine($"{item.Kind}: {item.Name} - {item.Id}");
    }

    public static async Task SearchShelvesAsync(
        string query)
    {
        YouTubeMusicClient client = new();

        IEnumerable<Shelf> shelves = await client.SearchAsync(query, null, null);
        foreach (Shelf shelf in shelves)
        {
            Console.WriteLine($"{shelf.Kind}: Next Continuation Token-{shelf.NextContinuationToken}");

            foreach (IYouTubeMusicItem item in shelf.Items)
                Console.WriteLine($"{item.Kind}: {item.Name} - {item.Id}");
        }
    }


    public static async Task SearchSongsAsync(
        string query)
    {
        YouTubeMusicClient client = new();

        IEnumerable<SongSearchResult> searchResults = await client.SearchAsync<SongSearchResult>(query);
        foreach (SongSearchResult song in searchResults)
            Console.WriteLine($"{song.Name}, {string.Join(", ", song.Artists.Select(artist => artist.Name))} - {song.Album.Name}");
    }

    public static async Task SearchSongsShelfAsync(
        string query)
    {
        YouTubeMusicClient client = new();

        IEnumerable<Shelf> shelves = await client.SearchAsync(query, null, YouTubeMusicItemKind.Songs);
        foreach (Shelf shelf in shelves)
        {
            Console.WriteLine($"{shelf.Kind}: Next Continuation Token-{shelf.NextContinuationToken}");

            foreach (IYouTubeMusicItem item in shelf.Items)
            {
                SongSearchResult song = (SongSearchResult)item;
                Console.WriteLine($"{song.Name}, {string.Join(", ", song.Artists.Select(artist => artist.Name))} - {song.Album.Name}");
            }
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