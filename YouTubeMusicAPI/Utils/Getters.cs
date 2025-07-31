using System.Text.Json;

namespace YouTubeMusicAPI.Utils;

/// <summary>
/// Contains extension methods for getting properties from JSON elements.
/// </summary>
internal static class Getters
{
    public static JsonElement? GetPropertyOrNull(
        this JsonElement element,
        string propertyName)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property))
            return property;

        return null;
    }


    public static JsonElement GetElementAt(
        this JsonElement element,
        int index) =>
        element[index];

    public static JsonElement? GetElementAtOrNull(
        this JsonElement element,
        int index)
    {
        if (index < 0 || index >= element.GetArrayLength())
            return null;

        return element[index];
    }
}