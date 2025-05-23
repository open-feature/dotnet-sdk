using System.Collections.Immutable;

namespace OpenFeature.Model;

/// <summary>
/// The `tracking event details` structure defines optional data pertinent to a particular `tracking event`.
/// </summary>
/// <seealso href="https://github.com/open-feature/spec/blob/main/specification/sections/06-tracking.md#62-tracking-event-details"/>
public sealed class TrackingEventDetails
{
    /// <summary>
    ///A predefined value field for the tracking details.
    /// </summary>
    public readonly double? Value;

    private readonly Structure _structure;

    /// <summary>
    /// Internal constructor used by the builder.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="value"></param>
    internal TrackingEventDetails(Structure content, double? value)
    {
        this.Value = value;
        this._structure = content;
    }


    /// <summary>
    /// Private constructor for making an empty <see cref="TrackingEventDetails"/>.
    /// </summary>
    private TrackingEventDetails()
    {
        this._structure = Structure.Empty;
        this.Value = null;
    }

    /// <summary>
    /// Empty tracking event details.
    /// </summary>
    public static TrackingEventDetails Empty { get; } = new();


    /// <summary>
    /// Gets the Value at the specified key
    /// </summary>
    /// <param name="key">The key of the value to be retrieved</param>
    /// <returns>The <see cref="Model.Value"/> associated with the key</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the context does not contain the specified key
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the key is <see langword="null" />
    /// </exception>
    public Value GetValue(string key) => this._structure.GetValue(key);

    /// <summary>
    /// Bool indicating if the specified key exists in the evaluation context
    /// </summary>
    /// <param name="key">The key of the value to be checked</param>
    /// <returns><see cref="bool" />indicating the presence of the key</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the key is <see langword="null" />
    /// </exception>
    public bool ContainsKey(string key) => this._structure.ContainsKey(key);

    /// <summary>
    /// Gets the value associated with the specified key
    /// </summary>
    /// <param name="value">The <see cref="Model.Value"/> or <see langword="null" /> if the key was not present</param>
    /// <param name="key">The key of the value to be retrieved</param>
    /// <returns><see cref="bool" />indicating the presence of the key</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the key is <see langword="null" />
    /// </exception>
    public bool TryGetValue(string key, out Value? value) => this._structure.TryGetValue(key, out value);

    /// <summary>
    /// Gets all values as a Dictionary
    /// </summary>
    /// <returns>New <see cref="IDictionary{TKey,TValue}"/> representation of this Structure</returns>
    public IImmutableDictionary<string, Value> AsDictionary()
    {
        return this._structure.AsDictionary();
    }

    /// <summary>
    /// Return a count of all values
    /// </summary>
    public int Count => this._structure.Count;

    /// <summary>
    /// Return an enumerator for all values
    /// </summary>
    /// <returns>An enumerator for all values</returns>
    public IEnumerator<KeyValuePair<string, Value>> GetEnumerator()
    {
        return this._structure.GetEnumerator();
    }

    /// <summary>
    /// Get a builder which can build an <see cref="EvaluationContext"/>.
    /// </summary>
    /// <returns>The builder</returns>
    public static TrackingEventDetailsBuilder Builder()
    {
        return new TrackingEventDetailsBuilder();
    }
}
