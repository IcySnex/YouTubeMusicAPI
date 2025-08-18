using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models;

/// <summary>
/// Represents the song metadata for which the <see cref="SongCredits"/> are.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="SongCreditsMetadata"/> class.
/// </remarks>
/// <param name="name">The name of the song.</param>
/// <param name="id">The ID of the song.</param>
/// <param name="thumbnails">The thumbnails associated with the song.</param>
/// <param name="primaryArtist">The primary artist of the song.</param>
/// <param name="releaseYear">The year the song was released.</param>
/// <param name="isExplicit">Indicates whether the song is explicit.</param>
public class SongCreditsMetadata(
    string name,
    string id,
    Thumbnail[] thumbnails,
    YouTubeMusicEntity primaryArtist,
    int? releaseYear,
    bool isExplicit)
{
    /// <summary>
    /// Parses a <see cref="JElement"/> into <see cref="SongCreditsMetadata"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> 'musicMultiRowListItemRenderer' to parse.</param>
    /// <returns><see cref="SongCreditsMetadata"/> representing the <see cref="JElement"/>.</returns>
    internal static SongCreditsMetadata Parse(
        JElement element)
    {
        JElement titleRun = element
            .Get("title")
            .Get("runs")
            .GetAt(0);


        string name = titleRun
            .Get("text")
            .AsString()
            .OrThrow();

        string id = titleRun
            .SelectNavigationVideoId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity primaryArtist = element
            .Get("subtitle")
            .Get("runs")
            .GetAt(0)
            .SelectArtist();

        int? releaseYear = element
            .SelectRunTextAt("secondTitle", 2)
            .ToInt32();

        bool isExplicit = element
            .SelectIsExplicit();

        return new(name, id, thumbnails, primaryArtist, releaseYear, isExplicit);
    }


    /// <summary>
    /// The name of the song.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The ID of the song.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The thumbnails associated with the song.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The primary artist of the song.
    /// </summary>
    public YouTubeMusicEntity PrimaryArtist { get; } = primaryArtist;

    /// <summary>
    /// The year the song was released.
    /// </summary>
    public int? ReleaseYear { get; } = releaseYear;

    /// <summary>
    /// Indicates whether the song is marked as explicit.
    /// </summary>
    public bool IsExplicit { get; } = isExplicit;
}