using System.Text.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a podcast episode search result on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="EpisodeSearchResult"/>.
/// </remarks>
/// <param name="name">The name of this podcast episode.</param>
/// <param name="id">The ID of this podcast episode.</param>
/// <param name="thumbnails">The thumbnails of this podcast episode.</param>
/// <param name="browseId">The browse ID of this podcast episode.</param>
/// <param name="releasedAt">The release data of this podcast episode.</param>
/// <param name="podcast">The podcast to which this episode belongs.</param>
/// <param name="isRatingsAllowed">Whether ratings are allowed for this podcast episode.</param>
public class EpisodeSearchResult(
    string name,
    string id,
    Thumbnail[] thumbnails,
    string browseId,
    DateTime releasedAt,
    YouTubeMusicEntity podcast,
    bool isRatingsAllowed) : SearchResult(name, id, browseId, thumbnails)
{
    /// <summary>
    /// Parses the JSON element into a <see cref="EpisodeSearchResult"/>.
    /// </summary>
    /// <param name="element">The JSON element containing "musicResponsiveListItemRenderer".</param>
    internal static EpisodeSearchResult Parse(
        JsonElement element)
    {
        JsonElement item = element
            .GetProperty("musicResponsiveListItemRenderer");

        JsonElement flexColumns = item
            .GetProperty("flexColumns");

        JsonElement titleRun = flexColumns
            .GetElementAt(0)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs")
            .GetElementAt(0);

        JsonElement descriptionRuns = flexColumns
            .GetElementAt(1)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs");

        int descriptionStartIndex = descriptionRuns
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow()
            .If("Episode", 2, 0);


        string name = titleRun
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = item
            .GetProperty("overlay")
            .SelectOverlayNavigationVideoId();

        string browseId = titleRun
            .SelectNavigationBrowseId();

        Thumbnail[] thumbnails = item
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        DateTime releasedAt = descriptionRuns
            .GetElementAt(descriptionStartIndex)
            .GetProperty("text")
            .GetString()
            .ToDateTime()
            .Or(new(1970, 1, 1));

        YouTubeMusicEntity podcast = descriptionRuns
            .GetElementAt(descriptionStartIndex + 2)
            .SelectPodcast();

        bool isRatingsAllowed = item
            .GetProperty("menu")
            .GetProperty("menuRenderer")
            .GetProperty("topLevelButtons")
            .GetElementAt(0)
            .GetProperty("likeButtonRenderer")
            .GetProperty("likesAllowed")
            .GetBoolean();

        return new(name, id, thumbnails, browseId, releasedAt, podcast, isRatingsAllowed);
    }

    /// <summary>
    /// Parses the JSON item into an <see cref="EpisodeSearchResult"/>.
    /// </summary>
    /// <param name="item">The JSON item "musicCardShelfRenderer".</param>
    internal static EpisodeSearchResult ParseTopResult(
        JsonElement item)
    {
        JsonElement descriptionRuns = item
            .GetProperty("subtitle")
            .GetProperty("runs");


        string name = item
            .GetProperty("title")
            .GetProperty("runs")
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = item
            .GetProperty("thumbnailOverlay")
            .SelectOverlayNavigationVideoId();

        Thumbnail[] thumbnails = item
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        string browseId = item
            .SelectTapBrowseId();

        DateTime releasedAt = descriptionRuns
            .GetElementAt(2)
            .GetProperty("text")
            .GetString()
            .ToDateTime()
            .Or(new(1970, 1, 1));

        YouTubeMusicEntity podcast = descriptionRuns
            .GetElementAt(4)
            .SelectPodcast();

        bool isRatingsAllowed = true;

        return new(name, id, thumbnails, browseId, releasedAt, podcast, isRatingsAllowed);
    }


    /// <summary>
    /// The release data of this podcast episode.
    /// </summary>
    public DateTime ReleasedAt { get; } = releasedAt;

    /// <summary>
    /// The podcast to which this episode belongs.
    /// </summary>
    public YouTubeMusicEntity Podcast { get; } = podcast;

    /// <summary>
    /// Whether ratings are allowed for this podcast episode.
    /// </summary>
    public bool IsRatingsAllowed { get; } = isRatingsAllowed;
}