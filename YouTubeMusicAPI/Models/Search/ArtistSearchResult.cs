namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a YouTube Music artist search result
/// </summary>
/// <param name="name">The name of this search result</param>
/// <param name="id">The id of this search result</param>
/// <param name="popularityInfo">The subscribers or monthly audience info of this artist</param>
/// <param name="radio">The radio channel of this artist</param>
/// <param name="thumbnails">The thumbnails of this search result</param>
public class ArtistSearchResult(
    string name,
    string id,
    string popularityInfo,
    Radio? radio,
    Thumbnail[] thumbnails) : SearchResult(name, id, thumbnails, SearchCategory.Artists)
{
    /// <summary>
    /// The subscribers or monthly audience info of this artist
    /// </summary>
    public string PopularityInfo { get; } = popularityInfo;

    /// <summary>
    /// The radio channel of this artist
    /// </summary>
    public Radio? Radio { get; } = radio;
}