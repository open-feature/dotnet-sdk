using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OpenFeature.SDK.Model
{
    /// <summary>
    ///
    /// </summary>
    public class Structure : IEnumerable<KeyValuePair<string, Value>>
    {
        private readonly Dictionary<string, Value> _attributes;

        /// <summary>
        ///
        /// </summary>
        public Structure()
        {
            this._attributes = new Dictionary<string, Value>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="attributes"></param>
        public Structure(IDictionary<string, Value> attributes)
        {
            this._attributes = new Dictionary<string, Value>(attributes);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Value GetValue(string key) => this._attributes[key];

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key) => this._attributes.ContainsKey(key);

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key) => this._attributes.Remove(key);

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, out Value value) => this._attributes.TryGetValue(key, out value);

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        public Value this[string key]
        {
            get => this._attributes[key];
            set => this._attributes[key] = value;
        }

        /// <summary>
        ///
        /// </summary>
        public ICollection<string> Keys => this._attributes.Keys;

        /// <summary>
        ///
        /// </summary>
        public ICollection<Value> Values => this._attributes.Values;

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Structure Add(string key, bool value)
        {
            this._attributes.Add(key, new Value(value));
            return this;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Structure Add(string key, string value)
        {
            this._attributes.Add(key, new Value(value));
            return this;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Structure Add(string key, int value)
        {
            this._attributes.Add(key, new Value(value));
            return this;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Structure Add(string key, double value)
        {
            this._attributes.Add(key, new Value(value));
            return this;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Structure Add(string key, DateTime value)
        {
            this._attributes.Add(key, new Value(value));
            return this;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Structure Add(string key, Structure value)
        {
            this._attributes.Add(key, new Value(value));
            return this;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Structure Add(string key, List<Value> value)
        {
            this._attributes.Add(key, new Value(value));
            return this;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Structure Add(string key, Value value)
        {
            this._attributes.Add(key, value);
            return this;
        }

        /// <summary>
        ///
        /// </summary>
        public int Count => this._attributes.Count;

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
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
