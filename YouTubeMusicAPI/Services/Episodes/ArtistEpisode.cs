using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Episodes;

/// <summary>
/// Represents an artist podcast episode on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="ArtistEpisode"/> class.
/// </remarks>
/// <param name="name">The name of this artist podcast episode.</param>
/// <param name="id">The ID of this artist podcast episode.</param>
/// <param name="thumbnails">The thumbnails of this artist podcast episode.</param>
/// <param name="description">The description of this artist podcast episode.</param>
/// <param name="browseId">The browse ID of this artist podcast episode.</param>
/// <param name="releasedAt">The release date of this artist podcast episode.</param>
/// <param name="duration">The releduration of this artist podcast episode.</param>
/// <param name="podcast">The podcast to which this artist podcast episode belongs.</param>
public class ArtistEpisode(
    string name,
    string id,
    string browseId,
    Thumbnail[] thumbnails,
    string description,
    DateTime releasedAt,
    TimeSpan duration,
    YouTubeMusicEntity podcast) : YouTubeMusicEntity(name, id, browseId)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="ArtistEpisode"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicMultiRowListItemRenderer".</param>
    internal static ArtistEpisode Parse(
        JElement element)
    {
        string name = element
            .SelectRunTextAt("title", 0)
            .OrThrow();

        string id = element
            .Get("overlay")
            .SelectOverlayPlaylistId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        string description = element
            .SelectRunTextAt("description", 0)
            .Or("");

        DateTime releasedAt = element
            .SelectRunTextAt("subtitle", 0)
            .ToDateTime()
            .Or(new(1970, 1, 1));

        TimeSpan duration = element
            .Get("playbackProgress")
            .SelectRunTextAt("durationText", 1)
            .ToTimeSpan()
            .Or(TimeSpan.Zero);

        YouTubeMusicEntity podcast = element
            .SelectMenu()
            .SelectPodcastUnknown();

        return new(name, id, "browseId", thumbnails, description, releasedAt, duration, podcast);
    }


    /// <summary>
    /// The ID of this artist podcast episode.
    /// </summary>
    public override string Id { get; } = id;

    /// <summary>
    /// The browse ID of this artist podcast episode.
    /// </summary>
    public override string BrowseId { get; } = browseId;


    /// <summary>
    /// The thumbnails of this artist podcast episode.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The description of this artist podcast episode.
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// The release date of this artist podcast episode.
    /// </summary>
    public DateTime ReleasedAt { get; } = releasedAt;

    /// <summary>
    /// The duration of this artist podcast episode.
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// The podcast to which this artist podcast episode belongs.
    /// </summary>
    public YouTubeMusicEntity Podcast { get; } = podcast;
}