using YouTubeMusicAPI.Models.Shelf;
using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models;

/// <summary>
/// Represents a YouTube Music profile
/// </summary>
/// <param name="name">The name of this profile</param>
/// <param name="id">The id of this profile</param>
/// <param name="handle">The handle of this profile</param>
/// <param name="thumbnails">The thumbnails of this profile</param>
public class Profile(
    string name,
    string id,
    string handle,
    Thumbnail[] thumbnails) : IShelfItem
{
    /// <summary>
    /// Gets the url of this profile which leads to YouTube music
    /// </summary>
    /// <param name="profile">The profile to get the url for </param>
    /// <returns>An url of this profile which leads to YouTube music</returns>
    public static string GetUrl(
        Profile profile) =>
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
    /// The kind of this shelf item
    /// </summary>
    public ShelfKind Kind => ShelfKind.Profiles;
}