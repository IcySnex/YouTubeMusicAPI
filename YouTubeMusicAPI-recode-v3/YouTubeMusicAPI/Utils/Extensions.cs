using System.Globalization;
using System.Runtime.CompilerServices;
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
    /// Returns the value if it is not <c>null</c>; otherwise, returns the specified default value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="defaultValue">The value to return if <paramref name="value"/> is <c>null</c>.</param>
    /// <returns><paramref name="value"/> if it is not <c>null</c>; otherwise, <paramref name="defaultValue"/>.</returns>
    public static T Or<T>(
        this T? value,
        T defaultValue) where T : struct =>
        value ?? defaultValue;
    /// <summary>
    /// Returns the value if it is not <c>null</c>; otherwise, returns the specified default value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="defaultValue">The value to return if <paramref name="value"/> is <c>null</c>.</param>
    /// <returns><paramref name="value"/> if it is not <c>null</c>; otherwise, <paramref name="defaultValue"/>.</returns>
    public static T Or<T>(
        this T? value,
        T defaultValue) where T : class =>
        value ?? defaultValue;


    /// <summary>
    /// Returns the value if it is not <c>null</c>; otherwise, throws a <see cref="NullReferenceException"/> with the original expression text included in the message.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="expression">The original expression that produced the value (automatically provided by the compiler).</param>
    /// <returns><paramref name="value"/> if it is not <c>null</c>.</returns>
    /// <exception cref="NullReferenceException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static T OrThrow<T>(
        this T? value,
        [CallerArgumentExpression(nameof(value))] string? expression = null) where T : struct =>
        value ?? throw new NullReferenceException($"Value was null: {expression}");
    /// <summary>
    /// Returns the value if it is not <c>null</c>; otherwise, throws a <see cref="NullReferenceException"/> with the original expression text included in the message.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="expression">The original expression that produced the value (automatically provided by the compiler).</param>
    /// <returns><paramref name="value"/> if it is not <c>null</c>.</returns>
    /// <exception cref="NullReferenceException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static T OrThrow<T>(
        this T? value,
        [CallerArgumentExpression(nameof(value))] string? expression = null) where T : class =>
        value ?? throw new NullReferenceException($"Value was null: {expression}");


    /// <summary>
    /// Converts a string to a <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>A TimeSpan representing the string.</returns>
    public static TimeSpan? ToTimeSpan(
        this string? text)
    {
        if (TimeSpan.TryParseExact(text, @"m\:ss", CultureInfo.InvariantCulture, out TimeSpan timeSpan))
            return timeSpan;
        if (TimeSpan.TryParseExact(text, @"mm\:ss", CultureInfo.InvariantCulture, out timeSpan))
            return timeSpan;
        if (TimeSpan.TryParseExact(text, @"h\:mm\:ss", CultureInfo.InvariantCulture, out timeSpan))
            return timeSpan;
        if (TimeSpan.TryParseExact(text, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out timeSpan))
            return timeSpan;

        return null;
    }

    /// <summary>
    /// Converts a string to a <see cref="int"/>.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>An Int32 representing the string.</returns>
    public static int? ToInt32(
        this string? text)
    {
        if (int.TryParse(text, CultureInfo.InvariantCulture, out int result))
            return result;

        return null;
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
    /// <exception cref="IndexOutOfRangeException">Occurrs when the index is out of bounds.</exception>
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
            .GetString()
            .OrThrow();
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
    /// Selects the overlay navigation playlist endpoint ID from a JSON element.
    /// </summary>
    /// <param name="element">The element containing "overlay.musicItemThumbnailOverlayRenderer.content.musicPlayButtonRenderer.playNavigationEndpoint.watchPlaylistEndpoint.playlistId".</param>
    /// <returns></returns>
    public static string SelectOverlayNavigationPlaylistId(
        this JsonElement element) =>
        element
            .GetProperty("overlay")
            .GetProperty("musicItemThumbnailOverlayRenderer")
            .GetProperty("content")
            .GetProperty("musicPlayButtonRenderer")
            .GetProperty("playNavigationEndpoint")
            .GetProperty("watchPlaylistEndpoint")
            .GetProperty("playlistId")
            .GetString()
            .OrThrow();

    /// <summary>
    /// Selects the overlay navigation video endpoint ID from a JSON element.
    /// </summary>
    /// <param name="element">The element containing "overlay.musicItemThumbnailOverlayRenderer.content.musicPlayButtonRenderer.playNavigationEndpoint.watchEndpoint.videoId".</param>
    /// <returns></returns>
    public static string SelectOverlayNavigationVideoId(
        this JsonElement element) =>
        element
            .GetProperty("overlay")
            .GetProperty("musicItemThumbnailOverlayRenderer")
            .GetProperty("content")
            .GetProperty("musicPlayButtonRenderer")
            .GetProperty("playNavigationEndpoint")
            .GetProperty("watchEndpoint")
            .GetProperty("videoId")
            .GetString()
            .OrThrow();


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
            .GetString()
            .OrThrow();

        string? id = element
            .SelectNavigationBrowseIdOrNull();

        return new(text, id);
    }

    /// <summary>
    /// Selects the artists from a JSON element.
    /// </summary>
    /// <param name="element">The array element.</param>
    /// <param name="startIndex">The index to start from.</param>
    /// <returns>An array containing the artists.</returns>
    public static YouTubeMusicEntity[] SelectArtists(
        this JsonElement element,
        int startIndex = 0)
    {
        int index = 0;

        List<YouTubeMusicEntity> result = [];
        foreach (JsonElement run in element.EnumerateArray())
        {
            if (index++ < startIndex)
                continue;

            string text = run
                .GetProperty("text")
                .GetString()
                .OrThrow()
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
                .GetString()
                .OrThrow();

            if (type != "Start radio")
                continue;


            JsonElement navigationEndpoint = menu
                .GetProperty("navigationEndpoint");

            JsonElement? watchEndpoint =
                navigationEndpoint
                    .GetPropertyOrNull("watchEndpoint")
                ?? navigationEndpoint
                    .GetPropertyOrNull("watchPlaylistEndpoint");

            if (watchEndpoint is null)
                return null;


            string? playlistId = watchEndpoint
                ?.GetPropertyOrNull("playlistId")
                ?.GetString();

            if (playlistId is null)
                return null;

            string? songVideoId = watchEndpoint
                ?.GetPropertyOrNull("videoId")
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