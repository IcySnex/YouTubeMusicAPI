using System.Globalization;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Models;

namespace YouTubeMusicAPI.Utils;

/// <summary>
/// Contains extension methods for converting data types.
/// </summary>
internal static class Conversion
{
    /// <summary>
    /// Converts a <see cref="ClientType"/> to a <see cref="Client"/>.
    /// </summary>
    /// <param name="type">The client type.</param>
    /// <returns>A <see cref="Client"/> representing the type.</returns>
    /// <exception cref="ArgumentException">Occurs when an invalid client type is passed.</exception>
    public static Client? ToClient(
        this ClientType type) =>
        type switch
        {
            ClientType.None => null,
            ClientType.WebMusic => Client.WebMusic.Clone(),
            ClientType.IOS => Client.IOS.Clone(),
            ClientType.Tv => Client.Tv.Clone(),
            _ => throw new ArgumentException($"Invalid client type: {type}.", nameof(type))
        };

    /// <summary>
    /// Converts a <see langword="string"/> to a <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>A <see cref="TimeSpan"/> representing the text.</returns>
    public static TimeSpan? ToTimeSpan(
        this string? text)
    {
        if (TimeSpan.TryParseExact(text, @"m\:ss", CultureInfo.InvariantCulture, out TimeSpan result))
            return result;
        if (TimeSpan.TryParseExact(text, @"mm\:ss", CultureInfo.InvariantCulture, out result))
            return result;
        if (TimeSpan.TryParseExact(text, @"h\:mm\:ss", CultureInfo.InvariantCulture, out result))
            return result;
        if (TimeSpan.TryParseExact(text, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts a <see langword="string"/> to a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>A  <see cref="DateTime"/> representing the text.</returns>
    public static DateTime? ToDateTime(
        this string? text)
    {
        if (text is null)
            return null;

        if (!text.Contains(" ago") && DateTime.TryParse(text, CultureInfo.InvariantCulture, out DateTime result))
            return result;

        string[] timeSpanParts = text.Split(' ');
        int timeSpanValue = int.Parse(timeSpanParts[0]);
        string timeSpanKind = timeSpanParts[1];

        return timeSpanKind[0] switch
        {
            'd' => DateTime.Now - TimeSpan.FromDays(timeSpanValue),
            'h' => DateTime.Now - TimeSpan.FromHours(timeSpanValue),
            'm' => DateTime.Now - TimeSpan.FromMinutes(timeSpanValue),
            's' => DateTime.Now - TimeSpan.FromSeconds(timeSpanValue),
            _ => null
        };
    }

    /// <summary>
    /// Converts a <see langword="string"/> to a <see cref="int"/>.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>An <see cref="int"/> representing the text.</returns>
    public static int? ToInt32(
        this string? text)
    {
        if (int.TryParse(text, CultureInfo.InvariantCulture, out int result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts a <see langword="string"/> to a <see cref="AlbumType"/>.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>An <see cref="AlbumType"/> representing the text.</returns>
    public static AlbumType? ToAlbumType(
        this string? text) =>
        text switch
        {
            "Album" => (AlbumType?)AlbumType.Album,
            "Single" => (AlbumType?)AlbumType.Single,
            "EP" => (AlbumType?)AlbumType.Ep,
            _ => null,
        };
}