using System;
using System.Collections.Generic;

namespace OpenFeatureSDK.Model
{
    /// <summary>
    /// A KeyValuePair with a string key and object value that is used to apply user defined properties
    /// to the feature flag evaluation context.
    /// <para>
    /// Application developers and providers should not provide implementations of this interface
    /// and should instead use the implementations provided by the SDK.
    /// </para>
    /// </summary>
    /// <seealso href="https://github.com/open-feature/spec/blob/main/specification/evaluation-context.md">Evaluation context</seealso>
    public interface IEvaluationContext
    {
        /// <summary>
        /// Gets the Value at the specified key
        /// </summary>
        /// <param name="key">The key of the value to be retrieved</param>
        /// <returns>The <see cref="Value"/> associated with the key</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the context does not contain the specified key
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the key is <see langword="null" />
        /// </exception>
        Value GetValue(string key);

        /// <summary>
        /// Bool indicating if the specified key exists in the evaluation context
        /// </summary>
        /// <param name="key">The key of the value to be checked</param>
        /// <returns><see cref="bool" />indicating the presence of the key</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the key is <see langword="null" />
        /// </exception>
        bool ContainsKey(string key);

        /// <summary>
        /// Gets the value associated with the specified key
        /// </summary>
        /// <param name="value">The <see cref="Value"/> or <see langword="null" /> if the key was not present</param>
        /// <param name="key">The key of the value to be retrieved</param>
        /// <returns><see cref="bool" />indicating the presence of the key</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the key is <see langword="null" />
        /// </exception>
        bool TryGetValue(string key, out Value value);

        /// <summary>
        /// Gets all values as a Dictionary
        /// </summary>
        /// <returns>New <see cref="IDictionary{TKey,TValue}"/> representation of this Structure</returns>
        IDictionary<string, Value> AsDictionary();

        /// <summary>
        /// Return a count of all values
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Return an enumerator for all values
        /// </summary>
        /// <returns>An enumerator for all values</returns>
        IEnumerator<KeyValuePair<string, Value>> GetEnumerator();
    }
}
