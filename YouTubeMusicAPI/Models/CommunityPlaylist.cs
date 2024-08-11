using YouTubeMusicAPI.Models.Shelf;
using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models;

/// <summary>
/// Represents a YouTube Music community playlist
/// </summary>
/// <param name="name">The name of this community playlist</param>
/// <param name="id">The id of this community playlist</param>
/// <param name="creator">The creator of this community playlist</param>
/// <param name="viewsInfo">The views info of this community playlist</param>
/// <param name="radio">The radio channel of this community playlist</param>
/// <param name="thumbnails">The thumbnails of this community playlist</param>
public class CommunityPlaylist(
    string name,
    string id,
    ShelfItem creator,
    string viewsInfo,
    Radio? radio,
    Thumbnail[] thumbnails) : IShelfItem
{
    /// <summary>
    /// Gets the url of this community playlist which leads to YouTube music
    /// </summary>
    /// <param name="communityPlaylist">The community playlist to get the url for </param>
    /// <returns>An url of this community playlist which leads to YouTube music</returns>
    public static string GetUrl(
        CommunityPlaylist communityPlaylist) =>
        $"https://music.youtube.com/playlist?list={communityPlaylist.Id}";


    /// <summary>
    /// The name of this community playlist
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of this community playlist
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The creator of this community playlist
    /// </summary>
    public ShelfItem Creator { get; } = creator;

    /// <summary>
    /// The views info of this community playlist
    /// </summary>
    public string ViewsInfo { get; } = viewsInfo;

    /// <summary>
    /// The radio channel of this community playlist
    /// </summary>
    public Radio? Radio { get; } = radio;

    /// <summary>
    /// The thumbnails of this community playlist
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;


    /// <summary>
    /// The kind of this shelf item
    /// </summary>
    public ShelfKind Kind => ShelfKind.CommunityPlaylists;
}