using System.Collections;
using System.Diagnostics;
using System.Text.Json;

namespace YouTubeMusicAPI.Json;

/// <summary>
/// A lightweight wrapper around <see cref="JsonElement"/> that represents a JSON array and provides safer and more convenient access to the JSON data.
/// </summary>
[DebuggerDisplay("JArray[{Length}] : {path}")]
internal sealed class JArray : IEnumerable<JElement>
{
    /// <summary>
    /// Represents an empty <see cref="JArray"/>.
    /// </summary>
    public static JArray Empty { get; } = new();


    readonly JsonElement element;
#if DEBUG
    readonly string path;
#endif

    /// <summary>
    /// Creates a new instance of the <see cref="JArray"/> class.
    /// </summary>
    /// <param name="element">The underlying <see cref="JsonElement"/> that must be an array.</param>
    /// <param name="path">The JSON path for debugging purposes (only used in DEBUG builds).</param>
    /// <exception cref="ArgumentException">Occurs when the provided <paramref name="element"/> is not an array.</exception>
    public JArray(
        JsonElement element
#if DEBUG
        , string path = "$"
#endif
        )
    {
        if (element.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("JsonElement is not an array.", nameof(element));

        this.element = element;
#if DEBUG
        this.path = path;
#endif
    }

    JArray()
    {
        element = default;
#if DEBUG
        path = "$";
#endif
    }


    /// <summary>
    /// The number of elements in the array.
    /// </summary>
    public int Length => element.ValueKind == JsonValueKind.Array ? element.GetArrayLength() : -1;

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>A <see cref="JElement"/> representing the array element, or an undefined element if the index is out of bounds.</returns>
    public JElement this[int index]
    {
        get
        {
            if (index >= 0 && index < Length)
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
    }


    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="JArray"/>.
    /// </summary>
    /// <returns>An enumerator of <see cref="JElement"/>.</returns>
    public IEnumerator<JElement> GetEnumerator()
    {
        for (int i = 0; i < Length; i++)
        {
#if DEBUG
            yield return new(element[i], $"{path}[{i}]");
#else
            yield return new(element[i]);
#endif
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="JArray"/>.
    /// </summary>
    /// <returns>An enumerator of <see cref="JElement"/>.</returns>
    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}