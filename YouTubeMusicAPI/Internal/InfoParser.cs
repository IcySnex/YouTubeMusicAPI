using Newtonsoft.Json.Linq;
using YouTubeMusicAPI.Models.Info;
using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Internal;

/// <summary>
/// Contains methods to parse info from json tokens
/// </summary>
internal static class InfoParser
{
    /// <summary>
    /// Parses song or video info data from the json token
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>The song or video info</returns>
    /// <exception cref="ArgumentNullException">Occurs when some parsed info is null</exception>
    public static SongVideoInfo GetSongVideo(
        JObject jsonToken)
    {
        return new(
            name: jsonToken.SelectObject<string>("videoDetails.title"),
            id: jsonToken.SelectObject<string>("videoDetails.videoId"),
            description: jsonToken.SelectObject<string>("microformat.microformatDataRenderer.description"),
            artists: jsonToken.SelectArtistsSimple(),
            duration: TimeSpan.FromSeconds(jsonToken.SelectObject<int>("videoDetails.lengthSeconds")),
            isRatingsAllowed: jsonToken.SelectObject<bool>("videoDetails.allowRatings"),
            isPrivate: jsonToken.SelectObject<bool>("videoDetails.isPrivate"),
            isUnlisted: jsonToken.SelectObject<bool>("microformat.microformatDataRenderer.unlisted"),
            isUnpluggedCorpus: jsonToken.SelectObject<bool>("videoDetails.isUnpluggedCorpus"),
            isLiveContent: jsonToken.SelectObject<bool>("videoDetails.isLiveContent"),
            isFamiliyFriendly: jsonToken.SelectObject<bool>("microformat.microformatDataRenderer.familySafe"),
            viewsCount: jsonToken.SelectObject<int>("videoDetails.viewCount"),
            publishedAt: jsonToken.SelectObject<DateTime>("microformat.microformatDataRenderer.publishDate"),
            uploadedAt: jsonToken.SelectObject<DateTime>("microformat.microformatDataRenderer.uploadDate"),
            thumbnails: jsonToken.SelectThumbnails("videoDetails.thumbnail.thumbnails"),
            tags: jsonToken.SelectObject<string[]>("microformat.microformatDataRenderer.tags"));
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
        JToken innerJsonToken = jsonToken.SelectRequieredToken("contents.twoColumnBrowseResultsRenderer.tabs[0].tabRenderer.content.sectionListRenderer.contents[0].musicResponsiveHeaderRenderer");

        return new(
            name: innerJsonToken.SelectObject<string>("title.runs[0].text"),
            id: innerJsonToken.SelectObject<string>("buttons[1].musicPlayButtonRenderer.playNavigationEndpoint.watchEndpoint.playlistId"),
            description: innerJsonToken.SelectObjectOptional<JToken[]>("description.musicDescriptionShelfRenderer.description.runs")?.Aggregate("", (desc, run) => desc + run.SelectObjectOptional<string>("text")),
            artists: innerJsonToken.SelectArtists("straplineTextOne.runs"),
            duration: innerJsonToken.SelectObject<string>("secondSubtitle.runs[2].text").ToTimeSpanLong(),
            songCount: int.Parse(innerJsonToken.SelectObject<string>("secondSubtitle.runs[0].text").Split(' ')[0]),
            releaseYear: innerJsonToken.SelectObject<int>("subtitle.runs[2].text"),
            isSingle: innerJsonToken.SelectObject<string>("subtitle.runs[0].text") == "Single",
            isEp: innerJsonToken.SelectObject<string>("subtitle.runs[0].text") == "EP",
            thumbnails: innerJsonToken.SelectThumbnails(),
            songs: jsonToken.SelectAlbumSongs("contents.twoColumnBrowseResultsRenderer.secondaryContents.sectionListRenderer.contents[0].musicShelfRenderer.contents"));
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
        JToken innerJsonToken = jsonToken.SelectRequieredToken("contents.twoColumnBrowseResultsRenderer.tabs[0].tabRenderer.content.sectionListRenderer.contents[0].musicResponsiveHeaderRenderer");

        JToken[] runs = innerJsonToken.SelectObject<JToken[]>("secondSubtitle.runs");

        return new(
            name: innerJsonToken.SelectObject<string>("title.runs[0].text"),
            id: innerJsonToken.SelectObject<string>("buttons[1].musicPlayButtonRenderer.playNavigationEndpoint.watchEndpoint.playlistId"),
            description: innerJsonToken.SelectObjectOptional<JToken[]>("description.musicDescriptionShelfRenderer.description.runs")?.Aggregate("", (desc, run) => desc + run.SelectObjectOptional<string>("text")),
            creator: innerJsonToken.SelectSehlfItem("straplineTextOne.runs[0].text", "straplineTextOne.runs[0].navigationEndpoint.browseEndpoint.browseId", ShelfKind.Profiles),
            viewsInfo: runs.Length - 5 < 0 ? null : innerJsonToken.SelectObjectOptional<string>("secondSubtitle.runs[0].text"),
            duration: innerJsonToken.SelectObject<string>($"secondSubtitle.runs[{runs.Length - 1}].text").ToTimeSpanLong(),
            songCount: int.Parse(innerJsonToken.SelectObject<string>($"secondSubtitle.runs[{runs.Length - 3}].text").Split(' ')[0]),
            creationYear: innerJsonToken.SelectObject<int>("subtitle.runs[2].text"),
            thumbnails: innerJsonToken.SelectThumbnails(),
            songs: jsonToken.SelectCommunityPlaylistSongs("contents.twoColumnBrowseResultsRenderer.secondaryContents.sectionListRenderer.contents[0].musicPlaylistShelfRenderer.contents"));
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
        JToken innerJsonToken = jsonToken.SelectRequieredToken("header.musicImmersiveHeaderRenderer");
        JToken[] contents = jsonToken.SelectObject<JToken[]>("contents.singleColumnBrowseResultsRenderer.tabs[0].tabRenderer.content.sectionListRenderer.contents");

        string? allSongsPlaylistId = null;
        ArtistSongInfo[] songs = [];
        List<ArtistAlbumInfo> albums = [];
        ArtistVideoInfo[] videos = [];
        ArtistFeaturedOnInfo[] featuredOns = [];
        ArtistsRelatedInfo[] related = [];

        string? viewsInfo = null;

        foreach (JToken content in contents)
        {
            if (content.SelectObjectOptional<string>("musicShelfRenderer.title.runs[0].text") == "Songs")
            {
                songs = content.SelectArtistSongs("musicShelfRenderer.contents");
                allSongsPlaylistId = content.SelectObjectOptional<string>("musicShelfRenderer.bottomEndpoint.browseEndpoint.browseId");
                continue;
            }
            if (content.SelectObjectOptional<string>("musicDescriptionShelfRenderer.header.runs[0].text") == "About")
            {
                viewsInfo = content.SelectObjectOptional<string>("musicDescriptionShelfRenderer.subheader.runs[0].text");
                continue;
            }

            switch (content.SelectObjectOptional<string>("musicCarouselShelfRenderer.header.musicCarouselShelfBasicHeaderRenderer.title.runs[0].text"))
            {
                case "Albums":
                case "Singles":
                    albums.AddRange(content.SelectArtistAlbums("musicCarouselShelfRenderer.contents"));
                    break;
                case "Videos":
                    videos = content.SelectArtistVideos("musicCarouselShelfRenderer.contents");
                    break;
                case "Featured on":
                    featuredOns = content.SelectArtistFeaturedOn("musicCarouselShelfRenderer.contents");
                    break;
                case "Fans might also like":
                    related = content.SelectArtistRelated("musicCarouselShelfRenderer.contents");
                    break;
            }
        }

        return new(
            name: innerJsonToken.SelectObject<string>("title.runs[0].text"),
            id: jsonToken.SelectObject<string>("responseContext.serviceTrackingParams[0].params[2].value"),
            description: innerJsonToken.SelectObjectOptional<string>("description.runs[0].text"),
            subscribersInfo: innerJsonToken.SelectObjectOptional<string>("subscriptionButton.subscribeButtonRenderer.longSubscriberCountText.runs[0].text"),
            viewsInfo: viewsInfo,
            thumbnails: innerJsonToken.SelectThumbnails(),
            allSongsPlaylistId: allSongsPlaylistId is null ? null : allSongsPlaylistId.StartsWith("VL") ? allSongsPlaylistId.Substring(2) : allSongsPlaylistId,
            songs: songs,
            albums: [.. albums],
            videos: videos,
            featuredOns: featuredOns,
            related: related);
    }
}