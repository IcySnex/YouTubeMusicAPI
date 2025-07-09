using System.Text.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Search;

/// <summary>
/// Represents a profile search result on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="ProfileSearchResult"/>.
/// </remarks>
/// <param name="name">The name of this profile.</param>
/// <param name="id">The ID of this profile.</param>
/// <param name="thumbnails">The thumbnails of this profile.</param>
/// <param name="handle">The handle of this profile.</param>
public class ProfileSearchResult(
    string name,
    string id,
    Thumbnail[] thumbnails,
    string handle) : SearchResult(name, id, thumbnails)
{
    /// <summary>
    /// Parses the JSON element into a <see cref="ProfileSearchResult"/>.
    /// </summary>
    /// <param name="element">The JSON element containing "musicResponsiveListItemRenderer".</param>
    internal static ProfileSearchResult Parse(
        JsonElement element)
    {
        JsonElement item = element
            .GetProperty("musicResponsiveListItemRenderer");

        JsonElement flexColumns = item
            .GetProperty("flexColumns");


        string name = flexColumns
            .GetElementAt(0)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs")
            .GetElementAt(0)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = item
            .SelectNavigationBrowseId();

        Thumbnail[] thumbnails = item
            .GetProperty("thumbnail")
            .SelectThumbnails();

        string handle = flexColumns
            .GetElementAt(1)
            .GetProperty("musicResponsiveListItemFlexColumnRenderer")
            .GetProperty("text")
            .GetProperty("runs")
            .GetElementAt(2)
            .GetProperty("text")
            .GetString()
            .OrThrow();

        return new(name, id, thumbnails, handle);
    }


    /// <summary>
    /// The handle of this profile.
    /// </summary>
    public string Handle { get; } = handle;
}