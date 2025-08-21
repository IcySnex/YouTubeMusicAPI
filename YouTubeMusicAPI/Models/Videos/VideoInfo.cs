using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Models.Songs;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Videos;

/// <summary>
/// Represents a video on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="VideoInfo"/>.
/// </remarks>
/// <param name="name">The name of this video.</param>
/// <param name="id">The ID of this video.</param>
/// <param name="thumbnails">The thumbnails of this video.</param>
/// <param name="relatedBrowseId">The browse ID related to this video for full navigation.</param>
/// <param name="lyricsBrowseId">The browse ID used to fetch lyrics for this video, if available.</param>
/// <param name="artists">The artists who performed this video.</param>
/// <param name="duration">The duration of this video.</param>
/// <param name="viewsInfo">The information about the number of views this video has.</param>
/// <param name="ratingsInfo">The information about the number of likes this video has.</param>
/// <param name="radio">The radio related to this video, if available.</param>
/// <param name="counterpartSong">The counterpart song of this video, if available.</param>
public class VideoInfo(
    string name,
    string id,
    Thumbnail[] thumbnails,
    string relatedBrowseId,
    string? lyricsBrowseId,
    YouTubeMusicEntity[] artists,
    TimeSpan duration,
    string viewsInfo,
    string ratingsInfo,
    Radio? radio,
    SongInfo? counterpartSong) : YouTubeMusicEntity(name, id, null)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="VideoInfo"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> '$' to parse.</param>
    /// <returns>A <see cref="VideoInfo"/> representing the <see cref="JElement"/>.</returns>
    internal static VideoInfo Parse(
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


        string relatedBrowseId = tabs
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

        return Parse(item, counterpartItem, relatedBrowseId, lyricsBrowseId);
    }

    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="VideoInfo"/>.
    /// </summary>

    /// <param name="item">The <see cref="JElement"/> '...content.playlistPanelVideoRenderer' to parse.</param>
    /// <param name="counterpartItem">The <see cref="JElement"/> '...content.playlistPanelVideoWrapperRenderer.counterpart[0].counterpartRenderer.playlistPanelVideoRenderer' to parse.</param>
    /// <param name="relatedBrowseId">The browse ID for related content associated with this video.</param>
    /// <param name="lyricsBrowseId">The browse ID for lyrics associated with this video, if available.</param>
    /// <returns>A <see cref="VideoInfo"/> representing the <see cref="JElement"/>.</returns>
    internal static VideoInfo Parse(
        JElement item,
        JElement counterpartItem,
        string relatedBrowseId,
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

        TimeSpan duration = item
            .SelectRunTextAt("lengthText", 0)
            .ToTimeSpan()
            .OrThrow();

        string viewsInfo = descriptionRuns
            .GetAt(artists.Length * 2)
            .Get("text")
            .AsString()
            .OrThrow();

        string ratingsInfo = descriptionRuns
            .GetAt(artists.Length * 2 + 2)
            .Get("text")
            .AsString()
            .Or("N/A likes");

        Radio? radio = menu
            .SelectRadio();


        VideoInfo result = new(name, id, thumbnails, relatedBrowseId, lyricsBrowseId, artists, duration, viewsInfo, ratingsInfo, radio, null);

        if (!counterpartItem.IsUndefined)
        {
            result.CounterpartSong = SongInfo.Parse(counterpartItem, default, relatedBrowseId, lyricsBrowseId);
            result.CounterpartSong.CounterpartVideo = result;
        }

        return result;
    }


    /// <summary>
    /// The ID of this video.
    /// </summary>
    public override string Id { get; } = id;


    /// <summary>
    /// The thumbnails of this video.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The browse ID for related content associated with this video.
    /// </summary>
    public string RelatedBrowseId { get; } = relatedBrowseId;

    /// <summary>
    /// The browse ID for lyrics associated with this video, if available.
    /// </summary>
    public string? LyricsBrowseId { get; } = lyricsBrowseId;

    /// <summary>
    /// The artists of this video.
    /// </summary>
    public YouTubeMusicEntity[] Artists { get; } = artists;

    /// <summary>
    /// The duration of this video.
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// The information about the number of views this video has.
    /// </summary>
    public string ViewsInfo { get; } = viewsInfo;

    /// <summary>
    /// The information about the number of likes this video has.
    /// </summary>
    public string RatingsInfo { get; } = ratingsInfo;

    /// <summary>
    /// The radio associated with this video, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;

    /// <summary>
    /// The counterpart song of this video, if available.
    /// </summary>
    /// <remarks>
    /// Only available for authenticated users with premium subscription.<br/>
    /// For more information about audio-only or video mode, see: <see href="https://support.google.com/youtubemusic/answer/6313574"/>
    /// </remarks>
    public SongInfo? CounterpartSong { get; internal set; } = counterpartSong;
}