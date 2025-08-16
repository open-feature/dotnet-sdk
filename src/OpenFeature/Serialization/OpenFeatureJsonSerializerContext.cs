using System.Collections.Immutable;
using System.Text.Json.Serialization;
using OpenFeature.Model;

namespace OpenFeature.Serialization;

/// <summary>
/// JSON serializer context for AOT compilation support.
/// This ensures that all necessary types are pre-compiled for JSON serialization
/// when using NativeAOT.
/// </summary>
[JsonSerializable(typeof(Value))]
[JsonSerializable(typeof(Structure))]
[JsonSerializable(typeof(EvaluationContext))]
[JsonSerializable(typeof(Dictionary<string, Value>))]
[JsonSerializable(typeof(ImmutableDictionary<string, Value>))]
[JsonSerializable(typeof(List<Value>))]
[JsonSerializable(typeof(ImmutableList<Value>))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(DateTime))]
[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class OpenFeatureJsonSerializerContext : JsonSerializerContext;
