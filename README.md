# YouTubeMusicAPI

YouTubeMusicAPI is a simple and efficient wrapper for the internal YouTube Music Web API for dotnet & C# developers.
This library allows you to easily search for songs, videos, albums, community playlists, artists, podcasts, podcast episodes & profiles on YouTube Music.

---

## Installation
To install YouTubeMusicAPI, add the following package to your project:
```
dotnet add package YouTubeMusicAPI
```

---

## Usage
#### **Search**
```cs
// Search for items directly
YouTubeMusicClient client = new();

IEnumerable<IShelfItem> searchResults = await client.SearchAsync<IShelfItem>(query);
foreach (IShelfItem item in searchResults)
  Console.WriteLine($"{item.Kind}: {item.Name} - {item.Id}");
```
```cs
// Search for shelves
YouTubeMusicClient client = new();

IEnumerable<Shelf> shelves = await client.SearchAsync(query, null);
foreach (Shelf shelf in shelves)
{
  Console.WriteLine($"{shelf.Kind} - Query: {shelf.Query}, Params: {shelf.Params}");

  foreach (IShelfItem item in shelf.Items)
    Console.WriteLine($"{item.Kind}: {item.Name} - {item.Id}");
}
```
‎
#### **Search for specific item only (songs, videos, albums, community playlists, artists, podcasts, podcast episodes profiles)**
```cs
// Search for songs directly
YouTubeMusicClient client = new();

IEnumerable<Song> searchResults = await client.SearchAsync<Song>(query);
foreach (Song song in searchResults)
  Console.WriteLine($"{song.Name}, {string.Join(", ", song.Artists.Select(artist => artist.Name))} - {song.Album}");
```
```cs
// Search for the songs shelves
YouTubeMusicClient client = new();

IEnumerable<Shelf> shelves = await client.SearchAsync(query, ShelfKind.Songs);
foreach (Shelf shelf in shelves)
{
  Console.WriteLine($"{shelf.Kind}: Query-{shelf.Query}, Params-{shelf.Params}");

  foreach (IShelfItem item in shelf.Items)
  {
    Song song = (Song)item;
    Console.WriteLine($"{song.Name}, {string.Join(", ", song.Artists.Select(artist => artist.Name))} - {song.Album}");
  }
}
```

## Shelves
In the usage samples you may have noticed there are two ways to search for something via this Wrapper - directly for the items or the "shelf".\
But what is a shelf? The internal YouTube Music API returns so called "shelves" when searching. A shelf is like a container for search results like songs, videos etc.\
Each shelf has a kind which says what items it contains for example "Songs". A Shelf is also connected to the sent request so it contains properties like "Query" or "Params" which help identify the initial request used for the search.

If all of this sounds kind of useless to you, dont worry! You can just use the search method with a generic type `SearchAsync<IShelfItem>()` instead.\
This handles all the shelf stuff in the background and returns a list of all your search results.

---

## License
This project is licensed under the GNU General Public License v3.0. See the [LICENSE](/LICENSE) file for details.

## Contact
For questions, suggestions or problems, please [open an issue](https://github.com/IcySnex/YouTubeMusicAPI/issues) with a detailed description.
