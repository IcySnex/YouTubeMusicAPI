using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Albums;

/// <summary>
/// 
/// </summary>
/// <param name="artistBrowseId">The browse id of the artist</param>
/// <param name="albumCategory">The album category</param>
/// <param name="sortingOrder">The sorting order of the albums</param>
/// <param name="albums">The albums of the said artist filtered by <param name="albumCategory" /> </param>
public class ArtistAlbums(
    string artistBrowseId,
    AlbumCategory albumCategory,
    AlbumSortingOrder sortingOrder,
    IReadOnlyList<ArtistAlbum> albums)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into <see cref="ArtistAlbums"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "contents".</param>
    /// <param name="browseId">The browse id of the artist</param>
    /// <param name="albumCategory">The album category</param>
    /// <param name="sortingOrder">The sorting order of the albums</param>
    internal static ArtistAlbums Parse(
        JElement element,
        string browseId,
        AlbumCategory albumCategory,
        AlbumSortingOrder sortingOrder)
    {
        List<ArtistAlbum> albums = [];
        
        element
            .Get("contents")
            .Get("singleColumnBrowseResultsRenderer")
            .Get("tabs")
            .GetAt(0)
            .Get("tabRenderer")
            .Get("content")
            .Get("sectionListRenderer")
            .Get("contents")
            .GetAt(0)
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

        return new(browseId, albumCategory, sortingOrder, albums);
    }

    /// <summary>
    /// The browse id of the artist
    /// </summary>
    public string ArtistBrowseId { get; } = artistBrowseId;
    
    /// <summary>
    /// The albums of the said artist
    /// </summary>
    public IReadOnlyList<ArtistAlbum> Albums { get; } = albums;

    /// <summary>
    /// The category of the album
    /// </summary>
    public AlbumCategory AlbumCategory { get; } = albumCategory;

    /// <summary>
    /// The album sorting order
    /// </summary>
    public AlbumSortingOrder SortingOrder { get; } = sortingOrder;
}