namespace YouTubeMusicAPI.Types;

/// <summary>
/// Contains extension methods for types
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Converts search kind to YouTube Music request payload params
    /// </summary>
    /// <param name="kind">The shelf kind to convert</param>
    /// <returns>A YouTube Music request payload params</returns>
    public static string? ToParams(
        this ShelfKind? kind) =>
        kind switch
        {
            ShelfKind.Songs => "EgWKAQIIAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            ShelfKind.Videos => "EgWKAQIQAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            ShelfKind.Albums => "EgWKAQIYAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            ShelfKind.CommunityPlaylists => "EgeKAQQoAEABahAQAxAKEAkQBBAFEBEQEBAV",
            ShelfKind.Artists => "EgWKAQIgAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            ShelfKind.Podcasts => "EgWKAQJQAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            ShelfKind.Episodes => "EgWKAQJIAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            ShelfKind.Profiles => "EgWKAQJYAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            _ => null
        };

    /// <summary>
    /// Converts string to shelf kind
    /// </summary>
    /// <param name="shelfKindString">The string to convert</param>
    /// <returns>A ShelfKind enum value</returns>
    public static ShelfKind ToShelfKind(
        this string? shelfKindString) =>
        shelfKindString switch
        {
            "Songs" => ShelfKind.Songs,
            "Videos" => ShelfKind.Videos,
            "Albums" => ShelfKind.Albums,
            "Community playlists" => ShelfKind.CommunityPlaylists,
            "Artists" => ShelfKind.Artists,
            "Podcasts" => ShelfKind.Podcasts,
            "Episodes" => ShelfKind.Episodes,
            "Profiles" => ShelfKind.Profiles,
            _ => ShelfKind.Unknown
        };
}