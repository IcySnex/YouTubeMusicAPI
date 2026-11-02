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
/// <param name="singlesAndEps">The info of preview singles and EPs of the artist</param>
/// <param name="videos">The info of all videos in artist</param>
/// <param name="featuredOns">The info of all artist featured on playlists</param>
/// <param name="related">The info of all related artists of the artist</param>
/// <param name="albumsEndpoint">The endpoint to browse the full list of albums, or <see langword="null"/> if the artist has 10 or fewer albums</param>
/// <param name="singlesAndEpsEndpoint">The endpoint to browse the full list of singles and EPs, or <see langword="null"/> if the artist has 10 or fewer singles/EPs</param>
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
    ArtistAlbum[] singlesAndEps,
    ArtistVideo[] videos,
    ArtistFeaturedOn[] featuredOns,
    ArtistsRelated[] related,
    BrowseEndpoint? albumsEndpoint = null,
    BrowseEndpoint? singlesAndEpsEndpoint = null) : EntityInfo(name, id, thumbnails)
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
    /// The info of preview albums of the artist
    /// </summary>
    public ArtistAlbum[] Albums { get; } = albums;

    /// <summary>
    /// The info of preview singles and EPs of the artist
    /// </summary>
    public ArtistAlbum[] SinglesAndEps { get; } = singlesAndEps;

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

    /// <summary>
    /// The endpoint to browse the full list of albums
    /// </summary>
    /// <remarks>
    /// If <see langword="null"/>, the artist has 10 or fewer albums, indicating that <see cref="Albums"/> is already completed.
    /// </remarks>
    public BrowseEndpoint? AlbumsEndpoint { get; } = albumsEndpoint;

    /// <summary>
    /// The endpoint to browse the full list of singles and EPs
    /// </summary>
    /// <remarks>
    /// If <see langword="null"/>, the artist has 10 or fewer singles/EPs, indicating that <see cref="SinglesAndEps"/> is already completed.
    /// </remarks>
    public BrowseEndpoint? SinglesAndEpsEndpoint { get; } = singlesAndEpsEndpoint;
}
