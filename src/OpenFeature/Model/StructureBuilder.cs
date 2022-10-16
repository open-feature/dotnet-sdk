using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace OpenFeature.Model
{
    /// <summary>
    /// A builder which allows the specification of attributes for a <see cref="Structure"/>.
    /// <para>
    /// A <see cref="StructureBuilder"/> object is intended for use by a single thread and should not be used from
    /// multiple threads. Once a <see cref="Structure"/> has been created it is immutable and safe for use from
    /// multiple threads.
    /// </para>
    /// </summary>
    public sealed class StructureBuilder
    {
        private readonly ImmutableDictionary<string, Value>.Builder _attributes =
            ImmutableDictionary.CreateBuilder<string, Value>();

        /// <summary>
        /// Internal to only allow direct creation by <see cref="Structure.Builder()"/>.
        /// </summary>
        internal StructureBuilder() { }

        /// <summary>
        /// Set the key to the given <see cref="Value"/>.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public StructureBuilder Set(string key, Value value)
        {
            // Remove the attribute. Will not throw an exception if not present.
            this._attributes.Remove(key);
            this._attributes.Add(key, value);
            return this;
        }

        /// <summary>
        /// Set the key to the given string.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public StructureBuilder Set(string key, string value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        /// <summary>
        /// Set the key to the given int.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public StructureBuilder Set(string key, int value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        /// <summary>
        /// Set the key to the given double.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public StructureBuilder Set(string key, double value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        /// <summary>
        /// Set the key to the given long.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public StructureBuilder Set(string key, long value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        /// <summary>
        /// Set the key to the given bool.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public StructureBuilder Set(string key, bool value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        /// <summary>
        /// Set the key to the given <see cref="Structure"/>.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public StructureBuilder Set(string key, Structure value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        /// <summary>
        /// Set the key to the given DateTime.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public StructureBuilder Set(string key, DateTime value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        /// <summary>
        /// Set the key to the given list.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public StructureBuilder Set(string key, IList<Value> value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        /// <summary>
        /// Build an immutable <see cref="Structure"/>/
        /// </summary>
        /// <returns>The built <see cref="Structure"/></returns>
        public Structure Build()
        {
            return new Structure(this._attributes.ToImmutable());
        }
    }
}
