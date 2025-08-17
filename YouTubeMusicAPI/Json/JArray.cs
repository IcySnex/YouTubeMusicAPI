using System.Collections;
using System.Text.Json;

namespace YouTubeMusicAPI.Json;

internal class JArray : IEnumerable<JElement>
{
    public static JArray Empty { get; } = new();


    readonly JsonElement element;

    public JArray(
        JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("JsonElement is not an array.", nameof(element));

        this.element = element;

        Count = element.GetArrayLength();
    }

    private JArray()
    {
        element = default;
        Count = 0;
    }



#if DEBUG
    readonly string path = "$";

    public JArray(
        JsonElement element,
        string path) : this(element)
    {
        this.path = path;
    }
#endif

    public int Count { get; }

    public JElement this[int index]
    {
        get
        {
            if (index >= 0 && index < Count)
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

    public IEnumerator<JElement> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
#if DEBUG
            yield return new(element[i], $"{path}[{i}]");
#else
            yield return new(element[i]);
#endif
        }
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}