using System.Text.Json;
using YouTubeMusicAPI.Exceptions;
using YouTubeMusicAPI.Http;
using YouTubeMusicAPI.Models;

namespace YouTubeMusicAPI.Utils;

/// <summary>
/// Contains extension methods for various use cases.
/// </summary>
internal static class Extensions
{
    /// <summary>
    /// Creates a new instance of a <see cref="Client"/> based on the specified <see cref="ClientType"/>.
    /// </summary>
    /// <param name="type">The type of client to create.</param>
    /// <returns>A new <see cref="Client"/> instance.</returns>
    /// <exception cref="UnknownClientException">Occurs when an invalid client type is passed.</exception>
    public static Client? Create(
        this ClientType type) =>
        type switch
        {
            ClientType.None => null,
            ClientType.WebMusic => Client.WebMusic.Clone(),
            ClientType.IOS => Client.IOS.Clone(),
            ClientType.Tv => Client.Tv.Clone(),
            _ => throw new UnknownClientException(type)
        };


    /// <summary>
    /// Converts a string to a <see cref="TimeSpan"/> using the invariant culture.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>A TimeSpan representing the string.</returns>
    public static TimeSpan ToTimeSpan(
        this string text)
    {
        if (TimeSpan.TryParseExact(text, @"m\:ss", null, out TimeSpan timeSpan))
            return timeSpan;
        if (TimeSpan.TryParseExact(text, @"mm\:ss", null, out timeSpan))
            return timeSpan;
        if (TimeSpan.TryParseExact(text, @"h\:mm\:ss", null, out timeSpan))
            return timeSpan;
        if (TimeSpan.TryParseExact(text, @"hh\:mm\:ss", null, out timeSpan))
            return timeSpan;

        throw new ArgumentException($"The TimeSpan string '{text}' could not be parsed");
    }


    /// <summary>
    /// Looks for a property named <paramref name="propertyName"/> in the current object.
    /// </summary>
    /// <param name="element">The elemnt to search on.</param>
    /// <param name="propertyName">Name of the property to find.</param>
    /// <returns>The property if it exists. If not, null is returned.</returns>
    public static JsonElement? GetPropertyOrNull(
        this JsonElement element,
        string propertyName)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property))
            return property;

        return null;
    }

    /// <summary>
    /// Looks for a property at a specific index.
    /// </summary>
    /// <param name="element">The elemnt to get the item on.</param>
    /// <param name="index">The index to lookup.</param>
    /// <returns>The item at the specific index.</returns>
    /// <exception cref="IndexOutOfRangeException">Occurrs when if the index is out of bounds.</exception>
    public static JsonElement GetElementAt(
        this JsonElement element,
        int index)
    {
        if (index < 0 || index >= element.GetArrayLength())
            throw new IndexOutOfRangeException($"Index {index} is out of bounds for array with length {element.GetArrayLength()}.");

        return element[index];
    }
    /// <summary>
    /// Looks for a property at a specific index or returns null if not found.
    /// </summary>
    /// <param name="element">The elemnt to get the item on.</param>
    /// <param name="index">The index to lookup.</param>
    /// <returns>The item at the specific index.</returns>
    public static JsonElement? GetElementAtOrNull(
        this JsonElement element,
        int index)
    {
        if (index < 0 || index >= element.GetArrayLength())
            return null;

        return element[index];
    }

    /// <summary>
    /// Gets the value of the element as a <see cref="string"/>.
    /// </summary>
    /// <param name="element">The elemnt to get the string from.</param>
    /// <returns>The string value of the property.</returns>
    public static string GetStringOrEmpty(
        this JsonElement element) =>
        element.GetString() ?? string.Empty;


    /// <summary>
    /// Selects the navigation browse endpoint ID from a JSON element.
    /// </summary>
    /// <param name="element">The element containing "navigationEndpoint.browseEndpoint.browseId".</param>
    /// <returns></returns>
    public static string SelectNavigationBrowseId(
        this JsonElement element) =>
        element
            .GetProperty("navigationEndpoint")
            .GetProperty("browseEndpoint")
            .GetProperty("browseId")
            .GetStringOrEmpty();
    /// <summary>
    /// Selects the navigation browse endpoint ID from a JSON element or null if not found.
    /// </summary>
    /// <param name="element">The element containing "navigationEndpoint.browseEndpoint.browseId".</param>
    /// <returns></returns>
    public static string? SelectNavigationBrowseIdOrNull(
        this JsonElement element) =>
        element
            .GetPropertyOrNull("navigationEndpoint")
            ?.GetPropertyOrNull("browseEndpoint")
            ?.GetPropertyOrNull("browseId")
            ?.GetString();

    /// <summary>
    /// Selects the navigation watch endpoint ID from a JSON element.
    /// </summary>
    /// <param name="element">The element containing "navigationEndpoint.watchEndpoint.videoId".</param>
    /// <returns></returns>
    public static string SelectNavigationWatchId(
        this JsonElement element) =>
        element
            .GetProperty("navigationEndpoint")
            .GetProperty("watchEndpoint")
            .GetProperty("videoId")
            .GetStringOrEmpty();

    /// <summary>
    /// Selects the thumbnails from a JSON element.
    /// </summary>
    /// <param name="element">The elemnt containing "musicThumbnailRenderer.thumbnail.thumbnails".</param>
    /// <returns>An array containing the thumbnails.</returns>
    public static Thumbnail[] SelectThumbnails(
        this JsonElement element)
    {
        JsonElement thumbnails = element
            .GetProperty("musicThumbnailRenderer")
            .GetProperty("thumbnail")
            .GetProperty("thumbnails");

        List<Thumbnail> result = [];
        foreach (JsonElement item in thumbnails.EnumerateArray())
        {
            Thumbnail thumbnail = Thumbnail.Parse(item);
            result.Add(thumbnail);
        }

        return [.. result];
    }

    /// <summary>
    /// Selects a YouTubeMusicEntity with a text and navigation browse endpoint ID.
    /// </summary>
    /// <param name="element">The element containing "text" and "navigationEndpoint.browseEndpoint.browseId".</param>
    /// <returns>An array containing the artists.</returns>
    public static YouTubeMusicEntity SelectYouTubeMusicEntity(
        this JsonElement element)
    {
        string text = element
            .GetProperty("text")
            .GetStringOrEmpty();

        string? id = element
            .SelectNavigationBrowseIdOrNull();

        return new(text, id);
    }

    /// <summary>
    /// Selects the artists from a JSON element.
    /// </summary>
    /// <param name="element">The array element.</param>
    /// <returns>An array containing the artists.</returns>
    public static YouTubeMusicEntity[] SelectArtists(
        this JsonElement element)
    {
        List<YouTubeMusicEntity> result = [];
        foreach (JsonElement run in element.EnumerateArray())
        {
            string text = run
                .GetProperty("text")
                .GetStringOrEmpty()
                .Trim();

            switch (text)
            {
                case "•":
                    return [.. result];
                case "&":
                case ",":
                    continue;
            }

            string? id = run
                .SelectNavigationBrowseIdOrNull();

            result.Add(new(text, id));
        }

        return [.. result];
    }

    public static Radio? SelectRadioOrNull(
        this JsonElement element)
    {
        foreach (JsonElement item in element.EnumerateArray())
        {
            JsonElement menu = item
                .GetProperty("menuNavigationItemRenderer");

            string type = menu
                .GetProperty("text")
                .GetProperty("runs")
                .GetElementAt(0)
                .GetProperty("text")
                .GetStringOrEmpty();

            if (type != "Start radio")
                continue;

            string playlistId = menu
                .GetProperty("navigationEndpoint")
                .GetProperty("watchEndpoint")
                .GetProperty("playlistId")
                .GetStringOrEmpty();

            string? songVideoId = menu
                .GetProperty("navigationEndpoint")
                .GetProperty("watchEndpoint")
                .GetPropertyOrNull("videoId")
                ?.GetString();

            return new(playlistId, songVideoId);
        }

        return null;
    }

    /// <summary>
    /// Selects whether the given JSON element contains an explicit badge.
    /// </summary>
    /// <param name="element">The element containing "musicInlineBadgeRenderer.icon.iconType".</param>
    /// <returns>A boolean indicating wether a explicit badge is contained.</returns>
    public static bool SelectContainsExplicitBadge(
        this JsonElement? element)
    {
        if (element.HasValue)
            foreach (JsonElement badge in element.Value.EnumerateArray())
            {
                string? iconType = badge
                    .GetPropertyOrNull("musicInlineBadgeRenderer")
                    ?.GetPropertyOrNull("icon")
                    ?.GetPropertyOrNull("iconType")
                    ?.GetString();

                if (iconType == "MUSIC_EXPLICIT_BADGE")
                    return true;
            }

        return false;
    }
}