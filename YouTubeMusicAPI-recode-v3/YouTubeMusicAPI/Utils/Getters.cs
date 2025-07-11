using System.Text.Json;

namespace YouTubeMusicAPI.Utils;

/// <summary>
/// Contains extension methods for getting properties from JSON elements.
/// </summary>
internal static class Getters
{
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
        int index) =>
        element[index];
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
}