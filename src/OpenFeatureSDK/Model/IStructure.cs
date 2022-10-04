using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace OpenFeatureSDK.Model
{
    /// <summary>
    /// IStructure represents a map of Values
    /// <para>
    /// Application developers and providers should not provide implementations of this interface
    /// and should instead use the implementations provided by the SDK.
    /// </para>
    /// </summary>
    public interface IStructure
    {
        /// <summary>
        /// Gets the Value at the specified key
        /// </summary>
        /// <param name="key">The key of the value to be retrieved</param>
        /// <returns><see cref="Value"/></returns>
        Value GetValue(string key);

        /// <summary>
        /// Bool indicating if the specified key exists in the structure
        /// </summary>
        /// <param name="key">The key of the value to be retrieved</param>
        /// <returns><see cref="bool"/>indicating the presence of the key.</returns>
        bool ContainsKey(string key);

        /// <summary>
        /// Gets the value associated with the specified key by mutating the supplied value.
        /// </summary>
        /// <param name="key">The key of the value to be retrieved</param>
        /// <param name="value">value to be mutated</param>
        /// <returns><see cref="bool"/> indicating the presence of the key.</returns>
        bool TryGetValue(string key, out Value value);

        /// <summary>
        /// Gets all values as a Dictionary
        /// </summary>
        /// <returns>New <see cref="IDictionary"/> representation of this Structure</returns>
        IDictionary<string, Value> AsDictionary();

        /// <summary>
        /// Return the value at the supplied index
        /// </summary>
        /// <param name="key">The key of the value to be retrieved</param>
        Value this[string key] { get; }

        /// <summary>
        /// Return a list containing all the keys in this structure
        /// </summary>
        IImmutableList<string> Keys { get; }

        /// <summary>
        /// Return an enumerable containing all the values in this structure
        /// </summary>
        IImmutableList<Value> Values { get; }

        /// <summary>
        /// Return a count of all values
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Return an enumerator for all values
        /// </summary>
        /// <returns><see cref="IEnumerator"/></returns>
        IEnumerator<KeyValuePair<string, Value>> GetEnumerator();
    }
}
