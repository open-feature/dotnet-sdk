using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenFeature.Model;

/// <summary>
/// A <see cref="JsonConverter{T}"/> for <see cref="Value"/> for Json serialization.
/// This converter is AOT-compatible as it uses manual JSON reading/writing 
/// instead of reflection-based serialization.
/// </summary>
public sealed class ValueJsonConverter : JsonConverter<Value>
{
    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Value value, JsonSerializerOptions options) =>
        WriteJsonValue(value, writer);

    /// <inheritdoc />
    public override Value Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        ReadJsonValue(ref reader);

    private static void WriteJsonValue(Value value, Utf8JsonWriter writer)
    {
        if (value.IsNull)
        {
            writer.WriteNullValue();
            return;
        }

        if (value.IsBoolean)
        {
            writer.WriteBooleanValue(value.AsBoolean.GetValueOrDefault());
            return;
        }

        if (value.IsNumber)
        {
            writer.WriteNumberValue(value.AsDouble!.Value);
            return;
        }

        if (value.IsString)
        {
            writer.WriteStringValue(value.AsString);
            return;
        }

        if (value.IsDateTime)
        {
            writer.WriteStringValue(value.AsDateTime!.Value);
            return;
        }

        if (value.IsList)
        {
            writer.WriteStartArray();

            foreach (var item in value.AsList ?? [])
            {
                WriteJsonValue(item, writer);
            }

            writer.WriteEndArray();
            return;
        }

        if (value.IsStructure)
        {
            writer.WriteStartObject();

            var dic = value.AsStructure?.AsDictionary();
            if (dic is { Count: > 0 })
            {
                foreach (var pair in dic)
                {
                    writer.WritePropertyName(pair.Key);
                    WriteJsonValue(pair.Value, writer);
                }
            }

            writer.WriteEndObject();
        }
    }

    private static Value ReadJsonValue(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return new(true);
            case JsonTokenType.False:
                return new(false);
            case JsonTokenType.Number:
                if (reader.TryGetInt32(out var intVal))
                    return new(intVal);

                return new(reader.GetDouble());
            case JsonTokenType.String:
                if (reader.TryGetDateTime(out var dateTime))
                    return new(dateTime);

                return new(reader.GetString()!);
            case JsonTokenType.StartArray:
                var list = new List<Value>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }
                    list.Add(ReadJsonValue(ref reader));
                }
                return new(list);
            case JsonTokenType.StartObject:
                var objectBuilder = Structure.Builder();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    var name = reader.GetString();
                    Debug.Assert(name is not null);
                    reader.Read();
                    objectBuilder.Set(name!, ReadJsonValue(ref reader));
                }
                return new(objectBuilder.Build());

            default:
                return new();
        }
    }
}

