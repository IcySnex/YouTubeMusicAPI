using System.Text.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a podcast search result on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="PodcastSearchResult"/> class.
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
    YouTubeMusicEntity host) : SearchResult(name, id, browseId, thumbnails)
{
    /// <summary>
    /// Parses a <see cref="JsonElement"/> into a <see cref="PodcastSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> "musicResponsiveListItemRenderer".</param>
    internal static PodcastSearchResult Parse(
        JsonElement element)
    {
        JsonElement flexColumns = element
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

        string id = element
            .GetProperty("overlay")
            .SelectOverlayNavigationPlaylistId();

        string browseId = element
            .SelectNavigationBrowseId();

        Thumbnail[] thumbnails = element
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity host = descriptionRuns
            .GetElementAt(descriptionStartIndex)
            .SelectArtist();

        return new(name, id, thumbnails, browseId, host);
    }

    /// <summary>
    /// Parses a <see cref="JsonElement"/> into a <see cref="PodcastSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> "musicResponsiveListItemRenderer".</param>
    internal static PodcastSearchResult ParseSuggestion(
        JsonElement element)
    {
        JsonElement flexColumns = element
            .GetProperty("flexColumns");


        string name = flexColumns
            .GetElementAt(0)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs")
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = element
            .GetProperty("overlay")
            .SelectOverlayNavigationPlaylistId();

        string browseId = element
            .SelectNavigationBrowseId();

        Thumbnail[] thumbnails = element
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity host = flexColumns
            .GetElementAt(1)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs")
            .GetElementAt(2)
            .SelectArtist();

        return new(name, id, thumbnails, browseId, host);
    }


    /// <summary>
    /// The host of this podcast.
    /// </summary>
    public YouTubeMusicEntity Host { get; } = host;
}