namespace YouTubeMusicAPI.Services.Albums;

/// <summary>
/// Represents the type of an album on YouTube Music.
/// </summary>
public enum AlbumType
{
    /// <summary>
    /// A full-length album release.
    /// </summary>
    Album,

    /// <summary>
    /// A single track release.
    /// </summary>
    Single,

    /// <summary>
    /// An extended play (EP release, longer than a single but shorter than an album.
    /// </summary>
    Ep
}