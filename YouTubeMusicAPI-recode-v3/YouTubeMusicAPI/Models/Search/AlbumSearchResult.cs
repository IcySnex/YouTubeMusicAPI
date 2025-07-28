using System.Text.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents an album search result on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="AlbumSearchResult"/>.
/// </remarks>
/// <param name="name">The name of this album.</param>
/// <param name="id">The ID of this album.</param>
/// <param name="thumbnails">The thumbnails of this album.</param>
/// <param name="browseId">The browse ID of this album.</param>
/// <param name="artists">The artists of this album.</param>
/// <param name="releaseYear">The year this alvbum was released in.</param>
/// <param name="isExplicit">Whether this album is explicit or not.</param>
/// <param name="type">The type of this album, e.g. Album, Single, EP.</param>
/// <param name="radio">The radio associated with this album, if available.</param>
public class AlbumSearchResult(
    string name,
    string id,
    Thumbnail[] thumbnails,
    string browseId,
    YouTubeMusicEntity[] artists,
    int? releaseYear,
    bool isExplicit,
    AlbumType type,
    Radio? radio) : SearchResult(name, id, browseId, thumbnails)
{
    /// <summary>
    /// Parses the JSON element into a <see cref="AlbumSearchResult"/>.
    /// </summary>
    /// <param name="element">The JSON element containing "musicResponsiveListItemRenderer".</param>
    internal static AlbumSearchResult Parse(
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
            .GetProperty("overlay")
            .SelectOverlayNavigationPlaylistId();

        Thumbnail[] thumbnails = item
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        string browseId = item
            .SelectNavigationBrowseId();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists(2);

        int? releaseYear = descriptionRuns
            .GetElementAtOrNull(artists.Length * 2 + 2)
            ?.GetPropertyOrNull("text")
            ?.GetString()
            .ToInt32();

        bool isExplicit = item
            .GetPropertyOrNull("badges")
            .SelectContainsExplicitBadge();

        AlbumType type = descriptionRuns
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .ToAlbumType()
            .OrThrow();

        Radio? radio = item
            .SelectMenuItems()
            .SelectRadioOrNull();

        return new(name, id, thumbnails, browseId, artists, releaseYear, isExplicit, type, radio);
    }

    /// <summary>
    /// Parses the JSON item into an <see cref="AlbumSearchResult"/>.
    /// </summary>
    /// <param name="item">The JSON item "musicCardShelfRenderer".</param>
    internal static AlbumSearchResult ParseTopResult(
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
            .SelectOverlayNavigationPlaylistId();

        Thumbnail[] thumbnails = item
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        string browseId = item
            .SelectTapBrowseId();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists(2);

        int? releaseYear = null;

        bool isExplicit = item
            .GetPropertyOrNull("subtitleBadges")
            .SelectContainsExplicitBadge();

        AlbumType type = descriptionRuns
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .ToAlbumType()
            .OrThrow();

        Radio? radio = item
            .SelectMenuItems()
            .SelectRadioOrNull();

        return new(name, id, thumbnails, browseId, artists, releaseYear, isExplicit, type, radio);
    }

    /// <summary>
    /// Parses the JSON element into a <see cref="AlbumSearchResult"/>.
    /// </summary>
    /// <param name="item">The JSON item "musicResponsiveListItemRenderer".</param>
    internal static AlbumSearchResult ParseSuggestion(
        JsonElement item)
    {
        JsonElement menuItems = item
            .SelectMenuItems();

        JsonElement flexColumns = item
            .GetProperty("flexColumns");

        JsonElement descriptionRuns = flexColumns
            .GetElementAt(1)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs");


        string name = flexColumns
            .GetElementAt(0)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs")
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = menuItems
            .SelectPlaylistIdOrNull()
            .OrThrow();

        Thumbnail[] thumbnails = item
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        string browseId = item
            .SelectNavigationBrowseId();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists(2);

        int? releaseYear = descriptionRuns
            .GetElementAtOrNull(artists.Length * 2 + 2)
            ?.GetPropertyOrNull("text")
            ?.GetString()
            .ToInt32();

        bool isExplicit = item
            .GetPropertyOrNull("badges")
            .SelectContainsExplicitBadge();

        AlbumType type = descriptionRuns
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .ToAlbumType()
            .OrThrow();

        Radio? radio = menuItems
            .SelectRadioOrNull();

        return new(name, id, thumbnails, browseId, artists, releaseYear, isExplicit, type, radio);
    }


    /// <summary>
    /// The artists of this album.
    /// </summary>
    public YouTubeMusicEntity[] Artists { get; } = artists;

    /// <summary>
    /// The year this alvbum was released in.
    /// </summary>
    public int? ReleaseYear { get; } = releaseYear;

    /// <summary>
    /// Whether this album is explicit or not.
    /// </summary>
    public bool IsExplicit { get; } = isExplicit;

    /// <summary>
    /// The type of this album, e.g. Album, Single, EP.
    /// </summary>
    public AlbumType Type { get; } = type;

    /// <summary>
    /// The radio associated with this album, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}