using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Albums;

/// <summary>
/// Represents a artist album on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="ArtistAlbum"/>.
/// </remarks>
/// <param name="name">The name of this artist album.</param>
/// <param name="id">The ID of this artist album.</param>
/// <param name="thumbnails">The thumbnails of this artist album.</param>
/// <param name="browseId">The browse ID of this artist album.</param>
/// <param name="releaseYear">The year this artist album was released in.</param>
/// <param name="isExplicit">Whether this artist album is explicit or not.</param>
/// <param name="type">The type of this artist album, e.g. Album, Single, EP.</param>
/// <param name="radio">The radio associated with this artist album, if available.</param>
public class ArtistAlbum(
    string name,
    string id,
    string browseId,
    Thumbnail[] thumbnails,
    int? releaseYear,
    bool isExplicit,
    AlbumType type,
    Radio? radio) : YouTubeMusicEntity(name, id, browseId)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="ArtistAlbum"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> 'musicTwoRowItemRenderer' to parse.</param>
    /// <returns>A <see cref="ArtistAlbum"/> representing the <see cref="JElement"/>.</returns>
    internal static ArtistAlbum Parse(
        JElement element)
    {
        JElement subtitleRuns = element
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
            .SelectNavigationBrowseId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnailRenderer")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        int? releaseYear = subtitleRuns
            .GetAt(subtitleRuns.ArrayLength - 1)
            .Get("text")
            .AsString()
            .ToInt32();

        bool isExplicit = element
            .SelectIsExplicit("subtitleBadges");

        AlbumType type = subtitleRuns
            .GetAt(0)
            .Get("text")
            .AsString()
            .ToAlbumType()
            .Or(AlbumType.Album);

        Radio? radio = element
            .SelectMenu()
            .SelectRadio();

        return new(name, id, browseId, thumbnails, releaseYear, isExplicit, type, radio);
    }


    /// <summary>
    /// The ID of this artist album.
    /// </summary>
    public override string Id { get; } = id;

    /// <summary>
    /// The browse ID of this artist album.
    /// </summary>
    public override string BrowseId { get; } = browseId;


    /// <summary>
    /// The thumbnails of this artist album.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The year this artist album was released in.
    /// </summary>
    public int? ReleaseYear { get; } = releaseYear;

    /// <summary>
    /// Whether this artist album is explicit or not.
    /// </summary>
    public bool IsExplicit { get; } = isExplicit;

    /// <summary>
    /// The type of this artist album, e.g. Album, Single, EP.
    /// </summary>
    public AlbumType Type { get; } = type;

    /// <summary>
    /// The radio associated with this artist album, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}