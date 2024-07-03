using Newtonsoft.Json.Linq;
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
    /// Parses song info data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>The song info</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static SongInfo GetSong(
        JObject jsonToken)
    {
        // Parse info from json token
        Thumbnail[] thumbnails = Parser.GetThumbnails(jsonToken, "videoDetails.thumbnail.thumbnails");
        ShelfItem[] artists = Parser.GetArtistsSimple(jsonToken);

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
                    Parser.TimeSpanExact(duration),
                    string.IsNullOrWhiteSpace(index) ? null : int.Parse(index)));
            }

            // Return result
            return [.. result];
        }


        JToken token = jsonToken.SelectToken("contents.twoColumnBrowseResultsRenderer.tabs[0].tabRenderer.content.sectionListRenderer.contents[0].musicResponsiveHeaderRenderer") ?? throw new ArgumentNullException(null, "One or more values of item is null");

        // Parse info from json token
        Thumbnail[] thumbnails = Parser.GetThumbnails(token);
        ShelfItem[] artists = Parser.GetArtists(token, "straplineTextOne.runs");
        AlbumSongInfo[] songs = GetSongs(jsonToken);

        string? name = token.SelectToken("title.runs[0].text")?.ToString();
        string? id = token.SelectToken("buttons[1].musicPlayButtonRenderer.playNavigationEndpoint.watchEndpoint.playlistId")?.ToString();
        JToken[]? description = token.SelectToken("description.musicDescriptionShelfRenderer.description.runs")?.ToArray();
        string? kind = token.SelectToken("subtitle.runs[0].text")?.ToString();
        string? year = token.SelectToken("subtitle.runs[2].text")?.ToString();
        string? songCount = token.SelectToken("secondSubtitle.runs[0].text")?.ToString();
        string? totalDuration = token.SelectToken("secondSubtitle.runs[2].text")?.ToString();

        if (name is null || id is null || kind is null || year is null || songCount is null || totalDuration is null)
            throw new ArgumentNullException(null, "One or more values of item is null.");

        return new AlbumInfo(
            name,
            id,
            description is not null ? string.Join("", description.Select(run => run.SelectToken("text")?.ToString())) : null,
            artists,
            Parser.TimeSpanLong(totalDuration),
            int.Parse(songCount.Split(' ')[0]),
            int.Parse(year),
            kind == "Single",
            kind == "EP",
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
            // Parse runs from json token
            JToken[] contents = jsonToken.SelectToken("contents.twoColumnBrowseResultsRenderer.secondaryContents.sectionListRenderer.contents[0].musicPlaylistShelfRenderer.contents")?.ToArray() ?? throw new ArgumentNullException(null, "One or more values of item is null");

            List<CommunityPlaylistSongInfo> result = [];
            foreach (JToken content in contents)
            {
                ShelfItem[] artists = Parser.GetArtists(content, "musicResponsiveListItemRenderer.flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs");
                Thumbnail[] thumbnails = Parser.GetThumbnails(content, "musicResponsiveListItemRenderer.thumbnail.musicThumbnailRenderer.thumbnail.thumbnails");

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
                    Parser.TimeSpanExact(duration),
                    thumbnails));
            }

            // Return result
            return [.. result];
        }


        JToken token = jsonToken.SelectToken("contents.twoColumnBrowseResultsRenderer.tabs[0].tabRenderer.content.sectionListRenderer.contents[0].musicResponsiveHeaderRenderer") ?? throw new ArgumentNullException(null, "One or more values of item is null");

        // Parse info from json token
        Thumbnail[] thumbnails = Parser.GetThumbnails(token);
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
            Parser.TimeSpanLong(totalDuration),
            int.Parse(songCount.Split(' ')[0]),
            int.Parse(year),
            thumbnails,
            songs);
    }

    /// <summary>
    /// Parses artist info data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>The artist info</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static ArtistInfo GetArtist(
        JObject jsonToken)
    {
        static ArtistSongInfo[] GetSongs(
            JToken jsonToken)
        {
            JToken[] songsContainer = jsonToken.SelectToken("musicShelfRenderer.contents")?.ToArray() ?? throw new ArgumentNullException(null, "One or more values of item is null");

            List<ArtistSongInfo> result = [];
            foreach (JToken song in songsContainer)
            {
                // Parse info from songs 
                Thumbnail[] thumbnails = Parser.GetThumbnails(song, "musicResponsiveListItemRenderer.thumbnail.musicThumbnailRenderer.thumbnail.thumbnails");
                ShelfItem[] artists = Parser.GetArtists(song, "musicResponsiveListItemRenderer.flexColumns[1].musicResponsiveListItemFlexColumnRenderer.text.runs");

                string? name = song.SelectToken("musicResponsiveListItemRenderer.flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text")?.ToString();
                string? id = song.SelectToken("musicResponsiveListItemRenderer.flexColumns[0].musicResponsiveListItemFlexColumnRenderer.text.runs[0].navigationEndpoint.watchEndpoint.videoId")?.ToString();
                string? album = song.SelectToken("musicResponsiveListItemRenderer.flexColumns[3].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text")?.ToString();
                string? albumId = song.SelectToken("musicResponsiveListItemRenderer.flexColumns[3].musicResponsiveListItemFlexColumnRenderer.text.runs[0].navigationEndpoint.browseEndpoint.browseId")?.ToString();
                string? playsInfo = song.SelectToken("musicResponsiveListItemRenderer.flexColumns[2].musicResponsiveListItemFlexColumnRenderer.text.runs[0].text")?.ToString();

                if (name is null || id is null || album is null || albumId is null || playsInfo is null)
                    throw new ArgumentNullException(null, "One or more values of item is null.");

                result.Add(new(
                    name,
                    id,
                    artists,
                    new(album, albumId, ShelfKind.Albums),
                    playsInfo,
                    thumbnails));
            }

            // Return result
            return [.. result];
        }

        static ArtistAlbumInfo[] GetAlbums(
            JToken jsonToken)
        {
            JToken[] albumContainer = jsonToken.SelectToken("musicCarouselShelfRenderer.contents")?.ToArray() ?? throw new ArgumentNullException(null, "One or more values of item is null");

            List<ArtistAlbumInfo> result = [];
            foreach (JToken album in albumContainer)
            {
                // Parse info from songs 
                Thumbnail[] thumbnails = Parser.GetThumbnails(album, "musicTwoRowItemRenderer.thumbnailRenderer.musicThumbnailRenderer.thumbnail.thumbnails");

                string? name = album.SelectToken("musicTwoRowItemRenderer.title.runs[0].text")?.ToString();
                string? id = album.SelectToken("musicTwoRowItemRenderer.title.runs[0].navigationEndpoint.browseEndpoint.browseId")?.ToString();
                string? kind = album.SelectToken("musicTwoRowItemRenderer.subtitle.runs[0].text")?.ToString();
                bool isAlbum = kind == "Album";
                bool isEp = kind == "EP";
                string? year = album.SelectToken($"musicTwoRowItemRenderer.subtitle.runs[{(isAlbum || isEp ? "2" : "0")}].text")?.ToString();
                JToken[]? isExplicit = album.SelectToken("musicTwoRowItemRenderer.subtitleBadges")?.ToArray();

                if (name is null || id is null || year is null)
                    throw new ArgumentNullException(null, "One or more values of item is null.");

                result.Add(new(
                    name,
                    id,
                    int.Parse(year),
                    !(isAlbum || isEp),
                    isEp,
                    isExplicit?.Any(badge => badge.SelectToken("musicInlineBadgeRenderer.icon.iconType")?.ToString() == "MUSIC_EXPLICIT_BADGE") ?? false,
                    thumbnails));
            }

            // Return result
            return [.. result];
        }

        static ArtistVideoInfo[] GetVideos(
            JToken jsonToken)
        {
            JToken[] videoContainer = jsonToken.SelectToken("musicCarouselShelfRenderer.contents")?.ToArray() ?? throw new ArgumentNullException(null, "One or more values of item is null");

            List<ArtistVideoInfo> result = [];
            foreach (JToken video in videoContainer)
            {
                // Parse info from songs 
                Thumbnail[] thumbnails = Parser.GetThumbnails(video, "musicTwoRowItemRenderer.thumbnailRenderer.musicThumbnailRenderer.thumbnail.thumbnails");
                ShelfItem[] artists = Parser.GetArtists(video, "musicTwoRowItemRenderer.subtitle.runs", 0, 2);

                string? name = video.SelectToken("musicTwoRowItemRenderer.title.runs[0].text")?.ToString();
                string? id = video.SelectToken("musicTwoRowItemRenderer.navigationEndpoint.watchEndpoint.videoId")?.ToString();
                string? viewsInfo = video.SelectToken($"musicTwoRowItemRenderer.subtitle.runs[{artists.Length * 2}].text")?.ToString();

                if (name is null || id is null || viewsInfo is null)
                    throw new ArgumentNullException(null, "One or more values of item is null.");

                result.Add(new(
                    name,
                    id,
                    artists,
                    viewsInfo,
                    thumbnails));
            }

            // Return result
            return [.. result];
        }


        JToken token = jsonToken.SelectToken("header.musicImmersiveHeaderRenderer") ?? throw new ArgumentNullException(null, "One or more values of item is null");
        JToken[] contents = jsonToken.SelectToken("contents.singleColumnBrowseResultsRenderer.tabs[0].tabRenderer.content.sectionListRenderer.contents")?.ToArray() ?? throw new ArgumentNullException(null, "One or more values of item is null");

        // Parse info from json token
        Thumbnail[] thumbnails = Parser.GetThumbnails(token);

        string? name = token.SelectToken("title.runs[0].text")?.ToString();
        string? id = jsonToken.SelectToken("responseContext.serviceTrackingParams[0].params[2].value")?.ToString();
        string? subscriberCount = token.SelectToken("subscriptionButton.subscribeButtonRenderer.subscriberCountText.runs[0].text")?.ToString();
        string? description = token.SelectToken("description.runs[0].text")?.ToString();

        if (name is null || id is null)
            throw new ArgumentNullException(null, "One or more values of item is null.");

        string? allSongsPlaylistId = null;
        ArtistSongInfo[] songs = [];
        List<ArtistAlbumInfo> albums = [];
        ArtistVideoInfo[] videos = [];

        string? viewsInfo = null;

        foreach (JToken content in contents)
        {
            if (content.SelectToken("musicShelfRenderer.title.runs[0].text")?.ToString() == "Songs")
            {
                songs = GetSongs(content);
                allSongsPlaylistId = content.SelectToken("musicShelfRenderer.bottomEndpoint.browseEndpoint.browseId")?.ToString();
                continue;
            }
            if (content.SelectToken("musicDescriptionShelfRenderer.header.runs[0].text")?.ToString() == "About")
            {
                viewsInfo = content.SelectToken("musicDescriptionShelfRenderer.subheader.runs[0].text")?.ToString();
                continue;
            }

            string? kind = content.SelectToken("musicCarouselShelfRenderer.header.musicCarouselShelfBasicHeaderRenderer.title.runs[0].text")?.ToString();
            switch (kind)
            {
                case "Albums":
                case "Singles":
                    albums.AddRange(GetAlbums(content));
                    break;
                case "Videos":
                    videos = GetVideos(content);
                    break;
                case "Featured on":
                    break;
                case "Fans might also like":
                    break;
            }
        }

        // Return result
        return new(
            name,
            id,
            description,
            subscriberCount,
            viewsInfo,
            thumbnails,
            allSongsPlaylistId is null ? null : allSongsPlaylistId.StartsWith("VL") ? allSongsPlaylistId.Substring(2) : allSongsPlaylistId,
            songs,
            [.. albums],
            videos);
    }
}