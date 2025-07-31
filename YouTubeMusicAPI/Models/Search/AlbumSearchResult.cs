using System.Text.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents an album search result on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="AlbumSearchResult"/> class.
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
    string browseId,
    Thumbnail[] thumbnails,
    YouTubeMusicEntity[] artists,
    int? releaseYear,
    bool isExplicit,
    AlbumType type,
    Radio? radio) : SearchResult(name, id, browseId, thumbnails)
{
    /// <summary>
    /// Parses a <see cref="JsonElement"/> into a <see cref="AlbumSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> "musicResponsiveListItemRenderer".</param>
    internal static AlbumSearchResult Parse(
        JsonElement element)
    {
        JsonElement flexColumns = element
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

        string id = element
            .GetProperty("overlay")
            .SelectOverlayNavigationPlaylistId();

        string browseId = element
            .SelectNavigationBrowseId();

        Thumbnail[] thumbnails = element
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists(2);

        int? releaseYear = descriptionRuns
            .GetElementAtOrNull(artists.Length * 2 + 2)
            ?.GetPropertyOrNull("text")
            ?.GetString()
            .ToInt32();

        bool isExplicit = element
            .GetPropertyOrNull("badges")
            .SelectContainsExplicitBadge();

        AlbumType type = descriptionRuns
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .ToAlbumType()
            .OrThrow();

        Radio? radio = element
            .SelectMenuItems()
            .SelectRadioOrNull();

        return new(name, id, browseId, thumbnails, artists, releaseYear, isExplicit, type, radio);
    }

    /// <summary>
    /// Parses a <see cref="JsonElement"/> into a <see cref="AlbumSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> "musicCardShelfRenderer".</param>
    internal static AlbumSearchResult ParseTopResult(
        JsonElement element)
    {
        JsonElement descriptionRuns = element
            .GetProperty("subtitle")
            .GetProperty("runs");


        string name = element
            .GetProperty("title")
            .GetProperty("runs")
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = element
            .GetProperty("thumbnailOverlay")
            .SelectOverlayNavigationPlaylistId();

        string browseId = element
            .SelectTapBrowseId();

        Thumbnail[] thumbnails = element
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists(2);

        int? releaseYear = null;

        bool isExplicit = element
            .GetPropertyOrNull("subtitleBadges")
            .SelectContainsExplicitBadge();

        AlbumType type = descriptionRuns
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .ToAlbumType()
            .OrThrow();

        Radio? radio = element
            .SelectMenuItems()
            .SelectRadioOrNull();

        return new(name, id, browseId, thumbnails, artists, releaseYear, isExplicit, type, radio);
    }

    /// <summary>
    /// Parses a <see cref="JsonElement"/> into a <see cref="AlbumSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> "musicResponsiveListItemRenderer".</param>
    internal static AlbumSearchResult ParseSuggestion(
        JsonElement element)
    {
        JsonElement menuItems = element
            .SelectMenuItems();

        JsonElement flexColumns = element
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

        string browseId = element
            .SelectNavigationBrowseId();

        Thumbnail[] thumbnails = element
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists(2);

        int? releaseYear = descriptionRuns
            .GetElementAtOrNull(artists.Length * 2 + 2)
            ?.GetPropertyOrNull("text")
            ?.GetString()
            .ToInt32();

        bool isExplicit = element
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

        return new(name, id, browseId, thumbnails, artists, releaseYear, isExplicit, type, radio);
    }


    /// <summary>
    /// The browse ID of this album.
    /// </summary>
    public override string BrowseId { get; } = browseId;


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