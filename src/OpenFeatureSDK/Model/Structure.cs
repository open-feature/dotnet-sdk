using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace OpenFeatureSDK.Model
{
    /// <summary>
    /// Structure represents a map of Values
    /// </summary>
    public sealed class Structure : IEnumerable<KeyValuePair<string, Value>>, IStructure
    {
        private readonly ImmutableDictionary<string, Value> _attributes;

        /// <summary>
        /// Internal constructor for use by the builder.
        /// </summary>
        internal Structure(ImmutableDictionary<string, Value> attributes)
        {
            this._attributes = attributes;
        }

        /// <summary>
        /// Private constructor for creating an empty <see cref="Structure"/>.
        /// </summary>
        private Structure()
        {
            this._attributes = ImmutableDictionary<string, Value>.Empty; ;
        }

        /// <summary>
        /// An empty structure.
        /// </summary>
        public static readonly IStructure Empty = new Structure();

        /// <summary>
        /// Creates a new structure with the supplied attributes
        /// </summary>
        /// <param name="attributes"></param>
        public Structure(IDictionary<string, Value> attributes)
        {
            this._attributes = ImmutableDictionary.CreateRange(attributes);
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
        }

        /// <summary>
        /// Return a list containing all the keys in this structure
        /// </summary>
        public IImmutableList<string> Keys => this._attributes.Keys.ToImmutableList();

        /// <summary>
        /// Return an enumerable containing all the values in this structure
        /// </summary>
        public IImmutableList<Value> Values => this._attributes.Values.ToImmutableList();

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

        /// <summary>
        /// Get a builder which can build a <see cref="Structure"/>.
        /// </summary>
        /// <returns>The builder</returns>
        public static IStructureBuilder Builder()
        {
            return new StructureBuilder();
        }

        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
