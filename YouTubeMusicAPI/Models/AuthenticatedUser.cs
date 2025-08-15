using System.Text.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models;

/// <summary>
/// Represents an authenticated user on YouTube Music.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="AuthenticatedUser"/> class.
/// </remarks>
/// <param name="name">The name of this authenticated user.</param>
/// <param name="id">The ID of this authenticated user.</param>
/// <param name="thumbnails">The thumbnails of this authenticated user.</param>
/// <param name="handle">The handle of this authenticated user.</param>
/// <param name="isPremium">Whether this authenticated user is subscribed to premium.</param>
public class AuthenticatedUser(
    string name,
    string id,
    Thumbnail[] thumbnails,
    string handle,
    bool isPremium) : YouTubeMusicEntity(name, id, id)
{
    /// <summary>
    /// Parses a <see cref="JsonElement"/> into an <see cref="AuthenticatedUser"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> 'multiPageMenuRenderer' to parse.</param>
    internal static AuthenticatedUser Parse(
        JsonElement element)
    {
        JsonElement headerRenderer = element
            .GetProperty("header")
            .GetProperty("activeAccountHeaderRenderer");

        JsonElement items = element
            .GetProperty("sections")
            .GetPropertyAt(0)
            .GetProperty("multiPageMenuSectionRenderer")
            .GetProperty("items");


        string name = headerRenderer
            .SelectRunTextAt("accountName");

        string id = items
            .GetPropertyAt(0)
            .GetProperty("compactLinkRenderer")
            .SelectNavigationBrowseId();

        Thumbnail[] thumbnails = headerRenderer
            .SelectThumbnails("accountPhoto");

        string handle = headerRenderer
            .SelectRunTextAt("channelHandle");

        bool isPremium = items
            .GetPropertyAt(1)
            .GetProperty("compactLinkRenderer")
            .SelectRunTextAt("title")
            .If("Paid Memberships", true, false);

        return new(name, id, thumbnails, handle, isPremium);
    }


    /// <summary>
    /// The ID of this authenticated user.
    /// </summary>
    public override string Id { get; } = id;

    /// <summary>
    /// The browse ID of this authenticated user.
    /// </summary>
    public override string BrowseId { get; } = id;


    /// <summary>
    /// The thumbnails of this authenticated user.
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;

    /// <summary>
    /// The handle of this authenticated user.
    /// </summary>
    public string Handle { get; } = handle;

    /// <summary>
    /// Whether this authenticated user is subscribed to premium.
    /// </summary>
    public bool IsPremium { get; } = isPremium;
}