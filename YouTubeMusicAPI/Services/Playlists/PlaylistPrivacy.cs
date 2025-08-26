namespace YouTubeMusicAPI.Services.Playlists;

/// <summary>
/// Represents the privacy settings of a playlist on YouTube Music.
/// </summary>
/// <remarks>
/// Only available for playlists created by the user, else <see cref="Public"/>.
/// </remarks>
public enum PlaylistPrivacy
{
    /// <summary>
    /// The playlist is public and can be viewed by anyone.
    /// </summary>
    Public,

    /// <summary>
    /// The playlist is unlisted and can only be viewed by those who have the link.
    /// </summary>
    Unlisted,

    /// <summary>
    /// The playlist is private and can only be viewed by the owner.
    /// </summary>
    Private
}