# <img src="https://github.com/IcySnex/YouTubeMusicAPI/blob/main/icon.png" alt="YouTube Music Icon" width="40" height="40"> YouTubeMusicAPI

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

IEnumerable<IShelfItem> searchResults = await client.SearchAsync<IShelfItem>(query, limit);
foreach (IShelfItem item in searchResults)
  Console.WriteLine($"{item.Kind}: {item.Name} - {item.Id}");
```
```cs
// Search for shelves
YouTubeMusicClient client = new();

IEnumerable<Shelf> shelves = await client.SearchAsync(query, null, null);
foreach (Shelf shelf in shelves)
{
  Console.WriteLine($"{shelf.Kind}: Next Continuation Token-{shelf.NextContinuationToken}");

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
  Console.WriteLine($"{song.Name}, {string.Join(", ", song.Artists.Select(artist => artist.Name))} - {song.Album.Name}");
```
```cs
// Search for the songs shelves
YouTubeMusicClient client = new();

IEnumerable<Shelf> shelves = await client.SearchAsync(query, null, ShelfKind.Songs);
foreach (Shelf shelf in shelves)
{
  Console.WriteLine($"{shelf.Kind}: Next Continuation Token-{shelf.NextContinuationToken}");

  foreach (IShelfItem item in shelf.Items)
  {
    Song song = (Song)item;
    Console.WriteLine($"{song.Name}, {string.Join(", ", song.Artists.Select(artist => artist.Name))} - {song.Album.Name}");
  }
}
```
‎
#### **Get information about a song/video, album, community playlist & artist**
```cs
// Song/Video
YouTubeMusicClient client = new();

SongVideoInfo info = await client.GetSongVideoInfoAsync(id);
Console.WriteLine($"{info.Name}, {string.Join(", ", info.Artists.Select(artist => artist.Name))} - {info.Description}");
```
```cs
// Album
YouTubeMusicClient client = new();

string browseId = await client.GetAlbumBrowseIdAsync(id);

AlbumInfo info = await client.GetAlbumInfoAsync(browseId);
foreach (AlbumSongInfo song in info.Songs)
  Console.WriteLine($"{song.Name}, {song.SongNumber}/{info.SongCount}");
```
```cs
// Community Playlist
YouTubeMusicClient client = new();

string browseId = client.GetCommunityPlaylistBrowseId(id);

CommunityPlaylistInfo info = await client.GetCommunityPlaylistInfoAsync(browseId);
foreach (CommunityPlaylistSongInfo song in info.Songs)
  Console.WriteLine($"{song.Name}, {string.Join(", ", song.Artists.Select(artist => artist.Name))} - {song.Album?.Name}");
```
```cs
// Artist
YouTubeMusicClient client = new();

ArtistInfo info = await client.GetArtistInfoAsync(id);
foreach (ArtistSongInfo song in info.Songs)
  Console.WriteLine($"{song.Name}, {string.Join(", ", song.Artists.Select(artist => artist.Name))} - {song.Album?.Name}");
```

## Shelves
In the usage samples you may have noticed there are two ways to search for something via this Wrapper - directly for the items or the "shelf".\
But what is a shelf? The internal YouTube Music API returns so called "shelves" when searching. A shelf is like a container for search results like songs, videos etc.\
Each shelf has a kind which says what items it contains for example "Songs". A Shelf is also contains the 'Next Continuation Token' which is useful if you want to dynamically load more search results (e.g. when scrolling on a page).

If all of this sounds kind of useless to you, dont worry! You can just use the search method with a generic type `SearchAsync<IShelfItem>()` instead, this also contains a property to limit the search result so you dont have to manually implement that with the 'Next Continuation Token'.\
This handles all the shelf stuff in the background and returns a list of all your search results.

---

## License
This project is licensed under the GNU General Public License v3.0. See the [LICENSE](/LICENSE) file for details.

## Contact
For questions, suggestions or problems, please [open an issue](https://github.com/IcySnex/YouTubeMusicAPI/issues) with a detailed description.
