using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Services.Albums;
using YouTubeMusicAPI.Services.Artists;
using YouTubeMusicAPI.Services.Episodes;
using YouTubeMusicAPI.Services.Musical.Songs;
using YouTubeMusicAPI.Services.Musical.Videos;
using YouTubeMusicAPI.Services.Playlists;
using YouTubeMusicAPI.Services.Podcasts;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Profiles;

/// <summary>
/// Represents a profile on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="ProfileInfo"/> class.
/// </remarks>
/// <param name="name">The name of this profile.</param>
/// <param name="id">The ID of this profile.</param>
/// <param name="browseId">The browse ID of this profile.</param>
/// <param name="thumbnails">The thumbnails of this profile.</param>
/// <param name="foregroundThumbnails">The foreground thumbnails of this profile.</param>
/// <param name="description">The description of this profile.</param>
/// <param name="subscribersInfo">The information about the numbers of subscribers this profile has.</param>
/// <param name="videos">Some of the recent and popular videos of this profile.</param>
/// <param name="playlists">Some of the playlists created by this profile.</param>
/// <param name="latestEpisodes">The latest podcast episodes of this profile.</param>
/// <param name="podcasts">Some of the recent and popluar podcasts of this profile.</param>
public class ProfileInfo(
    string name,
    string id,
    string browseId,
    Thumbnail[] thumbnails,
    Thumbnail[] foregroundThumbnails,
    string description,
    string subscribersInfo,
    IReadOnlyList<ArtistVideo> videos,
    IReadOnlyList<ArtistPlaylist> playlists,
    IReadOnlyList<ProfileEpisode> latestEpisodes,
    IReadOnlyList<ProfilePodcast> podcasts) : YouTubeMusicEntity(name, id, browseId)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into a <see cref="ProfileInfo"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> '$' to parse.</param>
    /// <returns>A <see cref="ProfileInfo"/> representing the <see cref="JElement"/>.</returns>
    internal static ProfileInfo Parse(
        JElement element)
    {
        static List<T> Parse<T>(
            JElement item,
            string path,
            Func<JElement, T> parse) =>
            item
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
            .Get("musicVisualHeaderRenderer");

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

        Thumbnail[] foregroundThumbnails = item
            .Get("foregroundThumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        string description = element
            .Get("microformat")
            .Get("microformatDataRenderer")
            .Get("description")
            .AsString()
            .Or("");

        string subscribersInfo = subscriptionButton
            .SelectRunTextAt("longSubscriberCountText", 0)
            .Or("N/A subscribers");


        List<ArtistVideo> videos = [];
        List<ArtistPlaylist> playlists = [];
        List<ProfileEpisode> latestEpisodes = [];
        List<ProfilePodcast> podcasts = [];

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

                string? category = shelf
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
                    .GetAt(0)
                    .Get("text")
                    .AsString();
                switch (category)
                {
                    case "Videos":
                        videos = Parse(shelf, "musicTwoRowItemRenderer", ArtistVideo.Parse);
                        break;
                        
                    case "Playlists":
                        playlists = Parse(shelf, "musicTwoRowItemRenderer", ArtistPlaylist.Parse);
                        break;

                    case "Latest episodes":
                        latestEpisodes = Parse(shelf, "musicMultiRowListItemRenderer", ProfileEpisode.Parse);
                        break;

                    case "Podcasts":
                        podcasts = Parse(shelf, "musicTwoRowItemRenderer", ProfilePodcast.Parse);
                        break;
                }
            });

        return new(name, id, browseId, thumbnails, foregroundThumbnails, description, subscribersInfo, videos, playlists, latestEpisodes, podcasts);
    }


    /// <summary>
    /// The ID of this profile.
    /// </summary>
    public override string Id { get; } = id;

    /// <summary>
    /// The browse ID of this profile.
    /// </summary>
    public override string BrowseId { get; } = browseId;


    /// <summary>
    /// The thumbnails of this profile.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The foreground thumbnails of this profile.
    /// </summary>
    public Thumbnail[] ForegroundThumbnails { get; } = foregroundThumbnails;

    /// <summary>
    /// The description of this profile.
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// The information about the numbers of subscribers this profile has.
    /// </summary>
    public string? SubscribersInfo { get; } = subscribersInfo;

    /// <summary>
    /// Some of the recent and popular videos of this profile.
    /// </summary>
    public IReadOnlyList<ArtistVideo> Videos { get; } = videos;

    /// <summary>
    /// Some of the playlists created by this profile.
    /// </summary>
    public IReadOnlyList<ArtistPlaylist> Playlists { get; } = playlists;

    /// <summary>
    /// The latest podcast episodes of this profile.
    /// </summary>
    public IReadOnlyList<ProfileEpisode> LatestEpisodes { get; } = latestEpisodes;

    /// <summary>
    /// Some of the recent and popluar podcasts of this profile.
    /// </summary>
    public IReadOnlyList<ProfilePodcast> Podcasts { get; } = podcasts;
}
