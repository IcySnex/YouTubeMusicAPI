using System.Diagnostics;
using System.Text.Json;

namespace YouTubeMusicAPI.Json;

/// <summary>
/// A lightweight wrapper around <see cref="JsonElement"/> that provides safer and more convenient access to the JSON data.
/// </summary>
[DebuggerDisplay("ValueKind = {element.ValueKind} : {path}")]
internal readonly struct JElement
{
    readonly JsonElement element;
#if DEBUG
    readonly string path;
    readonly string rawText;
#endif

    /// <summary>
    /// Creates a new instance of the <see cref="JElement"/> struct.
    /// </summary>
    /// <param name="element">The underlying <see cref="JsonElement"/> to wrap.</param>
    /// <param name="path">The JSON path for debugging purposes (only used in DEBUG builds).</param>
    public JElement(
        JsonElement element
#if DEBUG
        , string path = "$"
#endif
        )
    {
        this.element = element;
#if DEBUG
        this.path = path;
        rawText = element.ToString();
#endif
    }


    /// <summary>
    /// Whether this element is undefined.
    /// </summary>
    public bool IsUndefined => element.ValueKind == JsonValueKind.Undefined;

    /// <summary>
    /// Whether this element is an array.
    /// </summary>
    public bool IsArray => element.ValueKind == JsonValueKind.Array;

    /// <summary>
    /// The length of the array if this element is an array; otherwise returns -1.
    /// </summary>
    public int ArrayLength => IsArray ? element.GetArrayLength() : -1;


    /// <summary>
    /// Gets the property with the specified key.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <returns>A <see cref="JElement"/> representing the value, or an undefined element if the property does not exist.</returns>
    public JElement Get(
        string key)
    {
        if (!IsUndefined && element.TryGetProperty(key, out JsonElement val))
        {
#if DEBUG
            return new(val, $"{path}.{key}");
#else
            return new(val);
#endif
        }

#if DEBUG
        return new(default, $"{path}.{key}<MISSING>");
#else
        return new(default);
#endif
    }

    /// <summary>
    /// Gets the element at the specified index if this element is an array.
    /// </summary>
    /// <param name="index">The index of the element to retrieve.</param>
    /// <returns>A <see cref="JElement"/> representing the element, or an undefined element if the index is out of bounds.</returns>
    public JElement GetAt(
        int index)
    {
        if (IsArray && index >= 0 && index < ArrayLength)
        {
#if DEBUG
            return new(element[index], $"{path}[{index}]");
#else
            return new(element[index]);
#endif
        }

#if DEBUG
        return new(default, $"{path}[{index}]");
#else
        return new(default);
#endif
    }


    /// <summary>
    /// Determines whether this element contains the specified property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <returns> <see langword="true"/> if the property exists; otherwise <see langword="false"/>.</returns>
    public bool Contains(
        string key)
    {
        if (IsUndefined)
            return false;

        return element.TryGetProperty(key, out _);
    }

    /// <summary>
    /// Determines whether this element contains the specified property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <param name="value">The resulting <see cref="JElement"/> value if the property exists.</param>
    /// <returns> <see langword="true"/> if the property exists; otherwise <see langword="false"/>.</returns>
    public bool Contains(
        string key,
        out JElement value)
    {
        if (IsUndefined)
        {
#if DEBUG
            value = new(default, $"{path}.{key}<MISSING>");
#else
            value = new(default);
#endif
            return false;
        }

        bool result = element.TryGetProperty(key, out JsonElement val);

#if DEBUG
        value = new(val, $"{path}.{key}");
#else
        value = new(val);
#endif
        return result;
    }


    /// <summary>
    /// Converts this element to a <see cref="JArray"/> if possible.
    /// </summary>
    /// <returns>A <see cref="JArray"/> instance, or <see langword="null"/> if this element is not an array.</returns>
    public JArray? AsArray()
    {
        if (IsArray)
        {
#if DEBUG
            return new(element, path);
#else
            return new(element);
#endif
        }

        return null;
    }

    /// <summary>
    /// Converts this element to a <see cref="string"/> if possible.
    /// </summary>
    /// <returns>The string value, or <see langword="null"/> if this element is not a string.</returns>
    public string? AsString()
    {
        if (element.ValueKind == JsonValueKind.String && element.GetString() is string value)
            return value;

        return null;
    }

    /// <summary>
    /// Converts this element to an <see cref="int"/> if possible.
    /// </summary>
    /// <returns>The int value, or <see langword="null"/> if this element is not an int.</returns>
    public int? AsInt32()
    {
        if (element.TryGetInt32(out int value))
            return value;

        return null;
    }

    /// <summary>
    /// Converts this element to a <see cref="bool"/> if possible.
    /// </summary>
    /// <returns>The bool value, or <see langword="null"/> if this element is not a bool.</returns>
    public bool? AsBool()
    {
        if (element.ValueKind == JsonValueKind.True || element.ValueKind == JsonValueKind.False)
            return element.GetBoolean();

        return null;
    }
}