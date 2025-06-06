﻿namespace YouTubeMusicAPI.Models;

/// <summary>
/// Represents a identifiable named entity on YouTube Music
/// </summary>
/// <param name="name">The name of this entity</param>
/// <param name="id">The id of this entity</param>
public class NamedEntity(
    string name,
    string? id)
{

    /// <summary>
    /// The name of this entity
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The id of this entity
    /// </summary>
    public virtual string? Id { get; } = id;
}