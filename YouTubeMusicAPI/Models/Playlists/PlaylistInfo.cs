using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Playlists;

/// <summary>
/// Represents a playlist on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="PlaylistInfo"/>.
/// </remarks>
/// <param name="name">The name of this playlist.</param>
/// <param name="id">The ID of this playlist.</param>
/// <param name="browseId">The browse ID of this playlist.</param>
/// <param name="thumbnails">The thumbnails of this playlist</param>
/// <param name="creator">The creator of this playlist, if available</param>
/// <param name="description">The description of this playlist, if available</param>
/// <param name="isOwner">Whether this playlist is owned by the current user.</param>
/// <param name="isMix">Whether this playlist is a mix</param>
/// <param name="privacy">The privacy settings of this playlist</param>
/// <param name="creationYear">The year this playlist has been created in.</param>
/// <param name="viewsInfo">The information about the number of views this playlist has</param>
/// <param name="songsInfo">The information about the number of songs this playlist has</param>
/// <param name="lengthInfo">The information about the length this playlist has</param>
/// <param name="radio">The radio associated with this playlist, if available</param>
/// <param name="relationsContinuationToken">The continuation token to fetch relations for this playlist.</param>
public class PlaylistInfo(
    string name,
    string id,
    string browseId,
    Thumbnail[] thumbnails,
    YouTubeMusicEntity? creator,
    string? description,
    bool isOwner,
    bool isMix,
    PlaylistPrivacy privacy,
    int creationYear,
    string viewsInfo,
    string songsInfo,
    string lengthInfo,
    Radio? radio,
    string? relationsContinuationToken) : YouTubeMusicEntity(name, id, browseId)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="PlaylistInfo"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> '$' to parse.</param>
    /// <returns>A <see cref="PlaylistInfo"/> representing the <see cref="JElement"/>.</returns>
    internal static PlaylistInfo Parse(
        JElement element)
    {
        JElement twoColumn = element
            .Get("contents")
            .Get("twoColumnBrowseResultsRenderer");

        JElement item = twoColumn
            .Get("tabs")
            .GetAt(0)
            .Get("tabRenderer")
            .Get("content")
            .Get("sectionListRenderer")
            .Get("contents")
            .GetAt(0)
            .Coalesce(
                item => item
                    .Get("musicResponsiveHeaderRenderer"),
                item => item
                    .Get("musicEditablePlaylistDetailHeaderRenderer")
                    .Get("header")
                    .Get("musicResponsiveHeaderRenderer"));

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

        JElement avatarStack = item
            .Get("facepile")
            .Get("avatarStackViewModel");


        string name = item
            .SelectRunTextAt("title", 0)
            .OrThrow();

        string id = buttons
            .FirstOrDefault(item => item
                .Contains("musicPlayButtonRenderer"))
            .Get("musicPlayButtonRenderer")
            .SelectPlayPlaylistId()
            .OrThrow();

        string browseId = $"VL{id}";

        Thumbnail[] thumbnails = item
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        string creatorName = avatarStack
            .Get("text")
            .Get("content")
            .AsString()
            .OrThrow();
        string? creatorId = avatarStack
            .Get("rendererContext")
            .Get("commandContext")
            .Get("onTap")
            .Get("innertubeCommand")
            .Get("browseEndpoint")
            .Get("browseId")
            .AsString();
        YouTubeMusicEntity? creator = creatorName
            .Is("YouTube Music")
            .And(creatorId.IsNull())
            .If(true,
                null,
                new YouTubeMusicEntity(creatorName, creatorId, creatorId));

        string? description = item
            .Get("description")
            .Get("musicDescriptionShelfRenderer")
            .SelectRunTextAt("description", 0);

        bool isOwner = subtitleRuns
            .ArrayLength
            .Is(5);

        bool isMix = subtitleRuns
            .GetAt(0)
            .Get("text")
            .AsString()
            .Is("Mix");

        PlaylistPrivacy privacy = isMix
            .If(true,
                PlaylistPrivacy.Private,
                isOwner
                    .If(true,
                        subtitleRuns
                            .GetAt(2)
                            .Get("text")
                            .AsString()
                            .ToPlaylistPrivacy()
                            .Or(PlaylistPrivacy.Public),
                        PlaylistPrivacy.Public));

        int creationYear = subtitleRuns
            .GetAt(isOwner ? 4 : 2)
            .Get("text")
            .AsString()
            .ToInt32()
            .OrThrow();

        bool hasViewsInfo = secondSubtitleRuns
            .ArrayLength
            .Is(5);
        string viewsInfo = hasViewsInfo
            .If(true,
                secondSubtitleRuns
                    .GetAt(0)
                    .Get("text")
                    .AsString(),
                null)
            .Or("N/A views");

        string songsInfo = secondSubtitleRuns
            .GetAt(hasViewsInfo ? 2 : 0)
            .Get("text")
            .AsString()
            .Or("N/A songs");

        string lengthInfo = secondSubtitleRuns
            .GetAt(hasViewsInfo ? 4 : 2)
            .Get("text")
            .AsString()
            .Or("N/A minutes");

        Radio? radio = buttons
            .FirstOrDefault(item => item
                .Contains("menuRenderer"))
            .Get("menuRenderer")
            .Get("items")
            .SelectRadio();

        string? relationsContinuationToken = twoColumn
            .Get("secondaryContents")
            .Get("sectionListRenderer")
            .Get("continuations")
            .GetAt(0)
            .Get("nextContinuationData")
            .Get("continuation")
            .AsString();

        return new(name, id, browseId, thumbnails, creator, description, isOwner, isMix, privacy, creationYear, viewsInfo, songsInfo, lengthInfo, radio, relationsContinuationToken);
    }


    /// <summary>
    /// The ID of this playlist.
    /// </summary>
    public override string Id { get; } = id;

    /// <summary>
    /// The browse ID of this playlist.
    /// </summary>
    public override string BrowseId { get; } = browseId;


    /// <summary>
    /// The thumbnails of this playlist.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The creator of this playlist, if available.
    /// </summary>
    /// <remarks>
    /// Only available if the playlist is a community playlist, else <see langword="null"/>.
    /// </remarks>
    public YouTubeMusicEntity? Creator { get; } = creator;

    /// <summary>
    /// The description of this playlist, if available.
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// Whether this playlist is owned by the current user.
    /// </summary>
    public bool IsOwner { get; } = isOwner;

    /// <summary>
    /// Whether this playlist is a mix.
    /// </summary>
    /// <remarks>
    /// A Mix is a nonstop playlist tailored to an user.
    /// </remarks>
    public bool IsMix { get; } = isMix;

    /// <summary>
    /// The privacy settings of this playlist.
    /// </summary>
    /// <remarks>
    /// Only available if <see cref="IsOwner"/> or <see cref="IsMix"/> is <see langword="true"/>, else always <see cref="PlaylistPrivacy.Public"/>.
    /// </remarks>
    public PlaylistPrivacy Privacy { get; } = privacy;

    /// <summary>
    /// The year this playlist has been created in..
    /// </summary>
    public int CreationYear { get; } = creationYear;

    /// <summary>
    /// The information about the number of views this playlist has.
    /// </summary>
    public string ViewsInfo { get; } = viewsInfo;

    /// <summary>
    /// The information about the number of songs this playlist has.
    /// </summary>
    public string SongsInfo { get; } = songsInfo;

    /// <summary>
    /// The information about the lengt this playlist has.
    /// </summary>
    public string LengthInfo { get; } = lengthInfo;

    /// <summary>
    /// The radio associated with this playlist, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;


    /// <summary>
    /// The continuation token to fetch relations for this playlist.
    /// </summary>
    /// <remarks>
    /// Only available when <see cref="IsMix"/> is <see langword="false"/>.
    /// </remarks>
    internal string? RelationsContinuationToken { get; } = relationsContinuationToken;
}