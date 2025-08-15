using System.Text.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a profile search result on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="ProfileSearchResult"/> class.
/// </remarks>
/// <param name="name">The name of this profile.</param>
/// <param name="id">The ID of this profile.</param>
/// <param name="thumbnails">The thumbnails of this profile.</param>
/// <param name="handle">The handle of this profile.</param>
public class ProfileSearchResult(
    string name,
    string id,
    Thumbnail[] thumbnails,
    string handle) : SearchResult(name, id, id, thumbnails)
{
    /// <summary>
    /// Parses a <see cref="JsonElement"/> into a <see cref="ProfileSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> "musicResponsiveListItemRenderer".</param>
    internal static ProfileSearchResult Parse(
        JsonElement element)
    {
        JsonElement flexColumns = element
            .GetProperty("flexColumns");

        JsonElement descriptionRuns = flexColumns
            .GetPropertyAt(1)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs");

        int descriptionStartIndex = descriptionRuns
            .GetPropertyAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow()
            .If("Profile", 2, 0);


        string name = flexColumns
            .GetPropertyAt(0)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs")
            .GetPropertyAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = element
            .SelectNavigationBrowseId();

        Thumbnail[] thumbnails = element
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        string handle = descriptionRuns
            .GetPropertyAt(descriptionStartIndex)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        return new(name, id, thumbnails, handle);
    }


    /// <summary>
    /// The browse ID of this profile.
    /// </summary>
    public override string BrowseId { get; } = id;


    /// <summary>
    /// The handle of this profile.
    /// </summary>
    public string Handle { get; } = handle;
}