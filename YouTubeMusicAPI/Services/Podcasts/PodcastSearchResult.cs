using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Services.Search;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Podcasts;

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
    string browseId,
    Thumbnail[] thumbnails,
    YouTubeMusicEntity host) : SearchResult(name, id, browseId, thumbnails)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="PodcastSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static PodcastSearchResult Parse(
        JElement element)
    {
        JElement flexColumns = element
            .Get("flexColumns");

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
            .Is("Podcast")
                ? 2
                : 0;


        string name = flexColumns
            .GetAt(0)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .SelectRunTextAt("text", 0)
            .OrThrow();

        string id = element
            .Get("overlay")
            .SelectOverlayPlaylistId()
            .OrThrow();

        string browseId = element
            .SelectNavigationBrowseId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity host = descriptionRuns
            .GetAt(descriptionStartIndex)
            .SelectArtist();

        return new(name, id, browseId, thumbnails, host);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="PodcastSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static PodcastSearchResult ParseSuggestion(
        JElement element)
    {
        JElement flexColumns = element
            .Get("flexColumns");


        string name = flexColumns
            .GetAt(0)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .SelectRunTextAt("text", 0)
            .OrThrow();

        string id = element
            .Get("overlay")
            .SelectOverlayPlaylistId()
            .OrThrow();

        string browseId = element
            .SelectNavigationBrowseId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity host = flexColumns
            .GetAt(1)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .Get("text")
            .Get("runs")
            .GetAt(2)
            .SelectArtist();

        return new(name, id, browseId, thumbnails, host);
    }


    /// <summary>
    /// The browse ID of this podcast.
    /// </summary>
    public override string BrowseId { get; } = browseId;


    /// <summary>
    /// The host of this podcast.
    /// </summary>
    public YouTubeMusicEntity Host { get; } = host;
}