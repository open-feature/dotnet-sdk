using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace OpenFeature.Model
{
    /// <summary>
    /// A KeyValuePair with a string key and object value that is used to apply user defined properties
    /// to the feature flag evaluation context.
    /// </summary>
    /// <seealso href="https://github.com/open-feature/spec/blob/v0.5.2/specification/sections/03-evaluation-context.md">Evaluation context</seealso>
    public sealed class EvaluationContext
    {
        /// <summary>
        /// The index for the "targeting key" property when the EvaluationContext is serialized or expressed as a dictionary.
        /// </summary>
        internal const string TargetingKeyIndex = "targetingKey";


        private readonly Structure _structure;

        /// <summary>
        /// Internal constructor used by the builder.
        /// </summary>
        /// <param name="content"></param>
        internal EvaluationContext(Structure content)
        {
            this._structure = content;
        }


        /// <summary>
        /// Private constructor for making an empty <see cref="EvaluationContext"/>.
        /// </summary>
        private EvaluationContext()
        {
            this._structure = Structure.Empty;
        }

        /// <summary>
        /// An empty evaluation context.
        /// </summary>
        public static EvaluationContext Empty { get; } = new EvaluationContext();

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
        /// Returns the targeting key for the context.
        /// </summary>
        public string? TargetingKey
        {
            get
            {
                this._structure.TryGetValue(TargetingKeyIndex, out Value? targetingKey);
                return targetingKey?.AsString;
            }
        }

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
        public static EvaluationContextBuilder Builder()
        {
            return new EvaluationContextBuilder();
        }
    }
}
