using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OpenFeature.SDK.Model
{
    /// <summary>
    /// Structure represents a map of Values
    /// </summary>
    public class Structure : IEnumerable<KeyValuePair<string, Value>>
    {
        private readonly Dictionary<string, Value> _attributes;

        /// <summary>
        /// Creates a new structure with an empty set of attributes
        /// </summary>
        public Structure()
        {
            this._attributes = new Dictionary<string, Value>();
        }

        /// <summary>
        /// Creates a new structure with the supplied attributes
        /// </summary>
        /// <param name="attributes"></param>
        public Structure(IDictionary<string, Value> attributes)
        {
            this._attributes = new Dictionary<string, Value>(attributes);
        }

        /// <summary>
        /// Gets the Value at the specified key
        /// </summary>
        /// <param name="key">The key of the value to be retrieved</param>
        /// <returns><see cref="Value"/></returns>
        public Value GetValue(string key) => this._attributes[key];

        /// <summary>
        /// Bool indicating if the specified key exists in the structure
        /// </summary>
        /// <param name="key">The key of the value to be retrieved</param>
        /// <returns><see cref="bool"/>indicating the presence of the key.</returns>
        public bool ContainsKey(string key) => this._attributes.ContainsKey(key);

        /// <summary>
        /// Removes the Value at the specified key
        /// </summary>
        /// <param name="key">The key of the value to be retrieved</param>
        /// <returns><see cref="bool"/> indicating the presence of the key.</returns>
        public bool Remove(string key) => this._attributes.Remove(key);

        /// <summary>
        /// Gets the value associated with the specified key by mutating the supplied value.
        /// </summary>
        /// <param name="key">The key of the value to be retrieved</param>
        /// <param name="value">value to be mutated</param>
        /// <returns><see cref="bool"/> indicating the presence of the key.</returns>
        public bool TryGetValue(string key, out Value value) => this._attributes.TryGetValue(key, out value);

        /// <summary>
        /// Gets all values as a Dictionary
        /// </summary>
        /// <returns>New <see cref="IDictionary"/> representation of this Structure</returns>
        public IDictionary<string, Value> AsDictionary()
        {
            return new Dictionary<string, Value>(this._attributes);
        }

        /// <summary>
        /// Return the value at the supplied index
        /// </summary>
        /// <param name="key">The key of the value to be retrieved</param>
        public Value this[string key]
        {
            get => this._attributes[key];
            set => this._attributes[key] = value;
        }

        /// <summary>
        /// Return a collection containing all the keys in this structure
        /// </summary>
        public ICollection<string> Keys => this._attributes.Keys;

        /// <summary>
        /// Return a collection containing all the values in this structure
        /// </summary>
        public ICollection<Value> Values => this._attributes.Values;

        /// <summary>
        /// Add a new bool Value to the structure
        /// </summary>
        /// <param name="key">The key of the value to be retrieved</param>
        /// <param name="value">The value to be added</param>
        /// <returns>This <see cref="Structure"/></returns>
        public Structure Add(string key, bool value)
        {
            this._attributes.Add(key, new Value(value));
            return this;
        }

        /// <summary>
        /// Add a new string Value to the structure
        /// </summary>
        /// <param name="key">The key of the value to be retrieved</param>
        /// <param name="value">The value to be added</param>
        /// <returns>This <see cref="Structure"/></returns>
        public Structure Add(string key, string value)
        {
            this._attributes.Add(key, new Value(value));
            return this;
        }

        /// <summary>
        /// Add a new int Value to the structure
        /// </summary>
        /// <param name="key">The key of the value to be retrieved</param>
        /// <param name="value">The value to be added</param>
        /// <returns>This <see cref="Structure"/></returns>
        public Structure Add(string key, int value)
        {
            this._attributes.Add(key, new Value(value));
            return this;
        }

        /// <summary>
        /// Add a new double Value to the structure
        /// </summary>
        /// <param name="key">The key of the value to be retrieved</param>
        /// <param name="value">The value to be added</param>
        /// <returns>This <see cref="Structure"/></returns>
        public Structure Add(string key, double value)
        {
            this._attributes.Add(key, new Value(value));
            return this;
        }

        /// <summary>
        /// Add a new DateTime Value to the structure
        /// </summary>
        /// <param name="key">The key of the value to be retrieved</param>
        /// <param name="value">The value to be added</param>
        /// <returns>This <see cref="Structure"/></returns>
        public Structure Add(string key, DateTime value)
        {
            this._attributes.Add(key, new Value(value));
            return this;
        }

        /// <summary>
        /// Add a new Structure Value to the structure
        /// </summary>
        /// <param name="key">The key of the value to be retrieved</param>
        /// <param name="value">The value to be added</param>
        /// <returns>This <see cref="Structure"/></returns>
        public Structure Add(string key, Structure value)
        {
            this._attributes.Add(key, new Value(value));
            return this;
        }

        /// <summary>
        /// Add a new List Value to the structure
        /// </summary>
        /// <param name="key">The key of the value to be retrieved</param>
        /// <param name="value">The value to be added</param>
        /// <returns>This <see cref="Structure"/></returns>
        public Structure Add(string key, IList<Value> value)
        {
            this._attributes.Add(key, new Value(value));
            return this;
        }

        /// <summary>
        /// Add a new Value to the structure
        /// </summary>
        /// <param name="key">The key of the value to be retrieved</param>
        /// <param name="value">The value to be added</param>
        /// <returns>This <see cref="Structure"/></returns>
        public Structure Add(string key, Value value)
        {
            this._attributes.Add(key, new Value(value));
            return this;
        }

        /// <summary>
        /// Return a count of all values
        /// </summary>
        public int Count => this._attributes.Count;

        /// <summary>
        /// Return an enumerator for all values
        /// </summary>
        /// <returns><see cref="IEnumerator"/></returns>
        public IEnumerator<KeyValuePair<string, Value>> GetEnumerator()
        {
            return this._attributes.GetEnumerator();
        }

        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
