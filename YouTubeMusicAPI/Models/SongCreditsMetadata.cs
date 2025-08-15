using System.Text.Json;
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
    /// Parses a <see cref="JsonElement"/> into <see cref="SongCreditsMetadata"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> 'musicMultiRowListItemRenderer' to parse.</param>
    internal static SongCreditsMetadata Parse(
        JsonElement element)
    {
        JsonElement titleRun = element
            .GetProperty("title")
            .GetProperty("runs")
            .GetPropertyAt(0);


        string name = titleRun
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = titleRun
            .SelectNavigationVideoId();

        Thumbnail[] thumbnails = element
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity primaryArtist = element
            .GetProperty("subtitle")
            .GetProperty("runs")
            .GetPropertyAt(0)
            .SelectArtist();

        int? releaseYear = element
            .GetPropertyOrNull("secondTitle")
            ?.GetPropertyOrNull("runs")
            ?.GetPropertyAtOrNull(2)
            ?.GetPropertyOrNull("text")
            ?.GetString()
            .ToInt32();

        bool isExplicit = element
            .GetPropertyOrNull("badges")
            .SelectContainsExplicitBadge();

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