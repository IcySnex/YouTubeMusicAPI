using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Services.Albums;
using YouTubeMusicAPI.Services.Artists;
using YouTubeMusicAPI.Services.Playlists;
using YouTubeMusicAPI.Services.Songs;
using YouTubeMusicAPI.Services.Videos;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Relations;

/// <summary>
/// Represents related content for a song/video on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="SongVideoRelations"/> class.
/// </remarks>
public class SongVideoRelations(
    IReadOnlyList<RelatedSong> youMightAlsoLike,
    IReadOnlyList<RelatedPlaylist> recommendedPlaylists,
    IReadOnlyList<RelatedVideo> otherPerformances,
    IReadOnlyList<RelatedArtist> similarArtists,
    IReadOnlyList<RelatedAlbum> moreFrom,
    string? aboutTheArtist)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into <see cref="SongVideoRelations"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "$".</param>
    internal static SongVideoRelations Parse(
        JElement element)
    {
        List<T> Parse<T>(
            JElement item,
            string key,
            Func<JElement, T> parse) =>
            item
                .Get("contents")
                .AsArray()
                .OrThrow()
                .Where(item => item
                    .Contains(key))
                .Select(item => item
                    .Get(key))
                .Select(parse)
                .ToList();


        JElement contents = element
            .Get("contents")
            .Get("sectionListRenderer")
            .Get("contents");


        List<RelatedSong> youMightAlsoLike = [];
        List<RelatedPlaylist> recommendedPlaylists = [];
        List<RelatedVideo> otherPerformances = [];
        List<RelatedArtist> similarArtists = [];
        List<RelatedAlbum> moreFrom = [];

        contents
            .AsArray()
            .Or(JArray.Empty)
            .Select(item => item
                .Get("musicCarouselShelfRenderer"))
            .Where(item => !item.IsUndefined)
            .ForEach(item =>
            {
                JElement header = item
                    .Get("header")
                    .Get("musicCarouselShelfBasicHeaderRenderer");

                switch (header
                    .SelectRunTextAt("title", 0))
                {
                    case "You might also like":
                        youMightAlsoLike = Parse(item, "musicResponsiveListItemRenderer", RelatedSong.Parse);
                        break;

                    case "Recommended playlists":
                        recommendedPlaylists = Parse(item, "musicTwoRowItemRenderer", RelatedPlaylist.Parse);
                        break;

                    case "Other performances":
                        otherPerformances = Parse(item, "musicResponsiveListItemRenderer", RelatedVideo.Parse);
                        break;

                    case "Similar artists":
                        similarArtists = Parse(item, "musicTwoRowItemRenderer", RelatedArtist.Parse);
                        break;

                    default:
                        if (header
                            .SelectRunTextAt("strapline", 0)
                            .Is("MORE FROM"))
                            moreFrom = Parse(item, "musicTwoRowItemRenderer", RelatedAlbum.Parse);
                        break;
                }
            });

        string? aboutTheArtist = contents
            .GetAt(contents.ArrayLength - 1)
            .Get("musicDescriptionShelfRenderer")
            .Get("description")
            .Get("runs")
            .AsArray()
            .IsNotNull(out JArray? runs)
                ? runs
                    .Select(run => run
                        .Get("text")
                        .AsString()
                        .OrThrow())
                    .Join("")
                : null;

        return new(youMightAlsoLike, recommendedPlaylists, otherPerformances, similarArtists, moreFrom, aboutTheArtist);
    }


    /// <summary>
    /// The songs which you might also like.
    /// </summary>
    public IReadOnlyList<RelatedSong> YouMightAlsoLike { get; } = youMightAlsoLike;

    /// <summary>
    /// The recommended playlists.
    /// </summary>
    public IReadOnlyList<RelatedPlaylist> RecommendedPlaylists { get; } = recommendedPlaylists;

    /// <summary>
    /// The other performances.
    /// </summary>
    /// <remarks>
    /// Some of the related "videos" may actually be songs but oh well :)
    /// </remarks> 
    public IReadOnlyList<RelatedVideo> OtherPerformances { get; } = otherPerformances;

    /// <summary>
    /// The similar artists.
    /// </summary>
    public IReadOnlyList<RelatedArtist> SimilarArtists { get; } = similarArtists;

    /// <summary>
    /// The more albums from the artists.
    /// </summary>
    public IReadOnlyList<RelatedAlbum> MoreFrom { get; } = moreFrom;

    /// <summary>
    /// The description about the artist.
    /// </summary>
    public string? AboutTheArtist { get; } = aboutTheArtist;
}