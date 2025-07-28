﻿using System.Text.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Represents a song on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="SongInfo"/>.
/// </remarks>
/// <param name="name">The name of this song.</param>
/// <param name="id">The ID of this song.</param>
/// <param name="thumbnails">The thumbnails of this song.</param>
/// <param name="relatedBrowseId">The browse ID related to this song for full navigation.</param>
/// <param name="lyricsBrowseId">The browse ID used to fetch lyrics for this song, if available.</param>
/// <param name="artists">The artists who performed this song.</param>
/// <param name="album">The album of this song.</param>
/// <param name="duration">The duration of this song.</param>
/// <param name="isExplicit">Indicates whether this song is marked as explicit.</param>
/// <param name="releaseYear">The year this song was released.</param>
/// <param name="isCreditsAvailable">Whether credits are available to fetch for this song.</param>
/// <param name="isRatingsAllowed">Whether ratings are allowed for this song.</param>
/// <param name="radio">The radio related to this song, if available.</param>
public class SongInfo(
    string name,
    string id,
    Thumbnail[] thumbnails,
    string relatedBrowseId,
    string? lyricsBrowseId,
    YouTubeMusicEntity[] artists,
    YouTubeMusicEntity album,
    TimeSpan duration,
    bool isExplicit,
    int? releaseYear,
    bool isCreditsAvailable,
    bool isRatingsAllowed,
    Radio? radio) : EntityInfo(name, id, null, thumbnails)
{
    /// <summary>
    /// Parses the JSON element into a <see cref="SongInfo"/>.
    /// </summary>
    /// <param name="element">The JSON element containing "contents" and "playerOverlays".</param>
    internal static SongInfo Parse(
        JsonElement element)
    {
        JsonElement tabs = element
            .GetProperty("contents")
            .GetProperty("singleColumnMusicWatchNextResultsRenderer")
            .GetProperty("tabbedRenderer")
            .GetProperty("watchNextTabbedResultsRenderer")
            .GetProperty("tabs");

        JsonElement item = tabs
            .GetElementAt(0)
            .GetProperty("tabRenderer")
            .GetProperty("content")
            .GetProperty("musicQueueRenderer")
            .GetProperty("content")
            .GetProperty("playlistPanelRenderer")
            .GetProperty("contents")
            .GetElementAt(0)
            .GetProperty("playlistPanelVideoRenderer");

        JsonElement lyrics = tabs
            .GetElementAt(1)
            .GetProperty("tabRenderer");

        JsonElement menuItems = item
            .SelectMenuItems();

        JsonElement descriptionRuns = item
            .GetProperty("longBylineText")
            .GetProperty("runs");


        string name = item
            .GetProperty("title")
            .GetProperty("runs")
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = item
            .GetProperty("videoId")
            .GetString()
            .OrThrow();

        Thumbnail[] thumbnails = item
            .SelectThumbnails();

        string relatedBrowseId = tabs
            .GetElementAt(2)
            .GetProperty("tabRenderer")
            .GetProperty("endpoint")
            .GetProperty("browseEndpoint")
            .GetProperty("browseId")
            .GetString()
            .OrThrow();

        bool isLyricsUnavailable = (lyrics
            .GetPropertyOrNull("unselectable")
            ?.GetBoolean())
            .Or(false);

        string? lyricsBrowseId = isLyricsUnavailable
            ? null
            : lyrics
                .GetProperty("endpoint")
                .GetProperty("browseEndpoint")
                .GetProperty("browseId")
                .GetString();

        YouTubeMusicEntity[] artists = descriptionRuns
            .SelectArtists();

        bool hasKnownAlbum = (descriptionRuns
            .GetElementAtOrNull(artists.Length * 2)
            ?.GetPropertyOrNull("navigationEndpoint"))
            .If(null, false, true);

        YouTubeMusicEntity album = hasKnownAlbum
            ? descriptionRuns
                .GetElementAt(artists.Length * 2)
                .SelectAlbum()
            : menuItems
                .SelectAlbumUnknown();

        TimeSpan duration = item
            .GetProperty("lengthText")
            .GetProperty("runs")
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .ToTimeSpan()
            .OrThrow();

        bool isExplicit = item
            .GetPropertyOrNull("badges")
            .SelectContainsExplicitBadge();

        int? releaseYear = descriptionRuns
            .GetElementAtOrNull(artists.Length * 2 + (hasKnownAlbum ? 2 : 0))
            ?.GetPropertyOrNull("text")
            ?.GetString()
            ?.ToInt32();

        bool isCreditsAvailable = menuItems
            .SelectIsCreditsAvailable();

        bool isRatingsAllowed = element
            .GetProperty("playerOverlays")
            .GetProperty("playerOverlayRenderer")
            .GetProperty("actions")
            .GetElementAt(0)
            .GetProperty("likeButtonRenderer")
            .GetProperty("likesAllowed")
            .GetBoolean();

        Radio? radio = menuItems
            .SelectRadioOrNull();

        return new(name, id, thumbnails, relatedBrowseId, lyricsBrowseId, artists, album, duration, isExplicit, releaseYear, isCreditsAvailable, isRatingsAllowed, radio);
    }


    /// <summary>
    /// The browse ID for related content associated with this song.
    /// </summary>
    public string RelatedBrowseId { get; } = relatedBrowseId;

    /// <summary>
    /// The browse ID for lyrics associated with this song, if available.
    /// </summary>
    public string? LyricsBrowseId { get; } = lyricsBrowseId;

    /// <summary>
    /// Whether credits are available to fetch for this song.
    /// </summary>
    public bool IsCreditsAvailable { get; } = isCreditsAvailable;

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
    /// Whether ratings are allowed for this song.
    /// </summary>
    public bool IsRatingsAllowed { get; } = isRatingsAllowed;

    /// <summary>
    /// The radio associated with this song, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;
}