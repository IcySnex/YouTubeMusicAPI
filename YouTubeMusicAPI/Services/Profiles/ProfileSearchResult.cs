using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Services.Search;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Services.Profiles;

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
    /// Parses a <see cref="JElement"/> into a <see cref="ProfileSearchResult"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> "musicResponsiveListItemRenderer".</param>
    internal static ProfileSearchResult Parse(
        JElement element)
    {
        JElement flexColumns = element
            .Get("flexColumns");

        JElement descriptionRuns = flexColumns
            .GetAt(1)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .Get("text")
            .Get("runs");

        int descriptionStartIndex = descriptionRuns
            .GetAt(0)
            .Get("text")
            .AsString()
            .OrThrow()
            .Is("Profile")
                ? 2
                : 0;


        string name = flexColumns
            .GetAt(0)
            .Get("musicResponsiveListItemFlexColumnRenderer")
            .SelectRunTextAt("text", 0)
            .OrThrow();

        string id = element
            .SelectNavigationBrowseId()
            .OrThrow();

        Thumbnail[] thumbnails = element
            .Get("thumbnail")
            .Get("musicThumbnailRenderer")
            .SelectThumbnails();

        string handle = descriptionRuns
            .GetAt(descriptionStartIndex)
            .Get("text")
            .AsString()
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