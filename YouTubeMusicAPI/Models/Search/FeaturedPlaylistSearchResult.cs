using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a featured playlist search result on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="FeaturedPlaylistSearchResult"/> class.
/// </remarks>
/// <param name="name">The name of this featured playlist.</param>
/// <param name="id">The ID of this featured playlist.</param>
/// <param name="thumbnails">The thumbnails of this featured playlist.</param>
/// <param name="browseId">The browse ID of this featured playlist.</param>
/// <param name="isMix">Whether this featured playlist is a mix.</param>
/// <param name="songsInfo">The information about the number of songs this featured playlist has.</param>
/// <param name="radio">The radio associated with this featured playlist, if available.</param>
public class FeaturedPlaylistSearchResult(
    string name,
    string id,
    string browseId,
    Thumbnail[] thumbnails,
    bool isMix,
    string songsInfo,
    Radio? radio) : SearchResult(name, id, browseId, thumbnails)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="FeaturedPlaylistSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static FeaturedPlaylistSearchResult Parse(
        JElement element)
    {
        JElement flexColumns = element
            .Get("flexColumns");

        JElement descriptionRuns = flexColumns
            .GetAt(1)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .Get("text")
            .Get("runs");

        string type = descriptionRuns
            .GetAt(0)
            .Get("text")
            .AsString()
            .OrThrow();

        int descriptionStartIndex = type == "Playlist" || type == "Mix" ? 2 : 0;


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

        bool isMix = type
            .Is("Mix");

        string songsInfo = descriptionRuns
            .GetAt(descriptionStartIndex + 2)
            .Get("text")
            .AsString()
            .Or("N/A songs");

        Radio? radio = element
            .SelectMenu()
            .SelectRadio();

        return new(name, id, browseId, thumbnails, isMix, songsInfo, radio);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="FeaturedPlaylistSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicCardShelfRenderer".</param>
    internal static FeaturedPlaylistSearchResult ParseTopResult(
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

        bool isMix = descriptionRuns
            .GetAt(0)
            .Get("text")
            .AsString()
            .OrThrow()
            .Is("Mix");

        string songsInfo = "N/A songs";

        Radio? radio = element
            .SelectMenu()
            .SelectRadio();

        return new(name, id, browseId, thumbnails, isMix, songsInfo, radio);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="FeaturedPlaylistSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static FeaturedPlaylistSearchResult ParseSuggestion(
        JElement element)
    {
        JElement flexColumns = element
            .Get("flexColumns");

        JElement descriptionRuns = flexColumns
            .GetAt(1)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .Get("text")
            .Get("runs");

        string type = descriptionRuns
            .GetAt(0)
            .Get("text")
            .AsString()
            .OrThrow();

        int descriptionStartIndex = type == "Playlist" || type == "Mix" ? 2 : 0;


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

        bool isMix = type
            .Is("Mix");

        string songsInfo = descriptionRuns
            .GetAt(descriptionStartIndex + 2)
            .Get("text")
            .AsString()
            .Or("N/A songs");

        Radio? radio = element
            .SelectMenu()
            .SelectRadio();

        return new(name, id, browseId, thumbnails, isMix, songsInfo, radio);
    }


    /// <summary>
    /// The browse ID of this featured playlist.
    /// </summary>
    public override string BrowseId { get; } = browseId;


    /// <summary>
    /// Whether this featured playlist is a mix.
    /// </summary>
    /// <remarks>
    /// A Mix is a nonstop playlist tailored to an user.
    /// </remarks>
    public bool IsMix { get; } = isMix;

    /// <summary>
    /// The information about the number of songs this featured playlist has.
    /// </summary>
    public string SongsInfo { get; } = songsInfo;

    /// <summary>
    /// The radio associated with this featured playlist, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}