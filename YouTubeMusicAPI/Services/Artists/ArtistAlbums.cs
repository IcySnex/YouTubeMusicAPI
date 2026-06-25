using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Services.Albums;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Artists;

/// <summary>
/// Represents the albums of an artist on YouTube Music.
/// </summary>
/// <param name="browseId"><see cref="ArtistAlbums.BrowseId" /></param>
/// <param name="params"><see cref="ArtistAlbums.Params" /></param>
/// <param name="sortingOrder"><see cref="ArtistAlbums.SortingOrder" /></param>
/// <param name="albums"><see cref="ArtistAlbums.Albums" /> </param>
public class ArtistAlbums(
    string browseId,
    string @params,
    AlbumSortingOrder sortingOrder,
    IReadOnlyList<ArtistAlbum> albums)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into <see cref="ArtistAlbums"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "contents".</param>
    /// <param name="browseId"><see cref="ArtistAlbums.BrowseId" /></param>
    /// <param name="params"><see cref="ArtistAlbums.Params" /></param>
    /// <param name="sortingOrder"><see cref="ArtistAlbums.SortingOrder" /></param>
    /// <param name="isContinuationResponse">A boolean to indicate whether the element is from a contination response</param>
    internal static ArtistAlbums Parse(
        JElement element,
        string browseId,
        string @params,
        AlbumSortingOrder sortingOrder,
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

        return new(browseId, @params, sortingOrder, albums);
    }

    /// <summary>
    /// The browse id of the albums for the the artist
    /// </summary>
    public string BrowseId { get; } = browseId;

    /// <summary>
    /// 
    /// </summary>
    public string Params { get; } = @params;

    /// <summary>
    /// The albums of the said artist
    /// </summary>
    public IReadOnlyList<ArtistAlbum> Albums { get; } = albums;

    /// <summary>
    /// The album sorting order
    /// </summary>
    public AlbumSortingOrder SortingOrder { get; } = sortingOrder;
}