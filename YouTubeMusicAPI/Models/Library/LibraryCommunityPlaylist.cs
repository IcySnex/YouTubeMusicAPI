﻿namespace YouTubeMusicAPI.Models.Library;

/// <summary>
/// Represents a YouTube Music library community playlist
/// </summary>
/// <param name="name">The name of this community playlist</param>
/// <param name="id">The id of this community playlist</param>
/// <param name="creator">The creator of this community playlist</param>
/// <param name="songCount">The count of songs in the community playlist</param>
/// <param name="radio">The radio channel of this community playlist</param>
/// <param name="thumbnails">The thumbnails of this community playlist</param>
public class LibraryCommunityPlaylist(
    string name,
    string id,
    YouTubeMusicItem creator,
    int songCount,
    Radio? radio,
    Thumbnail[] thumbnails)
{
    /// <summary>
    /// The name of this community playlist
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of this community playlist
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The creator of this community playlist
    /// </summary>
    public YouTubeMusicItem Creator { get; } = creator;

    /// <summary>
    /// The count of songs in the community playlist
    /// </summary>
    public int SongCount { get; } = songCount;

    /// <summary>
    /// The radio channel of this community playlist
    /// </summary>
    public Radio? Radio { get; } = radio;

    /// <summary>
    /// The thumbnails of this community playlist
    /// </summary>
    public Thumbnail[] Thumbnails { get; } = thumbnails;
}