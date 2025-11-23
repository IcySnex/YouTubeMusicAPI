using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Playlists;

/// <summary>
/// Represents a related playlist on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="RelatedPlaylist"/>.
/// </remarks>
/// <param name="name">The name of this playlist.</param>
/// <param name="id">The ID of this playlist.</param>
/// <param name="browseId">The browse ID of this playlist.</param>
/// <param name="thumbnails">The thumbnails of this playlist</param>
/// <param name="creator">The creator of this playlist, if available</param>
/// <param name="isMix">Whether this playlist is a mix</param>
/// <param name="viewsInfo">The information about the number of views this playlist has</param>
/// <param name="radio">The radio associated with this playlist, if available.</param>
public class RelatedPlaylist(
    string name,
    string id,
    string browseId,
    Thumbnail[] thumbnails,
    YouTubeMusicEntity? creator,
    bool isMix,
    string viewsInfo,
    Radio? radio) : YouTubeMusicEntity(name, id, browseId)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="RelatedPlaylist"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> 'musicTwoRowItemRenderer' to parse.</param>
    /// <returns>A <see cref="RelatedPlaylist"/> representing the <see cref="JElement"/>.</returns>
    internal static RelatedPlaylist Parse(
        JElement element)
    {
        JElement subtitleRuns = element
            .Get("subtitle")
            .Get("runs");

        JElement creatorRun = subtitleRuns
            .GetAt(2);


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

        string creatorName = creatorRun
            .Get("text")
            .AsString()
            .OrThrow();
        string? creatorId = creatorRun
            .SelectNavigationBrowseId();
        YouTubeMusicEntity? creator = creatorName
            .Is("YouTube Music")
            .And(creatorId.IsNull())
                ? null
                : new(creatorName, creatorId, creatorId);

        bool isMix = subtitleRuns
            .GetAt(0)
            .Get("text")
            .AsString()
            .Is("Mix");

        string viewsInfo = subtitleRuns
            .ArrayLength
            .Is(5)
                ? subtitleRuns
                    .GetAt(4)
                    .Get("text")
                    .AsString()
                    .Or("N/A views")
                : "N/A views";

        Radio? radio = element
            .SelectMenu()
            .SelectRadio();

        return new(name, id, browseId, thumbnails, creator, isMix, viewsInfo, radio);
    }


    /// <summary>
    /// The ID of this related playlist.
    /// </summary>
    public override string Id { get; } = id;

    /// <summary>
    /// The browse ID of this related playlist.
    /// </summary>
    public override string BrowseId { get; } = browseId;


    /// <summary>
    /// The thumbnails of this related playlist.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The creator of this related playlist, if available.
    /// </summary>
    /// <remarks>
    /// Only available if the playlist is a community playlist, else <see langword="null"/>.
    /// </remarks>
    public YouTubeMusicEntity? Creator { get; } = creator;

    /// <summary>
    /// Whether this related playlist is a mix.
    /// </summary>
    /// <remarks>
    /// A Mix is a nonstop playlist tailored to an user.
    /// </remarks>
    public bool IsMix { get; } = isMix;

    /// <summary>
    /// The information about the number of views this related playlist has.
    /// </summary>
    public string ViewsInfo { get; } = viewsInfo;

    /// <summary>
    /// The radio associated with this related playlist, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}