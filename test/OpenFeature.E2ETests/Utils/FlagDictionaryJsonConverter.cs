using System.Text.Json;
using System.Text.Json.Serialization;
using OpenFeature.Model;
using OpenFeature.Providers.Memory;

namespace OpenFeature.E2ETests.Utils;

public sealed class FlagDictionaryJsonConverter : JsonConverter<Dictionary<string, Flag>>
{
    public override Dictionary<string, Flag> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Root of flags JSON must be an object");

        var result = new Dictionary<string, Flag>(StringComparer.Ordinal);

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected property name (flag key)");

            var flagKey = reader.GetString()!;
            reader.Read();

            var flagDoc = JsonDocument.ParseValue(ref reader);
            var flagElement = flagDoc.RootElement;
            result[flagKey] = ReadFlag(flagKey, flagElement);
        }

        return result;
    }

    private static Flag ReadFlag(string flagKey, JsonElement flagElement)
    {
        if (!flagElement.TryGetProperty("variants", out var variantsElement) || variantsElement.ValueKind != JsonValueKind.Object)
            throw new JsonException($"Flag '{flagKey}' is missing 'variants' object");

        // Infer variant type
        VariantKind? inferredKind = null;
        foreach (var v in variantsElement.EnumerateObject())
        {
            var kind = ClassifyVariantValue(v.Value);
            inferredKind = inferredKind == null ? kind : Promote(inferredKind.Value, kind);
        }

        if (inferredKind == null)
            throw new JsonException($"Flag '{flagKey}' has no variants");

        var defaultVariant = InferDefaultVariant(flagElement, variantsElement);

        var contextEvaluator = flagElement.TryGetProperty("contextEvaluator", out var ctxElem) && ctxElem.ValueKind == JsonValueKind.String
            ? ContextEvaluatorUtility.BuildContextEvaluator(ctxElem.GetString()!)
            : null;

        var metadata = flagElement.TryGetProperty("flagMetadata", out var metaElem) && metaElem.ValueKind == JsonValueKind.Object
            ? BuildMetadata(metaElem)
            : null;

        // NOTE: The current Flag<T> type does not model 'disabled'

        return inferredKind switch
        {
            VariantKind.Boolean => BuildFlag(variantsElement, defaultVariant, contextEvaluator, metadata, static e => e.GetBoolean()),
            VariantKind.Integer => BuildFlag(variantsElement, defaultVariant, contextEvaluator, metadata, static e => e.GetInt32()),
            VariantKind.Double => BuildFlag(variantsElement, defaultVariant, contextEvaluator, metadata, static e => e.GetDouble()),
            VariantKind.String => BuildFlag(variantsElement, defaultVariant, contextEvaluator, metadata, static e => e.GetString()!),
            VariantKind.Object => BuildFlag(variantsElement, defaultVariant, contextEvaluator, metadata, ExtractObjectVariant),
            _ => throw new JsonException($"Unsupported variant kind for flag '{flagKey}'")
        };
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, Flag> value, JsonSerializerOptions options)
        => throw new NotSupportedException("Serialization is not implemented.");

    private static Flag<T> BuildFlag<T>(
        JsonElement variantsElement,
        string? defaultVariant,
        Func<EvaluationContext, string>? contextEvaluator,
        ImmutableMetadata? metadata,
        Func<JsonElement, T> projector)
    {
        var dict = new Dictionary<string, T>(StringComparer.Ordinal);
        foreach (var v in variantsElement.EnumerateObject())
        {
            dict[v.Name] = projector(v.Value);
        }
        return new Flag<T>(dict, defaultVariant!, contextEvaluator, metadata);
    }

    private static string? InferDefaultVariant(JsonElement flagElement, JsonElement variantsElement)
    {
        if (flagElement.TryGetProperty("defaultVariant", out var dv))
        {
            if (dv.ValueKind == JsonValueKind.String)
                return dv.GetString()!;
        }

        return null;
    }

    private static ImmutableMetadata? BuildMetadata(JsonElement metaElem)
    {
        var dict = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (var p in metaElem.EnumerateObject())
        {
            switch (p.Value.ValueKind)
            {
                case JsonValueKind.String: dict[p.Name] = p.Value.GetString()!; break;
                case JsonValueKind.Number:
                    if (p.Value.TryGetInt64(out var l) && l >= int.MinValue && l <= int.MaxValue)
                        dict[p.Name] = (int)l;
                    else
                        dict[p.Name] = p.Value.GetDouble();
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    dict[p.Name] = p.Value.GetBoolean();
                    break;
                default:
                    // Ignore null or complex types
                    break;
            }
        }
        return dict.Count == 0 ? null : new ImmutableMetadata(dict);
    }

    private static Dictionary<string, object> ExtractObjectVariant(JsonElement obj)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (var p in obj.EnumerateObject())
        {
            switch (p.Value.ValueKind)
            {
                case JsonValueKind.String: result[p.Name] = p.Value.GetString()!; break;
                case JsonValueKind.Number:
                    if (p.Value.TryGetInt64(out var l) && l >= int.MinValue && l <= int.MaxValue)
                        result[p.Name] = (int)l;
                    else
                        result[p.Name] = p.Value.GetDouble();
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    result[p.Name] = p.Value.GetBoolean();
                    break;
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    // Nested complex structures not required by current test data; could be added if needed.
                    result[p.Name] = p.Value.Clone();
                    break;
                case JsonValueKind.Null:
                    result[p.Name] = null!;
                    break;
            }
        }
        return result;
    }

    private enum VariantKind { Boolean, Integer, Double, String, Object }

    private static VariantKind ClassifyVariantValue(JsonElement e) =>
        e.ValueKind switch
        {
            JsonValueKind.True or JsonValueKind.False => VariantKind.Boolean,
            JsonValueKind.String => VariantKind.String,
            JsonValueKind.Object => VariantKind.Object,
            JsonValueKind.Number => e.TryGetInt64(out _) ? VariantKind.Integer : VariantKind.Double,
            _ => throw new JsonException($"Unsupported variant value kind '{e.ValueKind}'")
        };

    // Promote mixed numeric (int + double) to double
    private static VariantKind Promote(VariantKind existing, VariantKind incoming)
    {
        static bool IsNumeric(VariantKind k) => k == VariantKind.Integer || k == VariantKind.Double;

        if (existing == incoming)
            return existing;

        if (IsNumeric(existing) && IsNumeric(incoming))
            return VariantKind.Double;

        throw new JsonException($"Mixed incompatible variant kinds: {existing} and {incoming}");
    }
}
