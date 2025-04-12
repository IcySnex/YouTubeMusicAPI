namespace YouTubeMusicAPI.Models.Info;

/// <summary>
/// Contains information about a YouTube Music song in a community playlist
/// </summary>
/// <param name="name">The name of the community playlist song</param>
/// <param name="id">The id of the community playlist song</param>
/// <param name="artists">The artist of this community playlist song</param>
/// <param name="album">The album of this community playlist song</param>
/// <param name="isExplicit">Weither the community playlist song is explicit or not</param>
/// <param name="duration">The duration of the community playlist song</param>
/// <param name="thumbnails">The thumbnails of the community playlist song</param>
public class CommunityPlaylistSongInfo(
    string name,
    string? id,
    NamedEntity[] artists,
    NamedEntity? album,
    bool isExplicit,
    TimeSpan duration,
    Thumbnail[] thumbnails)
{
    /// <summary>
    /// The name of the community playlist song
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of the community playlist song
    /// </summary>
    public string? Id { get; } = id;

    /// <summary>
    /// The artist of this community playlist song
    /// </summary>
    public NamedEntity[] Artists { get; } = artists;

    /// <summary>
    /// The album of this community playlist song
    /// </summary>
    public NamedEntity? Album { get; } = album;

    /// <summary>
    /// Weither the community playlist song is explicit or not
    /// </summary>
    public bool IsExplicit { get; } = isExplicit;

    /// <summary>
    /// The duration of the community playlist song
    /// </summary>
    public TimeSpan Duration { get; } = duration;

    /// <summary>
    /// The thumbnails of the community playlist song
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;
}