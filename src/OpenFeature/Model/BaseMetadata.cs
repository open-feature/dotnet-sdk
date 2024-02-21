using System.Collections.Generic;
using System.Collections.Immutable;

#nullable enable
namespace OpenFeature.Model;

/// <summary>
/// Represents the base class for metadata objects.
/// </summary>
public abstract class BaseMetadata
{
    private readonly ImmutableDictionary<string, object> _metadata;

    internal BaseMetadata(Dictionary<string, object> metadata)
    {
        this._metadata = metadata.ToImmutableDictionary();
    }

    /// <summary>
    /// Gets the boolean value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The boolean value associated with the key, or null if the key is not found.</returns>
    public virtual bool? GetBool(string key)
    {
        return this.GetValue<bool>(key);
    }

    /// <summary>
    /// Gets the integer value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The integer value associated with the key, or null if the key is not found.</returns>
    public virtual int? GetInt(string key)
    {
        return this.GetValue<int>(key);
    }

    /// <summary>
    /// Gets the double value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The double value associated with the key, or null if the key is not found.</returns>
    public virtual double? GetDouble(string key)
    {
        return this.GetValue<double>(key);
    }

    /// <summary>
    /// Gets the string value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The string value associated with the key, or null if the key is not found.</returns>
    public virtual string? GetString(string key)
    {
        var hasValue = this._metadata.TryGetValue(key, out var value);
        if (!hasValue)
        {
            return null;
        }

        return value as string ?? null;
    }

    private T? GetValue<T>(string key) where T : struct
    {
        var hasValue = this._metadata.TryGetValue(key, out var value);
        if (!hasValue)
        {
            return null;
        }

        return value is T tValue ? tValue : null;
    }
}
