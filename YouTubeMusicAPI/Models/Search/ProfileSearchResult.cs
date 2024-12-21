using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a YouTube Music profile search result
/// </summary>
/// <param name="name">The name of this profile</param>
/// <param name="id">The id of this profile</param>
/// <param name="handle">The handle of this profile</param>
/// <param name="thumbnails">The thumbnails of this profile</param>
public class ProfileSearchResult(
    string name,
    string id,
    string handle,
    Thumbnail[] thumbnails) : IYouTubeMusicItem
{
    /// <summary>
    /// Gets the url of this profile which leads to YouTube music
    /// </summary>
    /// <param name="profile">The profile to get the url for </param>
    /// <returns>An url of this profile which leads to YouTube music</returns>
    public static string GetUrl(
        ProfileSearchResult profile) =>
        $"https://music.youtube.com/channel/{profile}";


    /// <summary>
    /// The name of this profile
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of this profile
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The handle of this profile
    /// </summary>
    public string Handle { get; } = handle;

    /// <summary>
    /// The thumbnails of this profile
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;


    /// <summary>
    /// The kind of this YouTube Music item
    /// </summary>
    public YouTubeMusicItemKind Kind => YouTubeMusicItemKind.Profiles;
}