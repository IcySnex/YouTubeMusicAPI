using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Episodes;

/// <summary>
/// Represents a profile podcast episode on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="ProfileEpisode"/> class.
/// </remarks>
/// <param name="name">The name of this profile podcast episode.</param>
/// <param name="id">The ID of this profile podcast episode.</param>
/// <param name="thumbnails">The thumbnails of this profile podcast episode.</param>
/// <param name="description">The description of this profile podcast episode.</param>
/// <param name="browseId">The browse ID of this profile podcast episode.</param>
/// <param name="releasedAt">The release date of this profile podcast episode.</param>
/// <param name="duration">The releduration of this profile podcast episode.</param>
/// <param name="podcast">The podcast to which this profile podcast episode belongs.</param>
public class ProfileEpisode(
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
    /// Parses a <see cref="JElement"/> into a <see cref="ProfileEpisode"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicMultiRowListItemRenderer".</param>
    internal static ProfileEpisode Parse(
        JElement element)
    {
        JElement descriptionRuns = element
            .Get("subtitle")
            .Get("runs");


        string name = element
            .SelectRunTextAt("title", 0)
            .OrThrow();

        string id = element
            .Get("overlay")
            .SelectOverlayVideoId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        string description = element
            .SelectRunTextAt("description", 0)
            .Or("");

        DateTime releasedAt = descriptionRuns
            .GetAt(0)
            .Get("text")
            .AsString()
            .ToDateTime()
            .Or(new(1970, 1, 1));

        TimeSpan duration = descriptionRuns
            .GetAt(2)
            .Get("text")
            .AsString()
            .ToTimeSpan()
            .Or(TimeSpan.Zero);

        YouTubeMusicEntity podcast = element
            .SelectMenu()
            .SelectPodcastUnknown();

        return new(name, id, "browseId", thumbnails, description, releasedAt, duration, podcast);
    }


    /// <summary>
    /// The ID of this profile podcast episode.
    /// </summary>
    public override string Id { get; } = id;

    /// <summary>
    /// The browse ID of this profile podcast episode.
    /// </summary>
    public override string BrowseId { get; } = browseId;


    /// <summary>
    /// The thumbnails of this profile podcast episode.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The description of this profile podcast episode.
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// The release date of this profile podcast episode.
    /// </summary>
    public DateTime ReleasedAt { get; } = releasedAt;

    /// <summary>
    /// The duration of this profile podcast episode.
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// The podcast to which this profile podcast episode belongs.
    /// </summary>
    public YouTubeMusicEntity Podcast { get; } = podcast;
}