namespace YouTubeMusicAPI.Types;

/// <summary>
/// Contains extension methods for types
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Converts search kind to YouTube Music request payload params
    /// </summary>
    /// <param name="kind">The YouTube Music item kind to convert</param>
    /// <returns>A YouTube Music request payload params</returns>
    public static string? ToParams(
        this YouTubeMusicItemKind? kind) =>
        kind switch
        {
            YouTubeMusicItemKind.Songs => "EgWKAQIIAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            YouTubeMusicItemKind.Videos => "EgWKAQIQAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            YouTubeMusicItemKind.Albums => "EgWKAQIYAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            YouTubeMusicItemKind.CommunityPlaylists => "EgeKAQQoAEABahAQAxAKEAkQBBAFEBEQEBAV",
            YouTubeMusicItemKind.Artists => "EgWKAQIgAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            YouTubeMusicItemKind.Podcasts => "EgWKAQJQAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            YouTubeMusicItemKind.Episodes => "EgWKAQJIAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            YouTubeMusicItemKind.Profiles => "EgWKAQJYAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            _ => null
        };

    /// <summary>
    /// Converts string to YouTube Music item kind
    /// </summary>
    /// <param name="shelfKindString">The string to convert</param>
    /// <returns>A YouTubeMusicItemKind enum value</returns>
    public static YouTubeMusicItemKind ToShelfKind(
        this string? shelfKindString) =>
        shelfKindString switch
        {
            "Songs" => YouTubeMusicItemKind.Songs,
            "Videos" => YouTubeMusicItemKind.Videos,
            "Albums" => YouTubeMusicItemKind.Albums,
            "Community playlists" => YouTubeMusicItemKind.CommunityPlaylists,
            "Artists" => YouTubeMusicItemKind.Artists,
            "Podcasts" => YouTubeMusicItemKind.Podcasts,
            "Episodes" => YouTubeMusicItemKind.Episodes,
            "Profiles" => YouTubeMusicItemKind.Profiles,
            _ => YouTubeMusicItemKind.Unknown
        };
}