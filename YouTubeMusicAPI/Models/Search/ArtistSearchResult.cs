using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a YouTube Music artist search result
/// </summary>
/// <param name="name">The name of this artist</param>
/// <param name="id">The id of this artist</param>
/// <param name="subscribersInfo">The subscribers info of this artist</param>
/// <param name="radio">The radio channel of this artist</param>
/// <param name="thumbnails">The thumbnails of this artist</param>
public class ArtistSearchResult(
    string name,
    string id,
    string subscribersInfo,
    Radio radio,
    Thumbnail[] thumbnails) : IYouTubeMusicItem
{
    /// <summary>
    /// Gets the url of this artist which leads to YouTube music
    /// </summary>
    /// <param name="artist">The artist to get the url for </param>
    /// <returns>An url of this artist which leads to YouTube music</returns>
    public static string GetUrl(
        ArtistSearchResult artist) =>
        $"https://music.youtube.com/channel/{artist.Id}";


    /// <summary>
    /// The name of this artist
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of this artist
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The subscribers info of this artist
    /// </summary>
    public string SubscribersInfo { get; } = subscribersInfo;

    /// <summary>
    /// The radio channel of this artist
    /// </summary>
    public Radio Radio { get; } = radio;

    /// <summary>
    /// The thumbnails of this artist
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;


    /// <summary>
    /// The kind of this YouTube Music item
    /// </summary>
    public YouTubeMusicItemKind Kind => YouTubeMusicItemKind.Artists;
}