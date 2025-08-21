using System.Globalization;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Search;

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
            ClientType.IOSMusic => Client.IOSMusic.Clone(),
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
        if (text is null)
            return null;

        string[] parts = text.Split(':');
        return parts.Length switch
        {
            2 when int.TryParse(parts[0], out int minutes) && int.TryParse(parts[1], out int seconds) => new(0, minutes, seconds),
            3 when int.TryParse(parts[0], out int hours) && int.TryParse(parts[1], out int minutes) && int.TryParse(parts[2], out int seconds) => new(hours, minutes, seconds),
            _ => null,
        };
    }

    /// <summary>
    /// Converts a milliseconds <see langword="long"/> to a <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="milliseconds">The milliseconds to convert.</param>
    /// <returns>A <see cref="TimeSpan"/> representing the milliseconds.</returns>
    public static TimeSpan? ToTimeSpan(
        this long? milliseconds)
    {
        if (milliseconds.HasValue)
            return TimeSpan.FromMilliseconds(milliseconds.Value);

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

        if (!text.Contains("ago") && DateTime.TryParse(text, CultureInfo.InvariantCulture, out DateTime result))
            return result;

        text = text.Replace(" ago", "").Trim();

        int i = 0;
        while (i < text.Length && char.IsDigit(text[i]))
            i++;

        if (i == 0)
            return null;

        int timeSpanValue = int.Parse(text.Substring(0, i));
        string timeSpanKind = text.Substring(i).Trim();

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
    /// Converts a <see langword="string"/> to an <see cref="int"/>.
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
    /// Converts a <see langword="string"/> to a <see cref="long"/>.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>A <see cref="long"/> representing the text.</returns>
    public static long? ToInt64(
        this string? text)
    {
        if (long.TryParse(text, CultureInfo.InvariantCulture, out long result))
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


    /// <summary>
    /// Converts a <see cref="SearchScope"/>, a <see cref="SearchCategory"/> and <see cref="bool"/> representing weither to ignore spelling to a query parameter.
    /// </summary>
    /// <param name="category">The category of content to search for.</param>
    /// <param name="scope">The scope of the search.</param>
    /// <param name="ignoreSpelling">Weither to ignore spelling suggestions.</param>
    /// <returns>A <see cref="string"/> representing the search query parameter.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Occurs when the given <see cref="SearchCategory"/> is invalid.</exception>
    public static string ToQueryParams(
        this SearchScope scope,
        SearchCategory? category,
        bool ignoreSpelling)
    {
        static string GetQueryParam2(SearchCategory category) =>
            category switch
            {
                SearchCategory.Songs => "II",
                SearchCategory.Videos => "IQ",
                SearchCategory.Albums => "IY",
                SearchCategory.Artists => "Ig",
                SearchCategory.CommunityPlaylists => "EA",
                SearchCategory.FeaturedPlaylists => "Dg",
                SearchCategory.Profiles => "JY",
                SearchCategory.Podcasts => "JQ",
                SearchCategory.Episodes => "JI",
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, "The given SearchCategory is invalid.")
            };

        const string filteredQueryParam1 = "EgWKAQ";

        string? queryParams = null;
        string queryParam1 = "";
        string queryParam2 = "";
        string queryParam3 = "";

        if (!category.HasValue && scope == SearchScope.Global && !ignoreSpelling)
            return "";

        switch (scope)
        {
            //case SearchScope.Uploads:
            //    queryParams = "agIYAw%3D%3D";
            //    break;

            case SearchScope.Library:
                if (!category.HasValue)
                {
                    queryParams = "agIYBA%3D%3D";
                    break;
                }

                queryParam1 = filteredQueryParam1;
                queryParam2 = GetQueryParam2(category.Value);
                queryParam3 = "AWoKEAUQCRADEAoYBA%3D%3D";
                break;

            case SearchScope.Global:
                if (!category.HasValue)
                {
                    queryParams = "EhGKAQ4IARABGAEgASgAOAFAAUICCAE%3D";
                    break;
                }

                if (category.Value == SearchCategory.CommunityPlaylists || category.Value == SearchCategory.FeaturedPlaylists)
                {
                    queryParam1 = "EgeKAQQoA";
                    queryParam3 = ignoreSpelling ? "BQgIIAWoMEA4QChADEAQQCRAF" : "BagwQDhAKEAMQBBAJEAU%3D";
                }
                else
                {
                    queryParam1 = filteredQueryParam1;
                    queryParam3 = ignoreSpelling ? "AUICCAFqDBAOEAoQAxAEEAkQBQ%3D%3D" : "AWoMEA4QChADEAQQCRAF";
                }
                queryParam2 = GetQueryParam2(category.Value);
                break;
        }

        return queryParams is null ? queryParam1 + queryParam2 + queryParam3 : queryParams;
    }
}