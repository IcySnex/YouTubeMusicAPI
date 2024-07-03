using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Info;
using YouTubeMusicAPI.Models.Shelf;
using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Internal;

/// <summary>
/// Contains methods to parse info from json tokens
/// </summary>
internal static class InfoParser
{
    /// <summary>
    /// Parses a string into a TimeSpan
    /// </summary>
    /// <param name="value">The string to parse</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">value has an invalid format</exception>
    static TimeSpan ParseTimeSpan(
        string value)
    {
        Regex hourPattern = new(@"(\d+)\s*\+?\s*hour", RegexOptions.IgnoreCase);
        Regex minutePattern = new(@"(\d+)\s*\+?\s*minute", RegexOptions.IgnoreCase);
        Regex secondPattern = new(@"(\d+)\s*\+?\s*second", RegexOptions.IgnoreCase);

        Match hourMatch = hourPattern.Match(value);
        Match minuteMatch = minutePattern.Match(value);
        Match secondMatch = secondPattern.Match(value);

        int hours = hourMatch.Success ? int.Parse(hourMatch.Groups[1].Value) : 0;
        int minutes = minuteMatch.Success ? int.Parse(minuteMatch.Groups[1].Value) : 0;
        int seconds = secondMatch.Success ? int.Parse(secondMatch.Groups[1].Value) : 0;

        return new(hours, minutes, seconds);
    }

    /// <summary>
    /// Parses a string into a TimeSpan
    /// </summary>
    /// <param name="value">The string to parse</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">value has an invalid format</exception>
    static TimeSpan ParseTimeSpanExact(
        string value)
    {
        if (TimeSpan.TryParseExact(value, @"m\:ss", null, out TimeSpan timeSpan))
            return timeSpan;
        if (TimeSpan.TryParseExact(value, @"mm\:ss", null, out timeSpan))
            return timeSpan;
        if (TimeSpan.TryParseExact(value, @"h\:mm\:ss", null, out timeSpan))
            return timeSpan;
        if (TimeSpan.TryParseExact(value, @"hh\:mm\:ss", null, out timeSpan))
            return timeSpan;

        throw new ArgumentException("value has an invalid format");
    }


    static Thumbnail[] GetThumbnails(
        JToken jsonToken)
    {
        // Parse thumbnails container from json token
        JToken? thumbnails = jsonToken.SelectToken("thumbnail.musicThumbnailRenderer.thumbnail.thumbnails");
        if (thumbnails is null)
            return [];

        List<Thumbnail> result = [];
        foreach (JToken thumbnail in thumbnails)
        {
            // Parse info from thumbnails container
            string? url = thumbnail.SelectToken("url")?.ToString();
            string? width = thumbnail.SelectToken("width")?.ToString();
            string? height = thumbnail.SelectToken("height")?.ToString();

            if (url is null)
                continue;

            result.Add(new(url, width is null ? 0 : int.Parse(width), height is null ? 0 : int.Parse(height)));
        }

        // Return result
        return [.. result];
    }


    /// <summary>
    /// Parses song info data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>The song info</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static SongInfo GetSong(
        JObject jsonToken)
    {
        static Thumbnail[] GetThumbnails(
            JToken jsonToken)
        {
            // Parse thumbnails container from json token
            JToken? thumbnails = jsonToken.SelectToken("videoDetails.thumbnail.thumbnails");
            if (thumbnails is null)
                return [];

            List<Thumbnail> result = [];
            foreach (JToken thumbnail in thumbnails)
            {
                // Parse info from thumbnails container
                string? url = thumbnail.SelectToken("url")?.ToString();
                string? width = thumbnail.SelectToken("width")?.ToString();
                string? height = thumbnail.SelectToken("height")?.ToString();

                if (url is null)
                    continue;

                result.Add(new(url, width is null ? 0 : int.Parse(width), height is null ? 0 : int.Parse(height)));
            }

            // Return result
            return [.. result];
        }

        static ShelfItem[] GetArtists(
            JToken jsonToken)
        {
            // Parse artist names from json token
            string? artistNames = jsonToken.SelectToken("videoDetails.author")?.ToString();
            string? primaryArtistId = jsonToken.SelectToken("videoDetails.channelId")?.ToString();

            if (artistNames is null || primaryArtistId is null)
                throw new ArgumentNullException(null, "One or more values of item is null.");

            // Add artists to result
            IEnumerable<string> artists = artistNames.Split(',', '&').Where(artistName => !string.IsNullOrWhiteSpace(artistName)).Select(artistName => artistName.Trim());

            List<ShelfItem> result = [];
            result.Add(new(artists.First(), primaryArtistId, ShelfKind.Artists));
            foreach (string artist in artists.Skip(1))
            {
                result.Add(new(artist, null, ShelfKind.Artists));
            }

            // Return result
            return [.. result];
        }


        // Parse info from json token
        Thumbnail[] thumbnails = GetThumbnails(jsonToken);
        ShelfItem[] artists = GetArtists(jsonToken);

        string? name = jsonToken.SelectToken("videoDetails.title")?.ToString();
        string? id = jsonToken.SelectToken("videoDetails.videoId")?.ToString();
        string? duration = jsonToken.SelectToken("videoDetails.lengthSeconds")?.ToString();
        string? isOwnerViewing = jsonToken.SelectToken("videoDetails.isOwnerViewing")?.ToString();
        string? isCrawlable = jsonToken.SelectToken("videoDetails.isCrawlable")?.ToString();
        string? allowRatings = jsonToken.SelectToken("videoDetails.allowRatings")?.ToString();
        string? viewCount = jsonToken.SelectToken("videoDetails.viewCount")?.ToString();
        string? isPrivate = jsonToken.SelectToken("videoDetails.isPrivate")?.ToString();
        string? isUnpluggedCorpus = jsonToken.SelectToken("videoDetails.isUnpluggedCorpus")?.ToString();
        string? isLiveContent = jsonToken.SelectToken("videoDetails.isLiveContent")?.ToString();

        string? description = jsonToken.SelectToken("microformat.microformatDataRenderer.description")?.ToString();
        string? unlisted = jsonToken.SelectToken("microformat.microformatDataRenderer.unlisted")?.ToString();
        string? familySafe = jsonToken.SelectToken("microformat.microformatDataRenderer.familySafe")?.ToString();
        string? publishDate = jsonToken.SelectToken("microformat.microformatDataRenderer.publishDate")?.ToString();
        string? uploadDate = jsonToken.SelectToken("microformat.microformatDataRenderer.uploadDate")?.ToString();
        JToken[]? tags = jsonToken.SelectToken("microformat.microformatDataRenderer.tags")?.ToArray();
        JToken[]? availableCountries = jsonToken.SelectToken("microformat.microformatDataRenderer.availableCountries")?.ToArray();

        if (name is null || id is null || duration is null || isOwnerViewing is null || isCrawlable is null || allowRatings is null || viewCount is null || isPrivate is null || isUnpluggedCorpus is null || isLiveContent is null || description is null || unlisted is null || familySafe is null || publishDate is null || uploadDate is null)
            throw new ArgumentNullException(null, "One or more values of item is null.");

        return new(
            name,
            id,
            description,
            artists,
            TimeSpan.FromSeconds(int.Parse(duration)),
            bool.Parse(isOwnerViewing),
            bool.Parse(isCrawlable),
            bool.Parse(allowRatings),
            bool.Parse(isPrivate),
            bool.Parse(unlisted),
            bool.Parse(isUnpluggedCorpus),
            bool.Parse(isLiveContent),
            bool.Parse(familySafe),
            int.Parse(viewCount),
            DateTime.Parse(publishDate),
            DateTime.Parse(uploadDate),
            thumbnails,
            tags is null ? [] : tags.Select(tag => tag.ToString()).ToArray(),
            availableCountries is null ? [] : availableCountries.Select(country => country.ToString()).ToArray());
    }

    /// <summary>
    /// Parses album info data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>The album info</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static AlbumInfo GetAlbum(
        JObject jsonToken)
    {        
        static ShelfItem[] GetArtists(
            JToken jsonToken)
        {
            // Parse runs from json token
            JToken[] runs = jsonToken.SelectToken("straplineTextOne.runs")?.ToArray() ?? throw new ArgumentNullException(null, "One or more values of item is null");

            List<ShelfItem> result = [];
            foreach (JToken run in runs)
            {
                // Parse info from artist container
                string? artist = run.SelectToken("text")?.ToString()?.Trim();
                string? artistId = run.SelectToken("navigationEndpoint.browseEndpoint.browseId")?.ToString();

                // Skip tokens that are just separators (e.g., ", " or " & ")
                if (artist is null || artist == "," || artist == "&")
                    continue;

                // Add the artist to the result list
                result.Add(new(artist, artistId, ShelfKind.Artists));
            }

            // Return result
            return [.. result];
        }

        static AlbumSongInfo[] GetSongs(
            JToken jsonToken)
        {
            // Parse runs from json token
            JToken[] contents = jsonToken.SelectToken("contents.twoColumnBrowseResultsRenderer.secondaryContents.sectionListRenderer.contents[0].musicShelfRenderer.contents")?.ToArray() ?? throw new ArgumentNullException(null, "One or more values of item is null");

            List<AlbumSongInfo> result = [];
            foreach (JToken content in contents)
            {
                string? name = content.SelectToken("musicResponsiveListItemRenderer.flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text")?.ToString();
                string? id = content.SelectToken("musicResponsiveListItemRenderer.flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].navigationEndpoint.watchEndpoint.videoId")?.ToString();
                string? playsInfo = content.SelectToken("musicResponsiveListItemRenderer.flexColumns[2].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text")?.ToString();
                string? duration = content.SelectToken("musicResponsiveListItemRenderer.fixedColumns[0].musicResponsiveListItemFixedColumnRenderer.text.runs[0].text")?.ToString();
                JToken[]? isExplicit = content.SelectToken("musicResponsiveListItemRenderer.badges")?.ToArray();
                string? index = content.SelectToken("musicResponsiveListItemRenderer.index.runs[0].text")?.ToString();

                if (name is null || duration is null || index is null)
                    throw new ArgumentNullException(null, "One or more values of item is null");

                result.Add(new(
                    name,
                    id,
                    isExplicit?.Any(badge => badge.SelectToken("musicInlineBadgeRenderer.icon.iconType")?.ToString() == "MUSIC_EXPLICIT_BADGE") ?? false,
                    playsInfo,
                    ParseTimeSpanExact(duration),
                    string.IsNullOrWhiteSpace(index) ? null : int.Parse(index)));
            }

            // Return result
            return [.. result];
        }


        JToken token = jsonToken.SelectToken("contents.twoColumnBrowseResultsRenderer.tabs[0].tabRenderer.content.sectionListRenderer.contents[0].musicResponsiveHeaderRenderer") ?? throw new ArgumentNullException(null, "One or more values of item is null");

        // Parse info from json token
        Thumbnail[] thumbnails = GetThumbnails(token);
        ShelfItem[] artists = GetArtists(token);
        AlbumSongInfo[] songs = GetSongs(jsonToken);

        string? name = token.SelectToken("title.runs[0].text")?.ToString();
        string? id = token.SelectToken("buttons[1].musicPlayButtonRenderer.playNavigationEndpoint.watchEndpoint.playlistId")?.ToString();
        JToken[]? description = token.SelectToken("description.musicDescriptionShelfRenderer.description.runs")?.ToArray();
        string? isSingle = token.SelectToken("subtitle.runs[0].text")?.ToString();
        string? year = token.SelectToken("subtitle.runs[2].text")?.ToString();
        string? songCount = token.SelectToken("secondSubtitle.runs[0].text")?.ToString();
        string? totalDuration = token.SelectToken("secondSubtitle.runs[2].text")?.ToString();

        if (name is null || id is null || isSingle is null || year is null || songCount is null || totalDuration is null)
            throw new ArgumentNullException(null, "One or more values of item is null.");

        return new AlbumInfo(
            name,
            id,
            description is not null ? string.Join("", description.Select(run => run.SelectToken("text")?.ToString())) : null,
            artists,
            ParseTimeSpan(totalDuration),
            int.Parse(songCount.Split(' ')[0]),
            int.Parse(year),
            isSingle == "Single",
            thumbnails,
            songs);
    }

    /// <summary>
    /// Parses community playlist info data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>The community playlist info</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static CommunityPlaylistInfo GetCommunityPlaylist(
        JObject jsonToken)
    {
        static CommunityPlaylistSongInfo[] GetSongs(
            JToken jsonToken)
        {
            static Thumbnail[] GetThumbnails(
                JToken jsonToken)
            {
                // Parse thumbnails container from json token
                JToken? thumbnails = jsonToken.SelectToken("musicResponsiveListItemRenderer.thumbnail.musicThumbnailRenderer.thumbnail.thumbnails");
                if (thumbnails is null)
                    return [];

                List<Thumbnail> result = [];
                foreach (JToken thumbnail in thumbnails)
                {
                    // Parse info from thumbnails container
                    string? url = thumbnail.SelectToken("url")?.ToString();
                    string? width = thumbnail.SelectToken("width")?.ToString();
                    string? height = thumbnail.SelectToken("height")?.ToString();

                    if (url is null)
                        continue;

                    result.Add(new(url, width is null ? 0 : int.Parse(width), height is null ? 0 : int.Parse(height)));
                }

                // Return result
                return [.. result];
            }

            static ShelfItem[] GetArtists(
                JToken jsonToken)
            {
                // Parse runs from json token
                JToken[] runs = jsonToken.SelectToken("musicResponsiveListItemRenderer.flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs")?.ToArray() ?? throw new ArgumentNullException(null, "One or more values of item is null");

                List<ShelfItem> result = [];
                foreach (JToken run in runs)
                {
                    // Parse info from artist container
                    string? artist = run.SelectToken("text")?.ToString()?.Trim();
                    string? artistId = run.SelectToken("navigationEndpoint.browseEndpoint.browseId")?.ToString();

                    // Skip tokens that are just separators (e.g., ", " or " & ")
                    if (artist is null || artist == "," || artist == "&")
                        continue;

                    // Add the artist to the result list
                    result.Add(new(artist, artistId, ShelfKind.Artists));
                }

                // Return result
                return [.. result];
            }


            // Parse runs from json token
            JToken[] contents = jsonToken.SelectToken("contents.twoColumnBrowseResultsRenderer.secondaryContents.sectionListRenderer.contents[0].musicPlaylistShelfRenderer.contents")?.ToArray() ?? throw new ArgumentNullException(null, "One or more values of item is null");

            List<CommunityPlaylistSongInfo> result = [];
            foreach (JToken content in contents)
            {
                ShelfItem[] artists = GetArtists(content);
                Thumbnail[] thumbnails = GetThumbnails(content);

                string? name = content.SelectToken("musicResponsiveListItemRenderer.flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text")?.ToString();
                string? id = content.SelectToken("musicResponsiveListItemRenderer.flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].navigationEndpoint.watchEndpoint.videoId")?.ToString();
                string? album = content.SelectToken("musicResponsiveListItemRenderer.flexColumns[2].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text")?.ToString();
                string? albumId = content.SelectToken("musicResponsiveListItemRenderer.flexColumns[2].musicResponsiveListItemFlexColumnRenderer.text.runs[0].navigationEndpoint.browseEndpoint.browseId")?.ToString();
                string? duration = content.SelectToken("musicResponsiveListItemRenderer.fixedColumns[0].musicResponsiveListItemFixedColumnRenderer.text.runs[0].text")?.ToString();
                JToken[]? isExplicit = content.SelectToken("musicResponsiveListItemRenderer.badges")?.ToArray();

                if (name is null || duration is null)
                    throw new ArgumentNullException(null, "One or more values of item is null");

                result.Add(new(
                    name,
                    id,
                    artists,
                    album is null ? null : new(album, albumId, ShelfKind.Albums),
                    isExplicit?.Any(badge => badge.SelectToken("musicInlineBadgeRenderer.icon.iconType")?.ToString() == "MUSIC_EXPLICIT_BADGE") ?? false,
                    ParseTimeSpanExact(duration),
                    thumbnails));
            }

            // Return result
            return [.. result];
        }


        JToken token = jsonToken.SelectToken("contents.twoColumnBrowseResultsRenderer.tabs[0].tabRenderer.content.sectionListRenderer.contents[0].musicResponsiveHeaderRenderer") ?? throw new ArgumentNullException(null, "One or more values of item is null");

        // Parse info from json token
        Thumbnail[] thumbnails = GetThumbnails(token);
        CommunityPlaylistSongInfo[] songs = GetSongs(jsonToken);

        string? name = token.SelectToken("title.runs[0].text")?.ToString();
        string? id = token.SelectToken("buttons[1].musicPlayButtonRenderer.playNavigationEndpoint.watchEndpoint.playlistId")?.ToString();
        string? creator = token.SelectToken("straplineTextOne.runs[0].text")?.ToString()?.Trim();
        string? creatorId = token.SelectToken("straplineTextOne.runs[0].navigationEndpoint.browseEndpoint.browseId")?.ToString();
        JToken[]? description = token.SelectToken("description.musicDescriptionShelfRenderer.description.runs")?.ToArray();
        string? year = token.SelectToken("subtitle.runs[2].text")?.ToString();
        string? viewsInfo = token.SelectToken("secondSubtitle.runs[0].text")?.ToString();
        string? songCount = token.SelectToken("secondSubtitle.runs[2].text")?.ToString();
        string? totalDuration = token.SelectToken("secondSubtitle.runs[4].text")?.ToString();

        if (name is null || id is null || creator is null || year is null || viewsInfo is null || songCount is null || totalDuration is null)
            throw new ArgumentNullException(null, "One or more values of item is null.");

        return new CommunityPlaylistInfo(
            name,
            id,
            description is not null ? string.Join("", description.Select(run => run.SelectToken("text")?.ToString())) : null,
            new(creator, creatorId, ShelfKind.Artists),
            viewsInfo,
            ParseTimeSpan(totalDuration),
            int.Parse(songCount.Split(' ')[0]),
            int.Parse(year),
            thumbnails,
            songs);
    }
}