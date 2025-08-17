using System.Text.Json;

namespace YouTubeMusicAPI.Json;

[System.Diagnostics.DebuggerDisplay("ValueKind = {element.ValueKind} : {path}")]
internal readonly struct JElement(
    JsonElement element)
{
    readonly JsonElement element = element;

#if DEBUG
    readonly string path = "$";
    readonly string rawText = "";

    public JElement(
        JsonElement element,
        string path) : this(element)
    {
        this.path = path;

        rawText = element.ToString();
    }
#endif

    public bool IsUndefined => element.ValueKind == JsonValueKind.Undefined;


    public JElement Get(
        string key)
    {
        if (!IsUndefined && element.TryGetProperty(key, out JsonElement value))
        {
#if DEBUG
            return new(value, $"{path}.{key}");
#else
            return new(value);
#endif
        }

#if DEBUG
        return new(default, $"{path}.{key}<MISSING>");
#else
            return new(default);
#endif
    }

    public JElement GetAt(
        int index)
    {
        if (!IsUndefined && index >= 0 && index < element.GetArrayLength())
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


    public bool Contains(
        string key)
    {
        if (!IsUndefined)
            return element.TryGetProperty(key, out _);

        return false;
    }


    public JArray? AsArray()
    {
        if (element.ValueKind == JsonValueKind.Array)
            return new(element, path);

        return null;
    }

    public string? AsString()
    {
        if (element.ValueKind == JsonValueKind.String && element.GetString() is string value)
            return value;

        return null;
    }

    public int? AsInt32()
    {
        if (element.TryGetInt32(out int value))
            return value;

        return null;
    }

    public bool? AsBool()
    {
        if (element.ValueKind == JsonValueKind.True || element.ValueKind == JsonValueKind.False)
            return element.GetBoolean();

        return null;
    }
}