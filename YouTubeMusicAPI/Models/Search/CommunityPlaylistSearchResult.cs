using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a YouTube Music community playlist search result
/// </summary>
/// <param name="name">The name of this search result</param>
/// <param name="id">The id of this search result</param>
/// <param name="creator">The creator of this community playlist</param>
/// <param name="viewsInfo">The views info of this community playlist</param>
/// <param name="radio">The radio channel of this community playlist</param>
/// <param name="thumbnails">The thumbnails of this search result</param>
public class CommunityPlaylistSearchResult(
    string name,
    string id,
    NamedEntity creator,
    string viewsInfo,
    Radio? radio,
    Thumbnail[] thumbnails) : SearchResult(name, id, thumbnails, SearchCategory.CommunityPlaylists)
{
    /// <summary>
    /// The creator of this community playlist
    /// </summary>
    public NamedEntity Creator { get; } = creator;

    /// <summary>
    /// The views info of this community playlist
    /// </summary>
    public string ViewsInfo { get; } = viewsInfo;

    /// <summary>
    /// The radio channel of this community playlist
    /// </summary>
    public Radio? Radio { get; } = radio;
}