using System.Collections.Immutable;
using System.Text.Json;
using OpenFeature.Model;

namespace OpenFeature.E2ETests.Utils;

public static class JsonStructureLoader
{
    public static Value ParseJsonValue(string raw)
    {
        var json = UnescapeGherkinJson(raw);
        if (json == "{}")
            return new Value(Structure.Empty);

        using var doc = JsonDocument.Parse(json);
        return ConvertElement(doc.RootElement);
    }

    private static string UnescapeGherkinJson(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return s;

        // Replace escaped quotes, if still present.
        if (s.Contains("\\\""))
            s = s.Replace("\\\"", "\"");

        // Trim wrapping quotes "\"{...}\"" if present.
        if (s.Length > 2 && s[0] == '"' && s[s.Length - 1] == '"' && s[1] == '{' && s[s.Length - 2] == '}')
        {
            var inner = s.Substring(1, s.Length - 2);
            if (inner.StartsWith("{") && inner.EndsWith("}"))
                s = inner;
        }

        return s.Trim();
    }

    private static Structure ConvertObject(JsonElement element)
    {
        var dict = new Dictionary<string, Value>(StringComparer.Ordinal);
        foreach (var prop in element.EnumerateObject())
        {
            dict[prop.Name] = ConvertElement(prop.Value);
        }
        return new Structure(dict);
    }

    private static Value ConvertElement(JsonElement el) =>
        el.ValueKind switch
        {
            JsonValueKind.Object => new Value(ConvertObject(el)),
            JsonValueKind.Array => new Value(el.EnumerateArray().Select(ConvertElement).ToImmutableList()),
            JsonValueKind.String => new Value(el.GetString()!),
            JsonValueKind.Number => ConvertNumber(el),
            JsonValueKind.True => new Value(true),
            JsonValueKind.False => new Value(false),
            JsonValueKind.Null => new Value(), // null inner value
            JsonValueKind.Undefined => new Value(),
            _ => throw new ArgumentOutOfRangeException(nameof(el), $"Unsupported JSON token: {el.ValueKind}")
        };

    private static Value ConvertNumber(JsonElement el)
    {
        // Prefer int when representable; Value(int) internally stores as double.
        if (el.TryGetInt64(out var l) && l is >= int.MinValue and <= int.MaxValue)
        {
            return new Value((int)l);
        }
        return new Value(el.GetDouble());
    }
}
