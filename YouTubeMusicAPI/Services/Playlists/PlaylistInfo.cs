using System.Text.Json.Serialization;
using YouTubeMusicAPI.Common;
using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Pagination;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Playlists;

/// <summary>
/// Represents a playlist on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="PlaylistInfo"/>.
/// </remarks>
/// <param name="name">The name of this playlist.</param>
/// <param name="id">The ID of this playlist.</param>
/// <param name="browseId">The browse ID of this playlist.</param>
/// <param name="thumbnails">The thumbnails of this playlist.</param>
/// <param name="creator">The creator of this playlist, if available.</param>
/// <param name="description">The description of this playlist, if available.</param>
/// <param name="isOwner">Whether this playlist is owned by the current user.</param>
/// <param name="isMix">Whether this playlist is a mix.</param>
/// <param name="isRadio">Whether this playlist is a radio.</param>
/// <param name="creationYear">The year this playlist has been created in, if available.</param>
/// <param name="privacy">The privacy settings of this playlist.</param>
/// <param name="viewsInfo">The information about the number of views this playlist has.</param>
/// <param name="itemsInfo">The information about the number of items this playlist has.</param>
/// <param name="lengthInfo">The information about the length this playlist has.</param>
/// <param name="radio">The radio associated with this playlist, if available.</param>
/// <param name="items">The async paginator that fetches items for this playlist.</param>
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
    bool isRadio,
    int? creationYear,
    PlaylistPrivacy privacy,
    string viewsInfo,
    string itemsInfo,
    string lengthInfo,
    Radio? radio,
    PaginatedAsyncEnumerable<PlaylistItem> items,
    string? relationsContinuationToken) : YouTubeMusicEntity(name, id, browseId)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="PlaylistInfo"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> '$' to parse.</param>
    /// <param name="items">The async paginator that fetches items for this playlist.</param>
    /// <returns>A <see cref="PlaylistInfo"/> representing the <see cref="JElement"/>.</returns>
    internal static PlaylistInfo Parse(
        JElement element,
        PaginatedAsyncEnumerable<PlaylistItem> items)
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
                ? null
                : new(creatorName, creatorId, creatorId);

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

        bool isRadio = false;

        int? creationYear = subtitleRuns
            .GetAt(isOwner ? 4 : 2)
            .Get("text")
            .AsString()
            .ToInt32();

        PlaylistPrivacy privacy = isMix
            ? PlaylistPrivacy.Private
            : isOwner
                ? subtitleRuns
                    .GetAt(2)
                    .Get("text")
                    .AsString()
                    .ToPlaylistPrivacy()
                    .Or(PlaylistPrivacy.Public)
                : PlaylistPrivacy.Public;

        bool hasViewsInfo = secondSubtitleRuns
            .ArrayLength
            .Is(5);
        string viewsInfo = hasViewsInfo
            ? secondSubtitleRuns
                .GetAt(0)
                .Get("text")
                .AsString()
                .Or("N/A views")
            : "N/A views";

        string itemsInfo = secondSubtitleRuns
            .GetAt(hasViewsInfo ? 2 : 0)
            .Get("text")
            .AsString()
            .Or("N/A tracks");

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

        return new(name, id, browseId, thumbnails, creator, description, isOwner, isMix, isRadio, creationYear, privacy, viewsInfo, itemsInfo, lengthInfo, radio, items, relationsContinuationToken);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="PlaylistInfo"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> '$' to parse.</param>
    /// <param name="items">The async paginator that fetches items for this playlist.</param>
    /// <returns>A <see cref="PlaylistInfo"/> representing the <see cref="JElement"/>.</returns>
    internal static PlaylistInfo ParseRadio(
        JElement element,
        PaginatedAsyncEnumerable<PlaylistItem> items)
    {
        JElement queue = element
            .Get("contents")
            .Get("singleColumnMusicWatchNextResultsRenderer")
            .Get("tabbedRenderer")
            .Get("watchNextTabbedResultsRenderer")
            .Get("tabs")
            .GetAt(0)
            .Get("tabRenderer")
            .Get("content")
            .Get("musicQueueRenderer");

        JElement item = queue
            .Get("content")
            .Get("playlistPanelRenderer");


        string name = queue
            .Get("header")
            .Get("musicQueueHeaderRenderer")
            .SelectRunTextAt("subtitle", 0)
            .OrThrow();

        string id = item
            .Get("playlistId")
            .AsString()
            .OrThrow();

        string browseId = $"VL{id}"; // doesn't even have a browseId so fake it 'till we make it ig

        Thumbnail[] thumbnails = [
            new($"https://www.gstatic.com/youtube/media/ytm/images/pbg/attribute-radio-fallback-{Math.Abs(id.Aggregate(0, (h, c) => h * 31 + c)) % 5 + 1}@1000.png", 1000, 1000)
            ]; // whoops again some fake data :3

        YouTubeMusicEntity? creator = null;

        string? description = null;

        bool isOwner = false;

        bool isMix = false;

        bool isRadio = true;

        PlaylistPrivacy privacy = PlaylistPrivacy.Public;

        int? creationYear = null;

        string viewsInfo = "N/A views";

        string itemsInfo = "N/A tracks";

        string lengthInfo = "N/A minutes";

        Radio? radio = null;

        string? relationsContinuationToken = null;

        return new(name, id, browseId, thumbnails, creator, description, isOwner, isMix, isRadio, creationYear, privacy, viewsInfo, itemsInfo, lengthInfo, radio, items, relationsContinuationToken);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="Page{T}"/> of <see cref="PlaylistItem"/>'s.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> '$' to parse.</param>
    /// <returns>A <see cref="Page{T}"/> representing the <see cref="JElement"/>.</returns>
    internal static Page<PlaylistItem> ParseItemsPage(
        JElement element)
    {
        JElement contents = element
            .Coalesce(
                item => item
                    .Get("contents")
                    .Get("twoColumnBrowseResultsRenderer")
                    .Get("secondaryContents")
                    .Get("sectionListRenderer")
                    .Get("contents")
                    .GetAt(0)
                    .Get("musicPlaylistShelfRenderer")
                    .Get("contents"),
                item => item
                    .Get("onResponseReceivedActions")
                    .GetAt(0)
                    .Get("appendContinuationItemsAction")
                    .Get("continuationItems"));


        List<PlaylistItem> result = contents
            .AsArray()
            .Or(JArray.Empty)
            .Where(item => item
                .Contains("musicResponsiveListItemRenderer"))
            .Select(item => item
                .Get("musicResponsiveListItemRenderer"))
            .Select(PlaylistItem.Parse)
            .ToList();

        string? nextContinuationToken = contents
            .GetAt(contents.ArrayLength - 1)
            .Get("continuationItemRenderer")
            .Get("continuationEndpoint")
            .Get("continuationCommand")
            .Get("token")
            .AsString();

        return new(result, nextContinuationToken);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="Page{T}"/> of <see cref="PlaylistItem"/>'s.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> '$' to parse.</param>
    /// <returns>A <see cref="Page{T}"/> representing the <see cref="JElement"/>.</returns>
    internal static Page<PlaylistItem> ParseItemsRadioPage(
        JElement element)
    {
        JElement playlistPanel = element
            .Coalesce(
                item => item
                    .Get("contents")
                    .Get("singleColumnMusicWatchNextResultsRenderer")
                    .Get("tabbedRenderer")
                    .Get("watchNextTabbedResultsRenderer")
                    .Get("tabs")
                    .GetAt(0)
                    .Get("tabRenderer")
                    .Get("content")
                    .Get("musicQueueRenderer")
                    .Get("content")
                    .Get("playlistPanelRenderer"),
                item => item
                    .Get("continuationContents")
                    .Get("playlistPanelContinuation"));


        List<PlaylistItem> result = playlistPanel
            .Get("contents")
            .AsArray()
            .Or(JArray.Empty)
            .Where(item => item
                .Contains("playlistPanelVideoRenderer"))
            .Select(item => item
                .Get("playlistPanelVideoRenderer"))
            .Select(PlaylistItem.ParseRadio)
            .ToList();

        string? nextContinuationToken = playlistPanel
            .Get("continuations")
            .GetAt(0)
            .Get("nextRadioContinuationData")
            .Get("continuation")
            .AsString();

        return new(result, nextContinuationToken);
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
    /// Whether this playlist is a radio.
    /// </summary>
    /// <remarks>
    /// A Radio is a nonstop playlist tailored to a song, album, playlist or artist which will always update.
    /// </remarks>
    public bool IsRadio { get; } = isRadio;

    /// <summary>
    /// The privacy settings of this playlist.
    /// </summary>
    /// <remarks>
    /// Only available if <see cref="IsOwner"/> or <see cref="IsMix"/> is <see langword="true"/>, else always <see cref="PlaylistPrivacy.Public"/>.
    /// </remarks>
    public PlaylistPrivacy Privacy { get; } = privacy;

    /// <summary>
    /// The year this playlist has been created in, if available.
    /// </summary>
    public int? CreationYear { get; } = creationYear;

    /// <summary>
    /// The information about the number of views this playlist has.
    /// </summary>
    public string ViewsInfo { get; } = viewsInfo;

    /// <summary>
    /// The information about the number of items this playlist has.
    /// </summary>
    public string ItemsInfo { get; } = itemsInfo;

    /// <summary>
    /// The information about the length this playlist has.
    /// </summary>
    public string LengthInfo { get; } = lengthInfo;

    /// <summary>
    /// Whether related content is available to fetch for this playlist.
    /// </summary>
    public bool IsRelationsAvailable => RelationsContinuationToken is not null;

    /// <summary>
    /// The radio associated with this playlist, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;

    /// <summary>
    /// The async paginator that fetches items for this playlist.
    /// </summary>
    [JsonIgnore]
    public PaginatedAsyncEnumerable<PlaylistItem> Items { get; } = items;


    /// <summary>
    /// The continuation token to fetch relations for this playlist.
    /// </summary>
    /// <remarks>
    /// Only available when <see cref="IsMix"/> is <see langword="false"/>.
    /// </remarks>
    internal string? RelationsContinuationToken { get; } = relationsContinuationToken;
}