using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OpenFeature.SDK.Model
{
    /// <summary>
    /// A KeyValuePair with a string key and object value that is used to apply user defined properties
    /// to the feature flag evaluation context.
    /// </summary>
    /// <seealso href="https://github.com/open-feature/spec/blob/main/specification/evaluation-context.md">Evaluation context</seealso>
    public class EvaluationContext : IEnumerable<KeyValuePair<string, object>>
    {
        private readonly Dictionary<string, object> _internalContext = new Dictionary<string, object>();

        /// <summary>
        /// Add a new key value pair to the evaluation context
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <typeparam name="T">Type of value</typeparam>
        public void Add<T>(string key, T value)
        {
            this._internalContext.Add(key, value);
        }

        /// <summary>
        /// Remove an object by key from the evaluation context
        /// </summary>
        /// <param name="key">Key</param>
        /// <exception cref="ArgumentException">Key is null</exception>
        public bool Remove(string key)
        {
            return this._internalContext.Remove(key);
        }

        /// <summary>
        /// Get an object from evaluation context by key
        /// </summary>
        /// <param name="key">Key</param>
        /// <typeparam name="T">Type of object</typeparam>
        /// <returns>Object casted to provided type</returns>
        /// <exception cref="InvalidCastException">A type mismatch occurs</exception>
        public T Get<T>(string key)
        {
            return (T)this._internalContext[key];
        }

        /// <summary>
        /// Get value by key
        ///
        /// Note: this will not case the object to type.
        /// This will need to be done by the caller
        /// </summary>
        /// <param name="key">Key</param>
        public object this[string key]
        {
            get => this._internalContext[key];
            set => this._internalContext[key] = value;
        }

        /// <summary>
        /// Merges provided evaluation context into this one
        ///
        /// Any duplicate keys will be overwritten
        /// </summary>
        /// <param name="other"><see cref="EvaluationContext"/></param>
        public void Merge(EvaluationContext other)
        {
            foreach (var key in other._internalContext.Keys)
            {
                if (this._internalContext.ContainsKey(key))
                {
                    this._internalContext[key] = other._internalContext[key];
                }
                else
                {
                    this._internalContext.Add(key, other._internalContext[key]);
                }
            }
        }

        /// <summary>
        /// Returns the number of items in the evaluation context
        /// </summary>
        public int Count => this._internalContext.Count;

        /// <summary>
        /// Returns an enumerator that iterates through the evaluation context
        /// </summary>
        /// <returns>Enumerator of the Evaluation context</returns>
        [ExcludeFromCodeCoverage]
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this._internalContext.GetEnumerator();
        }

        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
