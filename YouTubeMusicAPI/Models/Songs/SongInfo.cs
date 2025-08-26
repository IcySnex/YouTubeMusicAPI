using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Models.Videos;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Songs;

/// <summary>
/// Represents a song on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="SongInfo"/>.
/// </remarks>
/// <param name="name">The name of this song.</param>
/// <param name="id">The ID of this song.</param>
/// <param name="thumbnails">The thumbnails of this song.</param>
/// <param name="relationsBrowseId">The browse ID related to this song for full navigation.</param>
/// <param name="lyricsBrowseId">The browse ID used to fetch lyrics for this song, if available.</param>
/// <param name="artists">The artists who performed this song.</param>
/// <param name="album">The album of this song.</param>
/// <param name="duration">The duration of this song.</param>
/// <param name="isExplicit">Indicates whether this song is marked as explicit.</param>
/// <param name="releaseYear">The year this song was released.</param>
/// <param name="isCreditsAvailable">Whether credits are available to fetch for this song.</param>
/// <param name="radio">The radio related to this song, if available.</param>
public class SongInfo(
    string name,
    string id,
    Thumbnail[] thumbnails,
    YouTubeMusicEntity[] artists,
    YouTubeMusicEntity album,
    TimeSpan duration,
    bool isExplicit,
    int? releaseYear,
    bool isCreditsAvailable,
    Radio? radio,
    string relationsBrowseId,
    string? lyricsBrowseId) : YouTubeMusicEntity(name, id, null)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="SongInfo"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> '$' to parse.</param>
    /// <returns>A <see cref="SongInfo"/> representing the <see cref="JElement"/>.</returns>
    internal static SongInfo Parse(
        JElement element)
    {
        JElement tabs = element
            .Get("contents")
            .Get("singleColumnMusicWatchNextResultsRenderer")
            .Get("tabbedRenderer")
            .Get("watchNextTabbedResultsRenderer")
            .Get("tabs");

        JElement content = tabs
            .GetAt(0)
            .Get("tabRenderer")
            .Get("content")
            .Get("musicQueueRenderer")
            .Get("content")
            .Get("playlistPanelRenderer")
            .Get("contents")
            .GetAt(0);

        JElement item = content
            .Coalesce(
                item => item
                    .Get("playlistPanelVideoRenderer"),
                item => item
                    .Get("playlistPanelVideoWrapperRenderer")
                    .Get("primaryRenderer")
                    .Get("playlistPanelVideoRenderer"));

        JElement counterpartItem = content
            .Get("playlistPanelVideoWrapperRenderer")
            .Get("counterpart")
            .GetAt(0)
            .Get("counterpartRenderer")
            .Get("playlistPanelVideoRenderer");

        JElement lyricsTab = tabs
            .GetAt(1)
            .Get("tabRenderer");


        string relationsBrowseId = tabs
            .GetAt(2)
            .Get("tabRenderer")
            .Get("endpoint")
            .Get("browseEndpoint")
            .Get("browseId")
            .AsString()
            .OrThrow();

        string? lyricsBrowseId = lyricsTab
            .Get("unselectable")
            .AsBool()
            .Or(false)
            .If(true,
                null,
                lyricsTab
                    .Get("endpoint")
                    .Get("browseEndpoint")
                    .Get("browseId")
                    .AsString());

        return Parse(item, counterpartItem, relationsBrowseId, lyricsBrowseId);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="SongInfo"/>.
    /// </summary>
    /// <param name="item">The <see cref="JElement"/> '...content.playlistPanelVideoRenderer' to parse.</param>
    /// <param name="counterpartItem">The <see cref="JElement"/> '...content.playlistPanelVideoWrapperRenderer.counterpart[0].counterpartRenderer.playlistPanelVideoRenderer' to parse.</param>
    /// <param name="relationsBrowseId">The browse ID for related content associated with this video.</param>
    /// <param name="lyricsBrowseId">The browse ID for lyrics associated with this video, if available.</param>
    /// <returns>A <see cref="SongInfo"/> representing the <see cref="JElement"/>.</returns>
    internal static SongInfo Parse(
        JElement item,
        JElement counterpartItem,
        string relationsBrowseId,
        string? lyricsBrowseId)
    {
        JElement menu = item
            .SelectMenu();

        JElement descriptionRuns = item
            .Get("longBylineText")
            .Get("runs");


        string name = item
            .SelectRunTextAt("title", 0)
            .OrThrow();

        string id = item
            .Get("videoId")
            .AsString()
            .OrThrow();

        Thumbnail[] thumbnails = item
            .SelectThumbnails();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists();

        bool isAlbumUnknown = descriptionRuns
            .GetAt(artists.Length * 2)
            .Get("navigationEndpoint")
            .IsUndefined;
        YouTubeMusicEntity album = isAlbumUnknown
            .If(true,
                menu
                    .SelectAlbumUnknown(),
                descriptionRuns
                    .GetAt(artists.Length * 2)
                    .SelectAlbum());

        TimeSpan duration = item
            .SelectRunTextAt("lengthText", 0)
            .ToTimeSpan()
            .OrThrow();

        bool isExplicit = item
            .SelectIsExplicit();

        int? releaseYear = descriptionRuns
            .GetAt(artists.Length * 2 + (isAlbumUnknown ? 0 : 2))
            .Get("text")
            .AsString()
            .ToInt32();

        bool isCreditsAvailable = menu
            .SelectIsCreditsAvailable();

        Radio? radio = menu
            .SelectRadio();


        SongInfo result = new(name, id, thumbnails, artists, album, duration, isExplicit, releaseYear, isCreditsAvailable, radio, relationsBrowseId, lyricsBrowseId);

        if (!counterpartItem.IsUndefined)
        {
            result.CounterpartVideo = VideoInfo.Parse(counterpartItem, default, relationsBrowseId, lyricsBrowseId);
            result.CounterpartVideo.CounterpartSong = result;
        }

        return result;
    }

    /// <summary>
    /// The ID of this song.
    /// </summary>
    public override string Id { get; } = id;


    /// <summary>
    /// The thumbnails of this song.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The artists of this song.
    /// </summary>
    public YouTubeMusicEntity[] Artists { get; } = artists;

    /// <summary>
    /// The album of this song.
    /// </summary>
    public YouTubeMusicEntity Album { get; } = album;

    /// <summary>
    /// The duration of this song.
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// Indicates whether this song is marked as explicit.
    /// </summary>
    public bool IsExplicit { get; } = isExplicit;

    /// <summary>
    /// The year this song was released.
    /// </summary>
    public int? ReleaseYear { get; } = releaseYear;

    /// <summary>
    /// Whether credits are available to fetch for this song.
    /// </summary>
    public bool IsCreditsAvailable { get; } = isCreditsAvailable;

    /// <summary>
    /// Whether lyrics are available to fetch for this song.
    /// </summary>
    public bool IsLyricsAvailable { get; } = lyricsBrowseId is not null;

    /// <summary>
    /// The radio associated with this song, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;


    /// <summary>
    /// The counterpart video of this song, if available.
    /// </summary>
    /// <remarks>
    /// Only available for authenticated users with premium subscription.<br/>
    /// For more information about audio-only or video mode, see: <see href="https://support.google.com/youtubemusic/answer/6313574"/>
    /// </remarks>
    public VideoInfo? CounterpartVideo { get; internal set; }


    /// <summary>
    /// The browse ID for related content associated with this song.
    /// </summary>
    internal string RelationsBrowseId { get; } = relationsBrowseId;

    /// <summary>
    /// The browse ID for lyrics associated with this song, if available.
    /// </summary>
    internal string? LyricsBrowseId { get; } = lyricsBrowseId;
}