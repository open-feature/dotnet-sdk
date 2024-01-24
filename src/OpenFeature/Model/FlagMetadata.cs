using System;
using System.Collections.Generic;
using System.Collections.Immutable;

#nullable enable
namespace OpenFeature.Model;

public sealed class FlagMetadata
{
    private readonly Dictionary<string, object> _metadata;

    public FlagMetadata(Dictionary<string, object> metadata)
    {
        this._metadata = metadata;
    }

    public FlagMetadata() : this([])
    {
    }

    public string? GetString(string key)
    {
        var hasValue = this._metadata.TryGetValue(key, out var value);
        if (!hasValue)
        {
            return null;
        }

        return value is string tValue ? tValue : throw new InvalidCastException($"Cannot cast {value?.GetType().ToString() ?? "Nullable"} to {typeof(string)}");
    }

    public bool? GetBool(string key)
    {
        return this.GetValue<bool>(key);
    }

    public int? GetInt(string key)
    {
        return this.GetValue<int>(key);
    }

    public double? GetDouble(string key)
    {
        return this.GetValue<int>(key);
    }

    public decimal? GetDecimal(string key)
    {
        return this.GetValue<decimal>(key);
    }

    public T? GetValue<T>(string key) where T : struct
    {
        var hasValue = this._metadata.TryGetValue(key, out var value);
        if (!hasValue)
        {
            return null;
        }

        return value is T tValue ? tValue : throw new InvalidCastException($"Cannot cast {value?.GetType().ToString() ?? "Nullable"} to {typeof(T)}");
    }
}
