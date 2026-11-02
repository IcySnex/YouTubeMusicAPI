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
/// <param name="albumsEndpoint">The endpoint to browse the full albums discography, or <see langword="null"/> if the artist has 10 or fewer albums and no "More" button was present (meaning <paramref name="albums"/> is already complete)</param>
/// <param name="singlesAndEpsEndpoint">The endpoint to browse the full singles and EPs discography, or <see langword="null"/> if the artist has 10 or fewer singles/EPs and no "More" button was present (meaning <paramref name="singlesAndEps"/> is already complete)</param>
/// <param name="singlesAndEps">The preview carousel singles and EPs displayed on the artist overview page</param>
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
    ArtistsRelated[] related,
    BrowseEndpoint? albumsEndpoint = null,
    BrowseEndpoint? singlesAndEpsEndpoint = null,
    ArtistAlbum[]? singlesAndEps = null) : EntityInfo(name, id, thumbnails)
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
    /// The preview carousel albums displayed on the artist overview page (up to 10 releases).
    /// </summary>
    /// <remarks>
    /// If <see cref="AlbumsEndpoint"/> is <see langword="null"/>, no "More" button was present on YouTube Music
    /// (e.g. the artist has 10 or fewer albums), meaning this collection already contains all albums by the artist.
    /// Otherwise, call <c>YouTubeMusicClient.GetArtistAlbumsAsync(artist)</c> to fetch the complete albums discography.
    /// </remarks>
    public ArtistAlbum[] Albums { get; } = albums;

    /// <summary>
    /// The preview carousel singles and EPs displayed on the artist overview page (up to 10 releases).
    /// </summary>
    /// <remarks>
    /// If <see cref="SinglesAndEpsEndpoint"/> is <see langword="null"/>, no "More" button was present on YouTube Music
    /// (e.g. the artist has 10 or fewer singles/EPs), meaning this collection already contains all singles and EPs by the artist.
    /// Otherwise, call <c>YouTubeMusicClient.GetArtistSinglesAndEpsAsync(artist)</c> to fetch the complete singles and EPs discography.
    /// </remarks>
    public ArtistAlbum[] SinglesAndEps { get; } = singlesAndEps ?? [];

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
    /// The endpoint to browse the full albums discography
    /// </summary>
    /// <remarks>
    /// Populated from the "More" button on the artist overview page's albums carousel.
    /// If <see langword="null"/>, no "More" button was present on YouTube Music (typically when an artist has 10 or fewer albums),
    /// indicating that <see cref="Albums"/> is already exhaustive and complete without requiring a secondary network call.
    /// </remarks>
    public BrowseEndpoint? AlbumsEndpoint { get; } = albumsEndpoint;

    /// <summary>
    /// The endpoint to browse the full singles and EPs discography
    /// </summary>
    /// <remarks>
    /// Populated from the "More" button on the artist overview page's singles and EPs carousel.
    /// If <see langword="null"/>, no "More" button was present on YouTube Music (typically when an artist has 10 or fewer singles/EPs),
    /// indicating that <see cref="SinglesAndEps"/> is already exhaustive and complete without requiring a secondary network call.
    /// </remarks>
    public BrowseEndpoint? SinglesAndEpsEndpoint { get; } = singlesAndEpsEndpoint;

}
