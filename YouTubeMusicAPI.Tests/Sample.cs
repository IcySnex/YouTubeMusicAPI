using YouTubeMusicAPI.Client;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Shelf;
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

        IEnumerable<IShelfItem> searchResults = await client.SearchAsync<IShelfItem>(query);
        foreach (IShelfItem item in searchResults)
            Console.WriteLine($"{item.Kind}: {item.Name} - {item.Id}");
    }

    public static async Task SearchShelvesAsync(
        string query)
    {
        YouTubeMusicClient client = new();

        IEnumerable<Shelf> shelves = await client.SearchAsync(query, null);
        foreach (Shelf shelf in shelves)
        {
            Console.WriteLine($"{shelf.Kind}: Query-{shelf.Query}, Params-{shelf.Params}");

            foreach (IShelfItem item in shelf.Items)
                Console.WriteLine($"{item.Kind}: {item.Name} - {item.Id}");
        }
    }


    public static async Task SearchSongsAsync(
        string query)
    {
        YouTubeMusicClient client = new();

        IEnumerable<Song> searchResults = await client.SearchAsync<Song>(query);
        foreach (Song song in searchResults)
            Console.WriteLine($"{song.Name}, {string.Join(", ", song.Artists.Select(artist => artist.Name))} - {song.Album.Name}");
    }

    public static async Task SearchSongsShelfAsync(
        string query)
    {
        YouTubeMusicClient client = new();

        IEnumerable<Shelf> shelves = await client.SearchAsync(query, ShelfKind.Songs);
        foreach (Shelf shelf in shelves)
        {
            Console.WriteLine($"{shelf.Kind} - Query: {shelf.Query}, Params: {shelf.Params}");

            foreach (IShelfItem item in shelf.Items)
            {
                Song song = (Song)item;
                Console.WriteLine($"{song.Name}, {string.Join(", ", song.Artists.Select(artist => artist.Name))} - {song.Album.Name}");
            }
        }
    }
}