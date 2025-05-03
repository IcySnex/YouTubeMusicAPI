namespace YouTubeMusicAPI.Http;

internal static class Endpoints
{
    // Base Urls
    public const string YouTubeUrl = "https://www.youtube.com";


    // Url Paths
    public static string Embed(
        string videoId) =>
        $"/embed/{videoId}";
}