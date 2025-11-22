using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Services;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Http.Authentication;

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
    /// Parses a <see cref="JElement"/> into an <see cref="AuthenticatedUser"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> 'multiPageMenuRenderer' to parse.</param>
    internal static AuthenticatedUser Parse(
        JElement element)
    {
        JElement headerRenderer = element
            .Get("header")
            .Get("activeAccountHeaderRenderer");

        JElement items = element
            .Get("sections")
            .GetAt(0)
            .Get("multiPageMenuSectionRenderer")
            .Get("items");


        string name = headerRenderer
            .SelectRunTextAt("accountName", 0)
            .OrThrow();

        string id = items
            .GetAt(0)
            .Get("compactLinkRenderer")
            .SelectNavigationBrowseId()
            .OrThrow();

        Thumbnail[] thumbnails = headerRenderer
            .SelectThumbnails("accountPhoto");

        string handle = headerRenderer
            .SelectRunTextAt("channelHandle", 0)
            .OrThrow();

        bool isPremium = items
            .GetAt(1)
            .Get("compactLinkRenderer")
            .SelectRunTextAt("title", 0)
            .Is("Paid memberships");

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