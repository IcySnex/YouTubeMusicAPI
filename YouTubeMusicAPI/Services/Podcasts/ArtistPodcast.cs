using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Podcasts;

/// <summary>
/// Represents an artist podcast on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="ArtistPodcast"/> class.
/// </remarks>
/// <param name="name">The name of this artist podcast.</param>
/// <param name="id">The ID of this artist podcast.</param>
/// <param name="thumbnails">The thumbnails of this artist podcast.</param>
/// <param name="browseId">The browse ID of this artist podcast.</param>
/// <param name="host">The host of this artist podcast.</param>
public class ArtistPodcast(
    string name,
    string id,
    string browseId,
    Thumbnail[] thumbnails,
    YouTubeMusicEntity host) : YouTubeMusicEntity(name, id, browseId)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="ArtistPodcast"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicTwoRowItemRenderer".</param>
    internal static ArtistPodcast Parse(
        JElement element)
    {
        string name = element
            .SelectRunTextAt("title", 0)
            .OrThrow();

        string id = element
            .Get("thumbnailOverlay")
            .SelectOverlayPlaylistId()
            .OrThrow();

        string browseId = element
            .SelectNavigationBrowseId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnailRenderer")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity host = element
            .Get("subtitle")
            .Get("runs")
            .GetAt(0)
            .SelectArtist();

        return new(name, id, browseId, thumbnails, host);
    }


    /// <summary>
    /// The ID of this artist podcast.
    /// </summary>
    public override string Id { get; } = id;

    /// <summary>
    /// The browse ID of this artist podcast.
    /// </summary>
    public override string BrowseId { get; } = browseId;


    /// <summary>
    /// The thumbnails of this artist podcast.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The host of this artist podcast.
    /// </summary>
    public YouTubeMusicEntity Host { get; } = host;
}