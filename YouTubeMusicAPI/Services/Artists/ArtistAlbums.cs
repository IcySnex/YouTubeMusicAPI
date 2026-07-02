using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Services.Albums;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Artists;

/// <summary>
/// Represents a list of all YouTube Music albums or singles / EPs of an artist. 
/// </summary>
/// <param name="albums"><see cref="ArtistAlbums.Albums" /> </param>
public class ArtistAlbums(IReadOnlyList<ArtistAlbum> albums)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into <see cref="ArtistAlbums"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "contents".</param>
    /// <param name="isContinuationResponse">A boolean to indicate whether the element is from a contination response</param>
    internal static ArtistAlbums Parse(
        JElement element,
        bool isContinuationResponse)
    {
        List<ArtistAlbum> albums = [];

        var contents = isContinuationResponse
            ? element.Get("continuationContents")
                    .Get("sectionListContinuation")
                    .Get("contents")
            : element.Get("contents")
                    .Get("singleColumnBrowseResultsRenderer")
                    .Get("tabs")
                    .GetAt(0)
                    .Get("tabRenderer")
                    .Get("content")
                    .Get("sectionListRenderer")
                    .Get("contents");

        contents.GetAt(0)
            .Get("gridRenderer")
            .Get("items")
            .AsArray()
            .Or(JArray.Empty)
            .ForEach(item =>
            {
                JElement renderer = item.Get("musicTwoRowItemRenderer");
                ArtistAlbum album = ArtistAlbum.Parse(renderer);
                albums.Add(album);
            });

        return new(albums);
    }

    /// <summary>
    /// The albums of the said artist
    /// </summary>
    public IReadOnlyList<ArtistAlbum> Albums { get; } = albums;
}