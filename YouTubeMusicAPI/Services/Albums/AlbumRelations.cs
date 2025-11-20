using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Albums;

/// <summary>
/// Represents related content for an album on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="AlbumRelations"/> class.
/// </remarks>
/// <param name="otherVersions">The different editions of the source album, such as deluxe, remastared or alternative versions.</param>
/// <param name="releasesForYou">The related albums, similar to the source album.</param>
public class AlbumRelations(
    IReadOnlyList<RelatedAlbum> otherVersions,
    IReadOnlyList<RelatedAlbum> releasesForYou)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into <see cref="AlbumRelations"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "contents".</param>
    internal static AlbumRelations Parse(
        JElement element)
    {
        static List<RelatedAlbum> Parse(
            JElement item) =>
            item
                .Get("contents")
                .AsArray()
                .OrThrow()
                .Select(item => item
                    .Get("musicTwoRowItemRenderer"))
                .Where(item => !item.IsUndefined)
                .Select(RelatedAlbum.Parse)
                .ToList();


        List<RelatedAlbum> otherVersions = [];
        List<RelatedAlbum> releasesForYou = [];

        element
            .AsArray()
            .Or(JArray.Empty)
            .Select(item => item
                .Get("musicCarouselShelfRenderer"))
            .Where(item => !item.IsUndefined)
            .ForEach(item =>
            {
                switch (item
                    .Get("header")
                    .Get("musicCarouselShelfBasicHeaderRenderer")
                    .SelectRunTextAt("title", 0))
                {
                    case "Other versions":
                        otherVersions = Parse(item);
                        break;

                    case "Releases for you":
                        releasesForYou = Parse(item);
                        break;
                }
            });

        return new(otherVersions, releasesForYou);
    }


    /// <summary>
    /// The different editions of the source album, such as deluxe, remastared or alternative versions.
    /// </summary>
    public IReadOnlyList<RelatedAlbum> OtherVersions { get; } = otherVersions;

    /// <summary>
    /// The related albums, similar to the source album.
    /// </summary>
    public IReadOnlyList<RelatedAlbum> ReleasesForYou { get; } = releasesForYou;
}