using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Services.Albums;
using YouTubeMusicAPI.Services.Musical.Songs;
using YouTubeMusicAPI.Services.Musical.Videos;
using YouTubeMusicAPI.Services.Playlists;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Artists;

/// <summary>
/// Represents an artist on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="ArtistInfo"/> class.
/// </remarks>
/// <param name="name">The name of this artist.</param>
/// <param name="id">The ID of this artist.</param>
/// <param name="browseId">The browse ID of this artist.</param>
/// <param name="thumbnails">The thumbnails of this artist.</param>
/// <param name="description">The description of this artist.</param>
/// <param name="monthlyListenersInfo">The information about the monthly listeners of this artist.</param>
/// <param name="subscribersInfo">The information about the numbers of subscribers this artist has.</param>
/// <param name="viewsInfo">The information about the numbers of views this artist has.</param>
/// <param name="pronouns">The pronouns of this artist, if available.</param>
/// <param name="radio">The radio associated with this artist, if available.</param>
/// <param name="topSongs">The top songs of this artist.</param>
/// <param name="albums">Some of the recent albums of this artist.</param>
/// <param name="singlesAndEps">Some of the recent singles and EPs of this artist.</param>
/// <param name="videos">Some of the recent and popular videos of this artist.</param>
/// <param name="livePerformances">Some of the recent and popular live performances of this artist.</param>
/// <param name="featuredOn">Some of the playlists this artists is featured on.</param>
/// <param name="playlists">Some of the playlists created by this artists.</param>
/// <param name="fansMightAlsoLike">The correlations to this artists which fans might also like.</param>
public class ArtistInfo(
    string name,
    string id,
    string browseId,
    Thumbnail[] thumbnails,
    string description,
    string monthlyListenersInfo,
    string subscribersInfo,
    string viewsInfo,
    string? pronouns,
    Radio? radio,
    ResultList<ArtistSong> topSongs,
    ResultList<ArtistAlbum> albums,
    ResultList<ArtistAlbum> singlesAndEps,
    ResultList<ArtistVideo> videos,
    ResultList<ArtistVideo> livePerformances,
    List<ArtistPlaylist> featuredOn,
    ResultList<ArtistPlaylist> playlists,
    List<ArtistCorrelation> fansMightAlsoLike) : YouTubeMusicEntity(name, id, browseId)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="ArtistInfo"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> '$' to parse.</param>
    /// <returns>A <see cref="ArtistInfo"/> representing the <see cref="JElement"/>.</returns>
    internal static ArtistInfo Parse(
        JElement element)
    {
        static List<T> Parse<T>(
            JElement item,
            string path,
            Func<JElement, T> parse) => item
                .Get("contents")
                .AsArray()
                .OrThrow()
                .Select(item => item
                    .Get(path))
                .Where(item => !item.IsUndefined)
                .Select(parse)
                .ToList();

        JElement item = element
            .Get("header")
            .Get("musicImmersiveHeaderRenderer");

        JElement subscriptionButton = item
            .Get("subscriptionButton")
            .Get("subscribeButtonRenderer");


        string name = item
            .SelectRunTextAt("title", 0)
            .OrThrow();

        string id = subscriptionButton
            .Get("channelId")
            .AsString()
            .OrThrow();

        string browseId = id;

        Thumbnail[] thumbnails = item
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        string description = item
            .Get("description")
            .Get("runs")
            .AsArray()
            .Or(JArray.Empty)
            .Select(run => run
                .Coalesce(
                    item => item
                        .Get("navigationEndpoint")
                        .Get("urlEndpoint")
                        .Get("url"),
                    item => item
                        .Get("text"))
                .AsString()
                .Or(""))
            .Join("");

        string monthlyListenersInfo = item
            .SelectRunTextAt("monthlyListenerCount", 0)
            .Or("N/A monthly audience");

        string subscribersInfo = subscriptionButton
            .SelectRunTextAt("longSubscriberCountText", 0)
            .Or("N/A subscribers");

        string viewsInfo = "N/A views";

        string? pronouns = item
            .SelectRunTextAt("pronouns", 0);

        Radio? radio = item
            .Get("startRadioButton")
            .Get("buttonRenderer")
            .SelectNavigationPlaylistId()
            .Map(playlistId => new Radio(playlistId));

        ResultList<ArtistSong> topSongs = new ();
        ResultList<ArtistAlbum> albums = new ();
        ResultList<ArtistAlbum> singlesAndEps = new();
        ResultList<ArtistVideo> videos = new();
        ResultList<ArtistVideo> livePerformances = new();
        List<ArtistPlaylist> featuredOn = [];
        ResultList<ArtistPlaylist> playlists = new();
        List<ArtistCorrelation> fansMightAlsoLike = [];

        element
            .Get("contents")
            .Get("singleColumnBrowseResultsRenderer")
            .Get("tabs")
            .GetAt(0)
            .Get("tabRenderer")
            .Get("content")
            .Get("sectionListRenderer")
            .Get("contents")
            .AsArray()
            .Or(JArray.Empty)
            .ForEach(content =>
            {
                JElement shelf = content
                    .Coalesce(
                        item => item.Get("musicShelfRenderer"),
                        item => item.Get("musicCarouselShelfRenderer"),
                        item => item.Get("musicDescriptionShelfRenderer"));

                JElement titleNode = shelf
                    .Coalesce(
                        item => item
                            .Get("title"),
                        item => item
                            .Get("header")
                            .Get("musicCarouselShelfBasicHeaderRenderer")
                            .Get("title"),
                        item => item
                            .Get("header"))
                    .Get("runs")
                    .GetAt(0);

                string? category = titleNode
                    .Get("text")
                    .AsString();

                JElement browseEndpointNode =
                    titleNode.Get("navigationEndpoint")
                    .Get("browseEndpoint");

                string? browseId2 = browseEndpointNode.Get("browseId")
                    .AsString();

                string? @params = browseEndpointNode.Get("params")
                    .AsString();

                ResultList<T> ToResultList<T>(List<T> results) =>
                    new()
                    {
                        Results = results,
                        BrowseId = browseId2,
                        Params = @params
                    };

                switch (category)
                {
                    case "Top songs":
                        topSongs = ToResultList(Parse(shelf, "musicResponsiveListItemRenderer", ArtistSong.Parse));
                        break;

                    case "Albums":
                        albums = ToResultList(Parse(shelf, "musicTwoRowItemRenderer", ArtistAlbum.Parse));
                        break;

                    case "Singles & EPs":
                        singlesAndEps = ToResultList(Parse(shelf, "musicTwoRowItemRenderer", ArtistAlbum.Parse));
                        break;

                    case "Videos":
                        videos = ToResultList(Parse(shelf, "musicTwoRowItemRenderer", ArtistVideo.Parse));
                        break;

                    case "Live performances":
                        livePerformances = ToResultList(Parse(shelf, "musicTwoRowItemRenderer", ArtistVideo.Parse));
                        break;

                    case "Featured on":
                        featuredOn = Parse(shelf, "musicTwoRowItemRenderer", ArtistPlaylist.Parse);
                        break;

                    case "Fans might also like":
                        fansMightAlsoLike = Parse(shelf, "musicTwoRowItemRenderer", ArtistCorrelation.Parse);
                        break;

                    case "About":
                        viewsInfo = shelf
                            .SelectRunTextAt("subheader", 0)
                            .Or("N/A views");
                        break;

                    default:
                        if (category?.StartsWith("Playlists") ?? false)
                            playlists = ToResultList(Parse(shelf, "musicTwoRowItemRenderer", ArtistPlaylist.Parse));
                        break;
                }
            });

        return new(name, id, browseId, thumbnails, description, monthlyListenersInfo, subscribersInfo, viewsInfo, pronouns, radio, topSongs, albums, singlesAndEps, videos, livePerformances, featuredOn, playlists, fansMightAlsoLike);
    }


    /// <summary>
    /// The ID of this artist.
    /// </summary>
    public override string Id { get; } = id;

    /// <summary>
    /// The browse ID of this artist.
    /// </summary>
    public override string BrowseId { get; } = browseId;


    /// <summary>
    /// The thumbnails of this artist.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The description of this artist.
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// The information about the monthly listeners of this artist.
    /// </summary>
    public string MonthlyListenersInfo { get; } = monthlyListenersInfo;

    /// <summary>
    /// The information about the numbers of subscribers this artist has.
    /// </summary>
    public string SubscribersInfo { get; } = subscribersInfo;

    /// <summary>
    /// The information about the numbers of views this artist has.
    /// </summary>
    public string ViewsInfo { get; } = viewsInfo;

    /// <summary>
    /// The pronouns of this artist, if available.
    /// </summary>
    public string? Pronouns { get; } = pronouns;

    /// <summary>
    /// The radio associated with this artist, if available.
    /// </summary>
    public Radio? Radio { get; } = radio;

    /// <summary>
    /// The top songs of this artist.
    /// </summary>
    public ResultList<ArtistSong> TopSongs { get; } = topSongs;

    /// <summary>
    /// Some of the recent albums of this artist.
    /// </summary>
    public ResultList<ArtistAlbum> Albums { get; } = albums;

    /// <summary>
    /// Some of the recent singles and EPs of this artist.
    /// </summary>
    public ResultList<ArtistAlbum> SinglesAndEps { get; } = singlesAndEps;

    /// <summary>
    /// Some of the recent and popular videos of this artist.
    /// </summary>
    public ResultList<ArtistVideo> Videos { get; } = videos;

    /// <summary>
    /// Some of the recent and popular live performances of this artist.
    /// </summary>
    public ResultList<ArtistVideo> LivePerformances { get; } = livePerformances;

    // public IReadOnlyList<ArtistLibraryItem> FromYourLibrary { get; } = fromYourLibrary // ??

    /// <summary>
    /// Some of the playlists this artists is featured on.
    /// </summary>
    public List<ArtistPlaylist> FeaturedOn { get; } = featuredOn;

    /// <summary>
    /// Some of the playlists created by this artists.
    /// </summary>
    public ResultList<ArtistPlaylist> Playlists { get; } = playlists;

    /// <summary>
    /// The correlations to this artists which fans might also like.
    /// </summary>
    public List<ArtistCorrelation> FansMightAlsoLike { get; } = fansMightAlsoLike;
}
