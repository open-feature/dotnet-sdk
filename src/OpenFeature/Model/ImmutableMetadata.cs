using System.Collections.Generic;
using System.Collections.Immutable;

namespace OpenFeature.Model;

/// <summary>
/// Represents immutable metadata associated with feature flags and events.
/// </summary>
/// <seealso href="https://github.com/open-feature/spec/blob/v0.7.0/specification/types.md#flag-metadata"/>
/// <seealso href="https://github.com/open-feature/spec/blob/v0.7.0/specification/types.md#event-metadata"/>
public sealed class ImmutableMetadata
{
    private readonly ImmutableDictionary<string, object> _metadata;

    /// <summary>
    /// Constructor for the <see cref="ImmutableMetadata"/> class.
    /// </summary>
    public ImmutableMetadata()
    {
        this._metadata = ImmutableDictionary<string, object>.Empty;
    }

    /// <summary>
    /// Constructor for the <see cref="ImmutableMetadata"/> class.
    /// </summary>
    /// <param name="metadata">The dictionary containing the metadata.</param>
    public ImmutableMetadata(Dictionary<string, object> metadata)
    {
        this._metadata = metadata.ToImmutableDictionary();
    }

    /// <summary>
    /// Gets the boolean value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The boolean value associated with the key, or null if the key is not found.</returns>
    public bool? GetBool(string key)
    {
        return this.GetValue<bool>(key);
    }

    /// <summary>
    /// Gets the integer value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The integer value associated with the key, or null if the key is not found.</returns>
    public int? GetInt(string key)
    {
        return this.GetValue<int>(key);
    }

    /// <summary>
    /// Gets the double value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The double value associated with the key, or null if the key is not found.</returns>
    public double? GetDouble(string key)
    {
        return this.GetValue<double>(key);
    }

    /// <summary>
    /// Gets the string value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The string value associated with the key, or null if the key is not found.</returns>
    public string? GetString(string key)
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

    internal int Count => this._metadata.Count;
}
