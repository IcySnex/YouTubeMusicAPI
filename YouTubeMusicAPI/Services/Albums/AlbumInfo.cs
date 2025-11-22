using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Albums;

/// <summary>
/// Represents an album on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="AlbumInfo"/> class.
/// </remarks>
/// <param name="name">The name of this album.</param>
/// <param name="id">The ID of this album.</param>
/// <param name="browseId">The browse ID of this album.</param>
/// <param name="thumbnails">The thumbnails of this album.</param>
/// <param name="artists">The artists of this album.</param>
/// <param name="description">The description of this album, if available.</param>
/// <param name="releaseYear">The year this album was released in, if available.</param>
/// <param name="isExplicit">Whether this album is explicit or not.</param>
/// <param name="type">The type of this album, e.g. Album, Single, EP.</param>
/// <param name="itemsInfo">The information about the number of items this album has.</param>
/// <param name="lengthInfo">The information about the length this album has.</param>
/// <param name="radio">The radio associated with this album, if available.</param>
/// <param name="items">The items of this album.</param>
/// <param name="relations">The related content for this album.</param>
public class AlbumInfo(
    string name,
    string id,
    string browseId,
    Thumbnail[] thumbnails,
    YouTubeMusicEntity[] artists,
    string? description,
    int? releaseYear,
    bool isExplicit,
    AlbumType type,
    string itemsInfo,
    string lengthInfo,
    Radio? radio,
    IReadOnlyList<AlbumItem> items,
    AlbumRelations relations) : YouTubeMusicEntity(name, id, browseId)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="AlbumInfo"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> '$' to parse.</param>
    /// <returns>A <see cref="AlbumInfo"/> representing the <see cref="JElement"/>.</returns>
    internal static AlbumInfo Parse(
        JElement element)
    {
        JElement column = element
            .Get("contents")
            .Get("twoColumnBrowseResultsRenderer");

        JElement secondary = column
            .Get("secondaryContents")
            .Get("sectionListRenderer")
            .Get("contents");

        JElement item = column
            .Get("tabs")
            .GetAt(0)
            .Get("tabRenderer")
            .Get("content")
            .Get("sectionListRenderer")
            .Get("contents")
            .GetAt(0)
            .Get("musicResponsiveHeaderRenderer");

        JElement subtitleRuns = item
            .Get("subtitle")
            .Get("runs");

        JElement secondSubtitleRuns = item
            .Get("secondSubtitle")
            .Get("runs");

        JArray buttons = item
            .Get("buttons")
            .AsArray()
            .Or(JArray.Empty);


        string name = item
            .SelectRunTextAt("title", 0)
            .OrThrow();

        string id = buttons
            .FirstOrDefault(item => item
                .Contains("musicPlayButtonRenderer"))
            .Get("musicPlayButtonRenderer")
            .SelectPlayPlaylistId()
            .OrThrow();

        string browseId = element
            .Get("responseContext")
            .Get("serviceTrackingParams")
            .AsArray()
            .OrThrow()
            .FirstOrDefault(item => item
                .Get("service")
                .AsString()
                .Is("GFEEDBACK"))
            .Get("params")
            .AsArray()
            .OrThrow()
            .FirstOrDefault(item => item
                .Get("key")
                .AsString()
                .Is("browse_id"))
            .Get("value")
            .AsString()
            .OrThrow();

        Thumbnail[] thumbnails = item
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity[] artists = item
            .Get("straplineTextOne")
            .Get("runs")
            .SelectArtists();

        string? description = item
            .Get("description")
            .Get("musicDescriptionShelfRenderer")
            .SelectRunTextAt("description", 0);

        int? creationYear = subtitleRuns
            .GetAt(2)
            .Get("text")
            .AsString()
            .ToInt32();

        bool isExplicit = item
            .SelectIsExplicit("subtitleBadge");

        AlbumType type = subtitleRuns
            .GetAt(0)
            .Get("text")
            .AsString()
            .ToAlbumType()
            .OrThrow();

        string itemsInfo = secondSubtitleRuns
            .GetAt(0)
            .Get("text")
            .AsString()
            .Or("N/A songs");

        string lengthInfo = secondSubtitleRuns
            .GetAt(2)
            .Get("text")
            .AsString()
            .Or("N/A minutes");

        Radio? radio = buttons
            .FirstOrDefault(item => item
                .Contains("menuRenderer"))
            .Get("menuRenderer")
            .Get("items")
            .SelectRadio();

        IReadOnlyList<AlbumItem> items = secondary
            .GetAt(0)
            .Get("musicShelfRenderer")
            .Get("contents")
            .AsArray()
            .Or(JArray.Empty)
            .Where(item => item
                .Contains("musicResponsiveListItemRenderer"))
            .Select(item => item
                .Get("musicResponsiveListItemRenderer"))
            .Select(item => AlbumItem.Parse(item, artists))
            .ToList();

        AlbumRelations relations = AlbumRelations.Parse(secondary);

        return new(name, id, browseId, thumbnails, artists, description, creationYear, isExplicit, type, itemsInfo, lengthInfo, radio, items, relations);
    }


    /// <summary>
    /// The ID of this album.
    /// </summary>
    public override string Id { get; } = id;

    /// <summary>
    /// The browse ID of this album.
    /// </summary>
    public override string BrowseId { get; } = browseId;


    /// <summary>
    /// The thumbnails of this album.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The artists of this album.
    /// </summary>
    public YouTubeMusicEntity[] Artists { get; } = artists;

    /// <summary>
    /// The description of this album, if available.
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// The year this album was released in, if available.
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
    /// The information about the number of items this album has.
    /// </summary>
    public string ItemsInfo { get; } = itemsInfo;

    /// <summary>
    /// The information about the length this album has.
    /// </summary>
    public string LengthInfo { get; } = lengthInfo;

    /// <summary>
    /// The radio associated with this album, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;

    /// <summary>
    /// The items of this album.
    /// </summary>
    public IReadOnlyList<AlbumItem> Items { get; } = items;


    /// <summary>
    /// The related content for this album.
    /// </summary>
    internal AlbumRelations Relations { get; } = relations;
}