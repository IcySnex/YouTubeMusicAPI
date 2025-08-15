using System.Text.Json;

namespace YouTubeMusicAPI.Utils;

/// <summary>
/// Contains extension methods for getting properties from JSON elements.
/// </summary>
internal static class Getters
{
    /// <summary>
    /// Returns the property with the specified <c>propertyName</c> if it exists, otherwise <see langword="null"/>.
    /// </summary>
    /// <param name="element">The JSON element to get the property from.</param>
    /// <param name="propertyName">The name of the property to get.</param>
    /// <returns>The <see cref="JsonElement"/> if the property exists, otherwise <see langword="null"/>.</returns>
    public static JsonElement? GetPropertyOrNull(
        this JsonElement element,
        string propertyName)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property))
            return property;

        return null;
    }

    /// <summary>
    /// Returns the JSON element at the specified <c>index</c>.
    /// </summary>
    /// <param name="element">The JSON array element to index from.</param>
    /// <param name="index">The index of the element to get.</param>
    /// <returns>The <see cref="JsonElement"/> at the specified <c>index</c>.</returns>
    public static JsonElement GetPropertyAt(
        this JsonElement element,
        int index) =>
        element[index];

    /// <summary>
    /// Returns the JSON element at the specified <c>index</c> if it exists, otherwise <see langword="null"/>.
    /// </summary>
    /// <param name="element">The JSON array element to index from.</param>
    /// <param name="index">The index of the element to get.</param>
    /// <returns>The <see cref="JsonElement"/> at the specified <c>index</c> if valid, otherwise <see langword="null"/>.</returns>
    public static JsonElement? GetPropertyAtOrNull(
        this JsonElement element,
        int index)
    {
        if (index < 0 || index >= element.GetArrayLength())
            return null;

        return element[index];
    }
}
