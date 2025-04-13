namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Contains information about a YouTube Music artist
/// </summary>
/// <param name="name">The name of the artist</param>
/// <param name="id">The id of the artist</param>
/// <param name="description">The description of the artist</param>
/// <param name="subscribersInfo">The subscriber info of the artist</param>
/// <param name="viewsInfo">The views info of the artist</param>
/// <param name="thumbnails">The thumbnails of the artist</param>
/// <param name="allSongsPlaylistId">The id of the playlist containing all songs of the artist</param>
/// <param name="songs">The info of all songs in artist</param>
/// <param name="albums">The info of all albums in artist</param>
/// <param name="videos">The info of all videos in artist</param>
/// <param name="featuredOns">The info of all artist featured on playlists</param>
/// <param name="related">The info of all related artists of the artist</param>
public class ArtistInfo(
    string name,
    string id,
    string? description,
    string? subscribersInfo,
    string? viewsInfo,
    Thumbnail[] thumbnails,
    string? allSongsPlaylistId,
    ArtistSong[] songs,
    ArtistAlbum[] albums,
    ArtistVideo[] videos,
    ArtistFeaturedOn[] featuredOns,
    ArtistsRelated[] related) : EntityInfo(name, id, thumbnails)
{
    /// <summary>
    /// The description of the artist
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// The subscribers info of the artist
    /// </summary>
    public string? SubscribersInfo { get; } = subscribersInfo;

    /// <summary>
    /// The views info of the artist
    /// </summary>
    public string? ViewsInfo { get; } = viewsInfo;

    /// <summary>
    /// The id of the playlist containing all songs of the artist
    /// </summary>
    public string? AllSongsPlaylistId { get; } = allSongsPlaylistId;

    /// <summary>
    /// The info of all songs of the artist
    /// </summary>
    public ArtistSong[] Songs { get; } = songs;

    /// <summary>
    /// The info of all albums of the artist
    /// </summary>
    public ArtistAlbum[] Albums { get; } = albums;

    /// <summary>
    /// The info of all videos of the artist
    /// </summary>
    public ArtistVideo[] Videos { get; } = videos;

    /// <summary>
    /// The info of all artist featured on playlists
    /// </summary>
    public ArtistFeaturedOn[] FeaturedOns { get; } = featuredOns;

    /// <summary>
    /// The info of all related artists of the artist
    /// </summary>
    public ArtistsRelated[] Related { get; } = related;

}