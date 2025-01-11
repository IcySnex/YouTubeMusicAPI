# <img src="https://github.com/IcySnex/YouTubeMusicAPI/blob/main/icon.png" alt="YouTube Music Icon" width="40" height="40"> YouTubeMusicAPI

YouTubeMusicAPI is a simple and efficient C# wrapper for the YouTube Music Web API, enabling easy search and retrieval of songs, videos, albums, artists, podcasts, and more. It also provides streaming data and access to an authenticated user's library, all with minimal effort.

---

## Getting Started
To install YouTubeMusicAPI, add the following package to your project:
```
dotnet add package YouTubeMusicAPI
```
To start using YouTube Music in your project, just create a new `YouTubeMusicClient`.
```cs
YouTubeMusicClient client = new(logger, geographicalLocation, cookies);
```

---

## Usage
### Search
Using this wrapper, you can search in two ways: directly for items or via "shelves." A shelf is a container for search results like songs or videos. Each shelf has a type (e.g., "Songs") and a "Next Continuation Token" to load more results dynamically (e.g., for infinite scrolling on a page).

If this seems complicated, no worries! You can simply use the search method with a generic type `SearchAsync<IYouTubeMusicItem>()` instead, which also contains a property to limit the search result so you dont have to deal with any of the shelf or token stuff.
```cs
// Search for songs directly
IEnumerable<SongSearchResult> searchResults = await client.SearchAsync<SongSearchResult>(query);

foreach (SongSearchResult song in searchResults)
  Console.WriteLine($"{song.Name}, {string.Join(", ", song.Artists.Select(artist => artist.Name))} - {song.Album.Name}");
```
```cs
// Search for the songs using shelves
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
```
‎
```cs
// Search for all kinds directly
IEnumerable<IYouTubeMusicItem> searchResults = await client.SearchAsync<IYouTubeMusicItem>(query, limit);

foreach (IYouTubeMusicItem item in searchResults)
  Console.WriteLine($"{item.Kind}: {item.Name} - {item.Id}");
```
```cs
// Search for shelves including all kinds
IEnumerable<Shelf> shelves = await client.SearchAsync(query, null, null);

foreach (Shelf shelf in shelves)
{
  Console.WriteLine($"{shelf.Kind}: Next Continuation Token-{shelf.NextContinuationToken}");

  foreach (IYouTubeMusicItem item in shelf.Items)
    Console.WriteLine($"{item.Kind}: {item.Name} - {item.Id}");
}
```
‎
### Getting more information
```cs
// Song/Video
SongVideoInfo info = await client.GetSongVideoInfoAsync(id);

Console.WriteLine($"{info.Name}, {string.Join(", ", info.Artists.Select(artist => artist.Name))} - {info.Description}");
```
```cs
// Album
string browseId = await client.GetAlbumBrowseIdAsync(id);
AlbumInfo info = await client.GetAlbumInfoAsync(browseId);

foreach (AlbumSongInfo song in info.Songs)
  Console.WriteLine($"{song.Name}, {song.SongNumber}/{info.SongCount}");
```
```cs
// Community Playlist
string browseId = client.GetCommunityPlaylistBrowseId(id);
CommunityPlaylistInfo info = await client.GetCommunityPlaylistInfoAsync(browseId);

foreach (CommunityPlaylistSongInfo song in info.Songs)
  Console.WriteLine($"{song.Name}, {string.Join(", ", song.Artists.Select(artist => artist.Name))} - {song.Album?.Name}");
```
```cs
// Artist
ArtistInfo info = await client.GetArtistInfoAsync(id);

foreach (ArtistSongInfo song in info.Songs)
  Console.WriteLine($"{song.Name}, {string.Join(", ", song.Artists.Select(artist => artist.Name))} - {song.Album?.Name}");
```
‎
### Streaming & Downloading
```cs
// Getting streaming data of a song
StreamingData streamingData = await client.GetStreamingDataAsync(id);

Console.WriteLine($"Expires in: {streamingData.ExpiresIn}");
Console.WriteLine($"Hls Manifest URL: {streamingData.HlsManifestUrl}");

foreach(MediaStreamInfo streamInfo in streamingData.StreamInfo)
{
  if (streamInfo is VideoStreamInfo videoStreamInfo)
    Console.WriteLine($"Video ({videoStreamInfo.Quality}): {videoStreamInfo.Url}");
  else if (streamInfo is AudioStreamInfo audioStreamInfo)
    Console.WriteLine($"Audio ({audioStreamInfo.SampleRate}): {audioStreamInfo.Url}");
}
```
```cs
// Download highest quality audio stream
StreamingData streamingData = await client.GetStreamingDataAsync(id);

MediaStreamInfo highestAudioStreamInfo = streamingData.StreamInfo
  .OfType<AudioStreamInfo>()
  .OrderByDescending(info => info.Bitrate)
  .First();
Stream stream = await highestAudioStreamInfo.GetStreamAsync();

using FileStream fileStream = new("audio.m4a", FileMode.Create, FileAccess.Write);
await stream.CopyToAsync(fileStream);
```
‎
### Access to a user's library
In order to access a user's library you first need to authenticate the user by using pre-authenticated cookies.

To obtain the necessary cookies, you'll need to present the official [YouTube Music login page](https://accounts.google.com/ServiceLogin?continue=https%3A%2F%2Fmusic.youtube.com) to your users. You could use an embedded browser like [WebView2](https://learn.microsoft.com/en-us/microsoft-edge/webview2/) in your application and then extract the cookies for the site ".youtube.com".
```cs
// Get saved/created community playlists
IEnumerable<LibraryCommunityPlaylist> communityPlaylists = await client.GetLibraryCommunityPlaylistsAsync();

foreach (LibraryCommunityPlaylist playlist in communityPlaylists)
  Console.WriteLine($"{playlist.Name}, {playlist.SongCount} songs");
```
```cs
// Get saved songs
IEnumerable<LibrarySong> songs = await client.GetLibrarySongsAsync();

foreach (LibrarySong song in songs)
  Console.WriteLine($"{song.Name}, {string.Join(", ", song.Artists.Select(artist => artist.Name))} - {song.Album.Name}");
```
```cs
// Get saved albums
IEnumerable<LibraryAlbum> albums = await client.GetLibraryAlbumsAsync();

foreach (LibraryAlbum album in albums)
  Console.WriteLine($"{album.Name}, {album.ReleaseYear}");
```
```cs
// Get artists with saved songs
IEnumerable<LibraryArtist> artists = await client.GetLibraryArtistsAsync();

foreach (LibraryArtist artist in artists)
  Console.WriteLine($"{artist.Name}, {artist.SongCount} saved songs");
```
```cs
// Get subscribed artists
IEnumerable<LibrarySubscription> subscriptions = await client.GetLibrarySubscriptionsAsync();

foreach (LibrarySubscription subscription in subscriptions)
  Console.WriteLine($"{subscription.Name}, {subscription.SubscribersInfo}");
```
```cs
// Get saved podcasts
IEnumerable<LibraryPodcast> podcasts = await client.GetLibraryPodcastsAsync();

foreach (LibraryPodcast podcast in podcasts)
  Console.WriteLine($"{podcast.Name}, {podcast.Host.Name}");
```

---

## License
This project is licensed under the GNU General Public License v3.0. See the [LICENSE](/LICENSE) file for details.

---

## Contact
For questions, suggestions or problems, please [open an issue](https://github.com/IcySnex/YouTubeMusicAPI/issues) with a detailed description.
