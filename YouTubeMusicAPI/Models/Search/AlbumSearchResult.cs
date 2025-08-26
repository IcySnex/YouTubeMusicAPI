using YouTubeMusicAPI.Json;
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
    /// Parses a <see cref="JElement"/> into a <see cref="AlbumSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static AlbumSearchResult Parse(
        JElement element)
    {
        JElement flexColumns = element
            .Get("flexColumns");

        JElement descriptionRuns = flexColumns
            .GetAt(1)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .Get("text")
            .Get("runs");


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

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists(2);

        int? releaseYear = descriptionRuns
            .GetAt(artists.Length * 2 + 2)
            .Get("text")
            .AsString()
            .ToInt32();

        bool isExplicit = element
            .SelectIsExplicit();

        AlbumType type = descriptionRuns
            .GetAt(0)
            .Get("text")
            .AsString()
            .ToAlbumType()
            .OrThrow();

        Radio? radio = element
            .SelectMenu()
            .SelectRadio();

        return new(name, id, browseId, thumbnails, artists, releaseYear, isExplicit, type, radio);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="AlbumSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicCardShelfRenderer".</param>
    internal static AlbumSearchResult ParseTopResult(
        JElement element)
    {
        JElement descriptionRuns = element
            .Get("subtitle")
            .Get("runs");


        string name = element
            .SelectRunTextAt("title", 0)
            .OrThrow();

        string id = element
            .Get("thumbnailOverlay")
            .SelectOverlayPlaylistId()
            .OrThrow();

        string browseId = element
            .SelectTapBrowseId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists(2);

        int? releaseYear = null;

        bool isExplicit = element
            .SelectIsExplicit("subtitleBadges");

        AlbumType type = descriptionRuns
            .GetAt(0)
            .Get("text")
            .AsString()
            .ToAlbumType()
            .OrThrow();

        Radio? radio = element
            .SelectMenu()
            .SelectRadio();

        return new(name, id, browseId, thumbnails, artists, releaseYear, isExplicit, type, radio);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="AlbumSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static AlbumSearchResult ParseSuggestion(
        JElement element)
    {
        JElement menu = element
            .SelectMenu();

        JElement flexColumns = element
            .Get("flexColumns");

        JElement descriptionRuns = flexColumns
            .GetAt(1)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .Get("text")
            .Get("runs");


        string name = flexColumns
            .GetAt(0)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .SelectRunTextAt("text", 0)
            .OrThrow();

        string id = menu
            .SelectPlaylistId()
            .OrThrow();

        string browseId = element
            .SelectNavigationBrowseId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists(2);

        int? releaseYear = descriptionRuns
            .GetAt(artists.Length * 2 + 2)
            .Get("text")
            .AsString()
            .ToInt32();

        bool isExplicit = element
            .SelectIsExplicit();

        AlbumType type = descriptionRuns
            .GetAt(0)
            .Get("text")
            .AsString()
            .ToAlbumType()
            .OrThrow();

        Radio? radio = menu
            .SelectRadio();

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