namespace YouTubeMusicAPI.Services.Search;

/// <summary>
/// Represents the scope of a search on YouTube Music.
/// </summary>
public enum SearchScope
{
    /// <summary>
    /// Search across global YouTube Music.
    /// </summary>
    Global,

    /// <summary>
    /// Search only within the user's library.
    /// </summary>
    Library,

    // I have no way to test this, so it is not implemented.
    ///// <summary>
    ///// Search only within the user's uploads.
    ///// </summary>
    //Uploads
}