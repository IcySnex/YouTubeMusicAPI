using System.Text.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a podcast search result on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="PodcastSearchResult"/>.
/// </remarks>
/// <param name="name">The name of this podcast.</param>
/// <param name="id">The ID of this podcast.</param>
/// <param name="thumbnails">The thumbnails of this podcast.</param>
/// <param name="browseId">The browse ID of this podcast.</param>
/// <param name="host">The host of this podcast.</param>
public class PodcastSearchResult(
    string name,
    string id,
    Thumbnail[] thumbnails,
    string browseId,
    YouTubeMusicEntity host) : SearchResult(name, id, thumbnails)
{
    /// <summary>
    /// Parses the JSON element into a <see cref="PodcastSearchResult"/>.
    /// </summary>
    /// <param name="element">The JSON element containing "musicResponsiveListItemRenderer".</param>
    internal static PodcastSearchResult Parse(
        JsonElement element)
    {
        JsonElement item = element
            .GetProperty("musicResponsiveListItemRenderer");

        JsonElement flexColumns = item
            .GetProperty("flexColumns");

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
            .If("Podcast", 2, 0);


        string name = flexColumns
            .GetElementAt(0)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs")
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = item
            .SelectOverlayNavigationPlaylistId();

        string browseId = item
            .SelectNavigationBrowseId();

        Thumbnail[] thumbnails = item
            .GetProperty("thumbnail")
            .SelectThumbnails();

        YouTubeMusicEntity host = descriptionRuns
            .GetElementAt(descriptionStartIndex)
            .SelectYouTubeMusicEntity();

        return new(name, id, thumbnails, browseId, host);
    }


    /// <summary>
    /// The browse ID of this podcast.
    /// </summary>
    public string BrowseId { get; } = browseId;

    /// <summary>
    /// The host of this podcast.
    /// </summary>
    public YouTubeMusicEntity Host { get; } = host;
}