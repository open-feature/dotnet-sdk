using System;
using System.Collections.Generic;
using System.Collections.Immutable;

#nullable enable
namespace OpenFeature.Model;

public abstract class BaseMetadata
{
    private readonly ImmutableDictionary<string, object> _metadata;

    internal BaseMetadata(Dictionary<string, object> metadata)
    {
        this._metadata = metadata.ToImmutableDictionary();
    }

    internal BaseMetadata() : this([])
    {
    }

    public virtual bool? GetBool(string key)
    {
        return this.GetValue<bool>(key);
    }

    public virtual int? GetInt(string key)
    {
        return this.GetValue<int>(key);
    }

    public virtual double? GetDouble(string key)
    {
        return this.GetValue<double>(key);
    }

    public virtual string? GetString(string key)
    {
        var hasValue = this._metadata.TryGetValue(key, out var value);
        if (!hasValue)
        {
            return null;
        }

        return value as string ?? throw new InvalidCastException($"Cannot cast {value?.GetType()} to {typeof(string)}");
    }

    private T? GetValue<T>(string key) where T : struct
    {
        var hasValue = this._metadata.TryGetValue(key, out var value);
        if (!hasValue)
        {
            return null;
        }

        return value is T tValue ? tValue : throw new InvalidCastException($"Cannot cast {value?.GetType()} to {typeof(T)}");
    }
}
