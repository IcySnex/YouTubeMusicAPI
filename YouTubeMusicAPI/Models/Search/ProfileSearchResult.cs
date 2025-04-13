namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a YouTube Music profile search result
/// </summary>
/// <param name="name">The name of this search result</param>
/// <param name="id">The id of this search result</param>
/// <param name="handle">The handle of this profile</param>
/// <param name="thumbnails">The thumbnails of this search result</param>
public class ProfileSearchResult(
    string name,
    string id,
    string handle,
    Thumbnail[] thumbnails) : SearchResult(name, id, thumbnails, SearchCategory.Profiles)
{
    /// <summary>
    /// The handle of this profile
    /// </summary>
    public string Handle { get; } = handle;
}