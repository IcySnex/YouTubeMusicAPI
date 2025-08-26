using YouTubeMusicAPI.Common;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Services.Search;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Episodes;

/// <summary>
/// Represents a podcast episode search result on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="EpisodeSearchResult"/> class.
/// </remarks>
/// <param name="name">The name of this podcast episode.</param>
/// <param name="id">The ID of this podcast episode.</param>
/// <param name="thumbnails">The thumbnails of this podcast episode.</param>
/// <param name="browseId">The browse ID of this podcast episode.</param>
/// <param name="releasedAt">The release data of this podcast episode.</param>
/// <param name="podcast">The podcast to which this episode belongs.</param>
public class EpisodeSearchResult(
    string name,
    string id,
    string browseId,
    Thumbnail[] thumbnails,
    DateTime releasedAt,
    YouTubeMusicEntity podcast) : SearchResult(name, id, browseId, thumbnails)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="EpisodeSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static EpisodeSearchResult Parse(
        JElement element)
    {
        JElement flexColumns = element
            .Get("flexColumns");

        JElement titleRun = flexColumns
            .GetAt(0)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .Get("text")
            .Get("runs")
            .GetAt(0);

        JElement descriptionRuns = flexColumns
            .GetAt(1)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .Get("text")
            .Get("runs");

        int descriptionStartIndex = descriptionRuns
            .GetAt(0)
            .Get("text")
            .AsString()
            .OrThrow()
            .If("Episode", 2, 0);

        bool containsDuration = descriptionRuns
            .GetAt(0)
            .SelectNavigationBrowseId()
            .IsNotNull()
            .And(descriptionRuns
                .GetAt(2)
                .Get("text")
                .AsString()
                .IsNotNull(out string? duration)
                .And(duration!.Contains("hr") || duration.Contains("min") || duration.Contains("sec")));


        string name = titleRun
            .Get("text")
            .AsString()
            .OrThrow();

        string id = element
            .Get("overlay")
            .SelectOverlayVideoId()
            .OrThrow();

        string browseId = titleRun
            .SelectNavigationBrowseId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        DateTime releasedAt = containsDuration
            .If(true,
                flexColumns
                    .GetAt(2)
                    .Get("musicResponsiveListItemFlexColumnRenderer")
                    .SelectRunTextAt("text", 0),
                descriptionRuns
                    .GetAt(descriptionStartIndex + (containsDuration ? 4 : 0))
                    .Get("text")
                    .AsString())
            .ToDateTime()
            .Or(new(1970, 1, 1));

        YouTubeMusicEntity podcast = descriptionRuns
            .GetAt(descriptionStartIndex + (containsDuration ? 0 : 2))
            .SelectPodcast();

        return new(name, id, browseId, thumbnails, releasedAt, podcast);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="EpisodeSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicCardShelfRenderer".</param>
    internal static EpisodeSearchResult ParseTopResult(
        JElement element)
    {
        JElement descriptionRuns = element
            .Get("subtitle")
            .Get("runs");


        string name = element
            .Get("title")
            .Get("runs")
            .GetAt(0)
            .Get("text")
            .AsString()
            .OrThrow();

        string id = element
            .Get("thumbnailOverlay")
            .SelectOverlayVideoId()
            .OrThrow();

        string browseId = element
            .SelectTapBrowseId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        DateTime releasedAt = descriptionRuns
            .GetAt(2)
            .Get("text")
            .AsString()
            .ToDateTime()
            .Or(new(1970, 1, 1));

        YouTubeMusicEntity podcast = descriptionRuns
            .GetAt(4)
            .SelectPodcast();

        return new(name, id, browseId, thumbnails, releasedAt, podcast);
    }


    /// <summary>
    /// The browse ID of this podcast episode.
    /// </summary>
    public override string BrowseId { get; } = browseId;


    /// <summary>
    /// The release data of this podcast episode.
    /// </summary>
    public DateTime ReleasedAt { get; } = releasedAt;

    /// <summary>
    /// The podcast to which this episode belongs.
    /// </summary>
    public YouTubeMusicEntity Podcast { get; } = podcast;
}