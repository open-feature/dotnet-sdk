using System.Collections.Generic;
using System.Collections.Immutable;
using OpenFeature.Model;

namespace OpenFeature;

/// <summary>
/// A key-value collection of strings to objects used for passing data between hook stages.
/// <para>
/// This collection is scoped to a single evaluation for a single hook. Each hook stage for the evaluation
/// will share the same <see cref="HookData"/>.
/// </para>
/// <para>
/// This collection is intended for use only during the execution of individual hook stages, a reference
/// to the collection should not be retained.
/// </para>
/// <para>
/// This collection is not thread-safe.
/// </para>
/// </summary>
/// <seealso href="https://github.com/open-feature/spec/blob/main/specification/sections/04-hooks.md#46-hook-data"/>
public sealed class HookData
{
    private readonly Dictionary<string, object> _data = [];

    /// <summary>
    /// Set the key to the given value.
    /// </summary>
    /// <param name="key">The key for the value</param>
    /// <param name="value">The value to set</param>
    /// <returns>This hook data instance</returns>
    public HookData Set(string key, object value)
    {
        this._data[key] = value;
        return this;
    }

    /// <summary>
    /// Gets the value at the specified key as an object.
    /// <remarks>
    /// For <see cref="Value"/> types use <see cref="Get"/> instead.
    /// </remarks>
    /// </summary>
    /// <param name="key">The key of the value to be retrieved</param>
    /// <returns>The object associated with the key</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the context does not contain the specified key
    /// </exception>
    public object Get(string key)
    {
        return this._data[key];
    }

    /// <summary>
    /// Return a count of all values.
    /// </summary>
    public int Count => this._data.Count;

    /// <summary>
    /// Return an enumerator for all values.
    /// </summary>
    /// <returns>An enumerator for all values</returns>
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        return this._data.GetEnumerator();
    }

    /// <summary>
    /// Return a list containing all the keys in the hook data
    /// </summary>
    public IImmutableList<string> Keys => this._data.Keys.ToImmutableList();

    /// <summary>
    /// Return an enumerable containing all the values of the hook data
    /// </summary>
    public IImmutableList<object> Values => this._data.Values.ToImmutableList();

    /// <summary>
    /// Gets all values as a read only dictionary.
    /// <remarks>
    /// The dictionary references the original values and is not a thread-safe copy.
    /// </remarks>
    /// </summary>
    /// <returns>A <see cref="IDictionary{TKey,TValue}"/> representation of the hook data</returns>
    public IReadOnlyDictionary<string, object> AsDictionary()
    {
        return this._data;
    }

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get or set</param>
    /// <returns>The value associated with the specified key</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when getting a value and the context does not contain the specified key
    /// </exception>
    public object this[string key]
    {
        get => this.Get(key);
        set => this.Set(key, value);
    }
}
