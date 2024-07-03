namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Contains information about a YouTube Music artist
/// </summary>
/// <param name="name">The name of the artist</param>
/// <param name="id">The id of the artist</param>
/// <param name="description">The description of the artist</param>
/// <param name="subscriberCount">The subscriber count of the artist</param>
/// <param name="viewsInfo">The views info of the artist</param>
/// <param name="thumbnails">The thumbnails of the artist</param>
/// <param name="allSongsPlaylistId">The id of the playlist containing all songs of the artist</param>
/// <param name="songs">The info of all songs in artist</param>
/// <param name="albums">The info of all albums in artist</param>
/// <param name="videos">The info of all videos in artist</param>
public class ArtistInfo(
    string name,
    string id,
    string? description,
    string? subscriberCount,
    string? viewsInfo,
    Thumbnail[] thumbnails,
    string? allSongsPlaylistId,
    ArtistSongInfo[] songs,
    ArtistAlbumInfo[] albums,
    ArtistVideoInfo[] videos)
{
    /// <summary>
    /// The name of the artist
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of the artist
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The description of the artist
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// The subscriber count of the artist
    /// </summary>
    public string? SubscriberCount { get; } = subscriberCount;
    
    /// <summary>
    /// The views info of the artist
    /// </summary>
    public string? ViewsInfo { get; } = viewsInfo;

    /// <summary>
    /// The thumbnails of the artist
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The id of the playlist containing all songs of the artist
    /// </summary>
    public string? AllSongsPlaylistId { get; } = allSongsPlaylistId;

    /// <summary>
    /// The info of all songs of the artist
    /// </summary>
    public ArtistSongInfo[] Songs { get; } = songs;

    /// <summary>
    /// The info of all albums of the artist
    /// </summary>
    public ArtistAlbumInfo[] Albums { get; } = albums;

    /// <summary>
    /// The info of all videos of the artist
    /// </summary>
    public ArtistVideoInfo[] Videos { get; } = videos;

}