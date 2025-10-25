using Newtonsoft.Json.Linq;
using System.Web;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Info;
using YouTubeMusicAPI.Models.Streaming;

namespace YouTubeMusicAPI.Internal;

/// <summary>
/// Contains methods to select data from json tokens
/// </summary>
internal static class Selectors
{
    /// <summary>
    /// Selects a requiered token from a json token
    /// </summary>
    /// <param name="value">The json token containing the token</param>
    /// <param name="path">The json token path</param>
    /// <returns>A JToken</returns>
    /// <exception cref="ArgumentNullException">Occurrs when the specified path could not be found</exception>
    public static JToken SelectRequieredToken(
        this JToken value,
        string path)
    {
        JToken? result = value.SelectToken(path);
        return result is null ? throw new ArgumentNullException(path, "Requiered token is null.") : result;
    }


    /// <summary>
    /// Selects and casts an object from a json token
    /// </summary>
    /// <typeparam name="T">The type to cast to</typeparam>
    /// <param name="value">The json token containing the item data</param>
    /// <param name="path">The json token path</param>
    /// <returns>An object</returns>
    /// <exception cref="ArgumentNullException">Occurrs when the specified path could not be found</exception>
    public static T SelectObject<T>(
        this JToken value,
        string path)
    {
        object? result = value.SelectToken(path)?.ToObject(typeof(T));
        return result is null ? throw new ArgumentNullException(path, "Requiered token is null.") : (T)result;
    }

    /// <summary>
    /// Selects and casts an optional object from a json token
    /// </summary>
    /// <typeparam name="T">The type to cast to</typeparam>
    /// <param name="value">The json token containing the item data</param>
    /// <param name="path">The json token path</param>
    /// <returns>An object</returns>
    public static T? SelectObjectOptional<T>(
        this JToken value,
        string path) =>
        (T?)value.SelectToken(path)?.ToObject(typeof(T));


    /// <summary>
    /// Selects and casts a named entity from a json token
    /// </summary>
    /// <param name="value">The json token containing the item data</param>
    /// <param name="namePath">The json token name path</param>
    /// <param name="idPath">The json token name id</param>
    /// <exception cref="ArgumentNullException">Occurrs when the specified path could not be found</exception>
    /// <returns>A new shelf item</returns>
    public static NamedEntity SelectNamedEntity(
        this JToken value,
        string namePath,
        string idPath) =>
        new(value.SelectObject<string>(namePath), value.SelectObjectOptional<string>(idPath));

    /// <summary>
    /// Selects and casts an optional named entity from a json token
    /// </summary>
    /// <param name="value">The json token containing the item data</param>
    /// <param name="namePath">The json token name path</param>
    /// <param name="idPath">The json token name id</param>
    /// <returns>A new shelf item</returns>
    public static NamedEntity? SelectSehlfItemOptional(
        this JToken value,
        string namePath,
        string idPath) =>
        value.SelectObjectOptional<string>(namePath) is string name ? new(name, value.SelectObjectOptional<string>(idPath)) : null;


    /// <summary>
    /// Selects and casts a radio from a json token
    /// </summary>
    /// <param name="value">The json token containing the item data</param>
    /// <param name="path">The json token path</param>
    /// <exception cref="ArgumentNullException">Occurrs when the specified path could not be found</exception>
    /// <returns>A new radio</returns>
    public static DateTime SelectDateTime(
        this JToken value,
        string path)
    {
        string stringifiedDate = value.SelectObject<string>(path);

        if (!stringifiedDate.Contains(" ago"))
            return DateTime.Parse(stringifiedDate);

        string[] timeSpanParts = stringifiedDate.Split(' ');
        int timeSpanValue = int.Parse(timeSpanParts[0]);

        return timeSpanParts[1][0] switch
        {
            'd' => DateTime.Now - TimeSpan.FromDays(timeSpanValue),
            'h' => DateTime.Now - TimeSpan.FromHours(timeSpanValue),
            'm' => DateTime.Now - TimeSpan.FromMinutes(timeSpanValue),
            's' => DateTime.Now - TimeSpan.FromSeconds(timeSpanValue),
            _ => DateTime.Now
        };
    }


    /// <summary>
    /// Selects and casts a radio from a json token
    /// </summary>
    /// <param name="value">The json token containing the item data</param>
    /// <param name="playlistIdPath">The json token playlist id path</param>
    /// <param name="videoIdPath">The json token video id path</param>
    /// <exception cref="ArgumentNullException">Occurrs when the specified path could not be found</exception>
    /// <returns>A new radio</returns>
    public static Radio SelectRadio(
        this JToken value,
        string playlistIdPath = "menu.menuRenderer.items[0].menuNavigationItemRenderer.navigationEndpoint.watchEndpoint.playlistId",
        string? videoIdPath = "menu.menuRenderer.items[0].menuNavigationItemRenderer.navigationEndpoint.watchEndpoint.videoId") =>
        new(
            value.SelectObject<string>(playlistIdPath),
            videoIdPath is null ? null : value.SelectObjectOptional<string>(videoIdPath));

    /// <summary>
    /// Selects and casts an optional radio from a json token
    /// </summary>
    /// <param name="value">The json token containing the item data</param>
    /// <param name="playlistIdPath">The json token playlist id path</param>
    /// <param name="videoIdPath">The json token video id path</param>
    /// <exception cref="ArgumentNullException">Occurrs when the specified path could not be found</exception>
    /// <returns>A new radio</returns>
    public static Radio? SelectRadioOptional(
        this JToken value,
        string playlistIdPath = "menu.menuRenderer.items[0].menuNavigationItemRenderer.navigationEndpoint.watchEndpoint.playlistId",
        string? videoIdPath = "menu.menuRenderer.items[0].menuNavigationItemRenderer.navigationEndpoint.watchEndpoint.videoId") =>
        value.SelectObjectOptional<string>(playlistIdPath) is string playlistId ? new(playlistId, videoIdPath is null ? null : value.SelectObjectOptional<string>(videoIdPath)) : null;


    /// <summary>
    /// Selects and casts a bool weither its explicit from a json token
    /// </summary>
    /// <param name="value">The json token containing the item data</param>
    /// <param name="badgesPath">The json token badges path</param>
    /// <exception cref="ArgumentNullException">Occurrs when the specified path could not be found</exception>
    /// <returns>A bool</returns>
    public static bool SelectIsExplicit(
        this JToken value,
        string badgesPath) =>
        value.SelectObjectOptional<JToken[]>(badgesPath)?.Any(badge => badge.SelectObjectOptional<string>("musicInlineBadgeRenderer.icon.iconType") == "MUSIC_EXPLICIT_BADGE") ?? false;


    /// <summary>
    /// Selects and casts a thumbnail array from a json token
    /// </summary>
    /// <param name="value">The json token containing the item data</param>
    /// <param name="path">The json token path</param>
    /// <returns>An array of thumbnails</returns>
    public static Thumbnail[] SelectThumbnails(
        this JToken value,
        string path = "thumbnail.musicThumbnailRenderer.thumbnail.thumbnails")
    {
        // Parse container from json token
        JToken? thumbnails = value.SelectToken(path);
        if (thumbnails is null)
            return [];

        List<Thumbnail> result = [];
        foreach (JToken thumbnail in thumbnails)
        {
            // Parse info from container
            string? url = thumbnail.SelectToken("url")?.ToString();
            string? width = thumbnail.SelectToken("width")?.ToString();
            string? height = thumbnail.SelectToken("height")?.ToString();

            if (url is null)
                continue;

            result.Add(new(
                url,
                width is null ? 0 : int.Parse(width),
                height is null ? 0 : int.Parse(height)));
        }

        // Return result
        return [.. result];
    }


    /// <summary>
    /// Selects and casts artists from a json token
    /// </summary>
    /// <param name="value">The json token containing the item data</param>
    /// <param name="path">The json token path</param>
    /// <param name="startIndex">The index to start the runs container</param>
    /// <param name="trimBy">Trim the last items of runs container</param>
    /// <exception cref="ArgumentNullException">Occurrs when the specified path could not be found</exception>
    /// <returns>An array of artists</returns>
    public static NamedEntity[] SelectArtists(
        this JToken value,
        string path,
        int startIndex = 0,
        int trimBy = 0)
    {
        // Parse container from json token
        JToken[] runs = value.SelectObject<JToken[]>(path);

        List<NamedEntity> result = [];
        for (int i = startIndex; i < runs.Length - trimBy; i++)
        {
            JToken run = runs[i];

            // Parse info from container
            string? artist = run.SelectToken("text")?.ToString()?.Trim();
            string? artistId = run.SelectToken("navigationEndpoint.browseEndpoint.browseId")?.ToString();

            if (artist is null || artist == "," || artist == "&" || artist == "•")
                continue;

            result.Add(new(artist, artistId));
        }

        return [.. result];
    }

    /// <summary>
    /// Selects and casts simple artists from a json token
    /// </summary>
    /// <param name="value">The json token containing the item data</param>
    /// <returns>An array of artists</returns>
    public static NamedEntity[] SelectArtistsSimple(
        this JToken value)
    {
        // Parse artist names from json token
        string artistNames = value.SelectObject<string>("videoDetails.author");
        string primaryArtistId = value.SelectObject<string>("videoDetails.channelId");

        // Add artists to result
        IEnumerable<string> artists = artistNames.Split(',', '&', '•').Where(artistName => !string.IsNullOrWhiteSpace(artistName)).Select(artistName => artistName.Trim());

        List<NamedEntity> result = [];
        result.Add(new(artists.First(), primaryArtistId));
        foreach (string artist in artists.Skip(1))
            result.Add(new(artist, null));

        // Return result
        return [.. result];
    }


    /// <summary>
    /// Selects and casts album songs from a json token
    /// </summary>
    /// <param name="value">The json token containing the item data</param>
    /// <param name="path">The json token path</param>
    /// <returns>An array of album songs</returns>
    public static AlbumSong[] SelectAlbumSongs(
        this JToken value,
        string path)
    {
        List<AlbumSong> result = [];
        foreach (JToken content in value.SelectObject<JToken[]>(path))
        {
            if (content.SelectObjectOptional<JToken>("continuationItemRenderer") is not null)
                continue;

            result.Add(new(
                name: content.SelectObject<string>("musicResponsiveListItemRenderer.flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text"),
                id: content.SelectObjectOptional<string>("musicResponsiveListItemRenderer.flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].navigationEndpoint.watchEndpoint.videoId"),
                isExplicit: content.SelectObjectOptional<JToken[]>("musicResponsiveListItemRenderer.badges")?.Any(badge => badge.SelectToken("musicInlineBadgeRenderer.icon.iconType")?.ToString() == "MUSIC_EXPLICIT_BADGE") ?? false,
                playsInfo: content.SelectObjectOptional<string>("musicResponsiveListItemRenderer.flexColumns[2].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text"),
                duration: content.SelectObject<string>("musicResponsiveListItemRenderer.fixedColumns[0].musicResponsiveListItemFixedColumnRenderer.text.runs[0].text").ToTimeSpan(),
                songNumber: content.SelectObjectOptional<int>("musicResponsiveListItemRenderer.index.runs[0].text")));
        }

        return [.. result];
    }


    /// <summary>
    /// Selects and casts artist songs from a json token
    /// </summary>
    /// <param name="value">The json token containing the item data</param>
    /// <param name="path">The json token path</param>
    /// <returns>An array of artist songs</returns>
    public static ArtistSong[] SelectArtistSongs(
        this JToken value,
        string path)
    {
        List<ArtistSong> result = [];
        foreach (JToken content in value.SelectObject<JToken[]>(path))
            result.Add(new(
                name: content.SelectObject<string>("musicResponsiveListItemRenderer.flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text"),
                id: content.SelectObject<string>("musicResponsiveListItemRenderer.flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].navigationEndpoint.watchEndpoint.videoId"),
                artists: content.SelectArtists("musicResponsiveListItemRenderer.flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs"),
                album: content.SelectNamedEntity("musicResponsiveListItemRenderer.flexColumns[3].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text", "musicResponsiveListItemRenderer.flexColumns[3].musicResponsiveListItemFlexColumnRenderer.text.runs[0].navigationEndpoint.browseEndpoint.browseId"),
                playsinfo: content.SelectObject<string>("musicResponsiveListItemRenderer.flexColumns[2].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text"),
                thumbnails: content.SelectThumbnails("musicResponsiveListItemRenderer.thumbnail.musicThumbnailRenderer.thumbnail.thumbnails")));

        return [.. result];
    }

    /// <summary>
    /// Selects and casts artist albums from a json token
    /// </summary>
    /// <param name="value">The json token containing the item data</param>
    /// <param name="path">The json token path</param>
    /// <returns>An array of artist albums</returns>
    public static ArtistAlbum[] SelectArtistAlbums(
        this JToken value,
        string path)
    {
        List<ArtistAlbum> result = [];
        foreach (JToken content in value.SelectObject<JToken[]>(path))
        {
            string kind = content.SelectObject<string>("musicTwoRowItemRenderer.subtitle.runs[0].text");
            bool isAlbum = kind == "Album";
            bool isEp = kind == "EP";

            result.Add(new(
                name: content.SelectObject<string>("musicTwoRowItemRenderer.title.runs[0].text"),
                id: content.SelectObject<string>("musicTwoRowItemRenderer.title.runs[0].navigationEndpoint.browseEndpoint.browseId"),
                releaseYear: content.SelectObject<int>($"musicTwoRowItemRenderer.subtitle.runs[{(isAlbum || isEp ? "2" : "0")}].text"),
                isSingle: !(isAlbum || isEp),
                isEp: isEp,
                isExplicit: content.SelectObjectOptional<JToken[]>("musicTwoRowItemRenderer.subtitleBadges")?.Any(badge => badge.SelectToken("musicInlineBadgeRenderer.icon.iconType")?.ToString() == "MUSIC_EXPLICIT_BADGE") ?? false,
                thumbnails: content.SelectThumbnails("musicTwoRowItemRenderer.thumbnailRenderer.musicThumbnailRenderer.thumbnail.thumbnails")));
        }

        return [.. result];
    }

    /// <summary>
    /// Selects and casts artist videos from a json token
    /// </summary>
    /// <param name="value">The json token containing the item data</param>
    /// <param name="path">The json token path</param>
    /// <returns>An array of artist videos</returns>
    public static ArtistVideo[] SelectArtistVideos(
        this JToken value,
        string path)
    {
        List<ArtistVideo> result = [];
        foreach (JToken content in value.SelectObject<JToken[]>(path))
        {
            NamedEntity[] artists = content.SelectArtists("musicTwoRowItemRenderer.subtitle.runs", 0, 2);

            result.Add(new(
                name: content.SelectObject<string>("musicTwoRowItemRenderer.title.runs[0].text"),
                id: content.SelectObject<string>("musicTwoRowItemRenderer.navigationEndpoint.watchEndpoint.videoId"),
                artists: artists,
                viewsInfo: content.SelectObject<string>($"musicTwoRowItemRenderer.subtitle.runs[{artists.Length * 2}].text"),
                thumbnails: content.SelectThumbnails("musicTwoRowItemRenderer.thumbnailRenderer.musicThumbnailRenderer.thumbnail.thumbnails")));
        }

        return [.. result];
    }

    /// <summary>
    /// Selects and casts artist featured on playlists from a json token
    /// </summary>
    /// <param name="value">The json token containing the item data</param>
    /// <param name="path">The json token path</param>
    /// <returns>An array of artist videos</returns>
    public static ArtistFeaturedOn[] SelectArtistFeaturedOn(
        this JToken value,
        string path)
    {
        List<ArtistFeaturedOn> result = [];
        foreach (JToken content in value.SelectObject<JToken[]>(path))
        {
            result.Add(new(
                name: content.SelectObject<string>("musicTwoRowItemRenderer.title.runs[0].text"),
                id: content.SelectObject<string>("musicTwoRowItemRenderer.navigationEndpoint.browseEndpoint.browseId"),
                creator: content.SelectNamedEntity("musicTwoRowItemRenderer.subtitle.runs[2].text", "musicTwoRowItemRenderer.subtitle.runs[2].navigationEndpoint.browseEndpoint.browseId"),
                thumbnails: content.SelectThumbnails("musicTwoRowItemRenderer.thumbnailRenderer.musicThumbnailRenderer.thumbnail.thumbnails")));
        }

        return [.. result];
    }

    /// <summary>
    /// Selects and casts related artists from a json token
    /// </summary>
    /// <param name="value">The json token containing the item data</param>
    /// <param name="path">The json token path</param>
    /// <returns>An array of artist videos</returns>
    public static ArtistsRelated[] SelectArtistRelated(
        this JToken value,
        string path)
    {
        List<ArtistsRelated> result = [];
        foreach (JToken content in value.SelectObject<JToken[]>(path))
        {
            result.Add(new(
                name: content.SelectObject<string>("musicTwoRowItemRenderer.title.runs[0].text"),
                id: content.SelectObject<string>("musicTwoRowItemRenderer.navigationEndpoint.browseEndpoint.browseId"),
                subscribersInfo: content.SelectObject<string>("musicTwoRowItemRenderer.subtitle.runs[0].text"),
                thumbnails: content.SelectThumbnails("musicTwoRowItemRenderer.thumbnailRenderer.musicThumbnailRenderer.thumbnail.thumbnails")));
        }

        return [.. result];
    }


    /// <summary>
    /// Selects and casts stream info from a json token
    /// </summary>
    /// <param name="value">The json token containing the item data</param>
    /// <param name="getUrl">The function to retrieve a formats stream url</param>
    /// <returns>An array of stream info</returns>
    public static StreamInfo[] SelectStreamInfo(
        this JToken value,
        Func<JToken, string> getUrl)
    {
        JToken[]? adaptiveFormats = value.SelectObjectOptional<JToken[]>("streamingData.adaptiveFormats");
        if (adaptiveFormats is null)
            return [];

        List<StreamInfo> result = [];
        foreach (JToken content in adaptiveFormats)
        {
            string mimeType = content.SelectObject<string>("mimeType");
            string format = mimeType.Split('/')[1].Split(';')[0];
            string codecs = mimeType.Split('"')[1];

            int itag = content.SelectObject<int>("itag");
            DateTime lastModifiedAt = content["lastModifed"] is null ? DateTime.Now : DateTimeOffset.FromUnixTimeMilliseconds(content.SelectObjectOptional<long>("lastModified") / 1000).DateTime;
            TimeSpan duration = content["approxDurationMs"] is null ? TimeSpan.MaxValue : TimeSpan.FromMilliseconds(content.SelectObject<long>("approxDurationMs"));
            long contentLength = content["contentLength"] is null ? long.MaxValue : content.SelectObject<long>("contentLength");
            int bitrate = content.SelectObject<int>("bitrate");

            string url = getUrl(content);
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                url = HttpUtility.UrlDecode(url);

            if (mimeType.StartsWith("video"))
            {
                result.Add(new VideoStreamInfo(
                    itag: itag,
                    url: url,
                    container: new(false, true, format, codecs),
                    lastModifedAt: lastModifiedAt,
                    duration: duration,
                    contentLenght: contentLength,
                    bitrate: bitrate,
                    framerate: content.SelectObject<int>("fps"),
                    quality: content.SelectObject<string>("quality"),
                    qualityLabel: content.SelectObject<string>("qualityLabel"),
                    width: content.SelectObjectOptional<int>("width"),
                    height: content.SelectObjectOptional<int>("height")));
            }
            else if (mimeType.StartsWith("audio"))
            {
                result.Add(new AudioStreamInfo(
                    itag: itag,
                    url: url,
                    container: new(true, false, format, codecs),
                    lastModifedAt: lastModifiedAt,
                    duration: duration,
                    contentLenght: contentLength,
                    bitrate: bitrate,
                    quality: content.SelectObject<string>("quality"),
                    sampleRate: content.SelectObject<int>("audioSampleRate"),
                    channels: content.SelectObject<int>("audioChannels"),
                    loudnessDb: content["loudnessDb"] is null ? 0 : content.SelectObject<double>("loudnessDb")));
            }
            else
                throw new InvalidDataException("Invalid stream mime type. Only video and audio are supported.");
        }

        return [.. result];
    }
}