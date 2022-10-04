using System;
using System.Collections.Generic;

namespace OpenFeatureSDK.Model
{
    /// <summary>
    /// A KeyValuePair with a string key and object value that is used to apply user defined properties
    /// to the feature flag evaluation context.
    /// </summary>
    /// <seealso href="https://github.com/open-feature/spec/blob/main/specification/evaluation-context.md">Evaluation context</seealso>
    public sealed class EvaluationContext
    {
        private readonly Structure _structure;

        internal EvaluationContext(Structure content)
        {
            this._structure = content;
        }

        private EvaluationContext()
        {
            this._structure = Structure.Empty;
        }

        private static EvaluationContext _empty = new EvaluationContext();

        public static EvaluationContext Empty => _empty;

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
        /// <param name="value">The <see cref="Value"/> or <see langword="null" /> if the key was not present</param>
        /// <param name="key">The key of the value to be retrieved</param>
        /// <returns><see cref="bool" />indicating the presence of the key</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the key is <see langword="null" />
        /// </exception>
        public bool TryGetValue(string key, out Value value) => this._structure.TryGetValue(key, out value);

        /// <summary>
        /// Gets all values as a Dictionary
        /// </summary>
        /// <returns>New <see cref="IDictionary{TKey,TValue}"/> representation of this Structure</returns>
        public IDictionary<string, Value> AsDictionary()
        {
            return new Dictionary<string, Value>(this._structure.AsDictionary());
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
    }
}
