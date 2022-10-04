using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace OpenFeatureSDK.Model
{
    /// <summary>
    /// A builder which allows the specification of attributes for a <see cref="Structure"/>.
    /// <para>
    /// A <see cref="StructureBuilder"/> object is intended for use by a single thread and should not be used from
    /// multiple threads. Once a <see cref="Structure"/> has been created it is immutable and safe for use from
    /// multiple threads.
    /// </para>
    /// </summary>
    public sealed class StructureBuilder : IStructureBuilder
    {
        private readonly ImmutableDictionary<string, Value>.Builder _attributes =
            ImmutableDictionary.CreateBuilder<string, Value>();

        /// <summary>
        /// Internal to only allow direct creation by <see cref="Structure.Builder()"/>.
        /// </summary>
        internal StructureBuilder() { }

        /// <inheritdoc />
        public StructureBuilder Set(string key, Value value)
        {
            // Remove the attribute. Will not throw an exception if not present.
            this._attributes.Remove(key);
            this._attributes.Add(key, value);
            return this;
        }

        /// <inheritdoc />
        public StructureBuilder Set(string key, string value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        /// <inheritdoc />
        public StructureBuilder Set(string key, int value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        /// <inheritdoc />
        public StructureBuilder Set(string key, double value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        /// <inheritdoc />
        public StructureBuilder Set(string key, long value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        /// <inheritdoc />
        public StructureBuilder Set(string key, bool value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        /// <inheritdoc />
        public StructureBuilder Set(string key, IStructure value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        /// <inheritdoc />
        public StructureBuilder Set(string key, DateTime value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        /// <inheritdoc />
        public StructureBuilder Set(string key, IList<Value> value)
        {
            this.Set(key, new Value(value));
            return this;
        }


        /// <inheritdoc />
        public StructureBuilder Remove(string key)
        {
            this._attributes.Remove(key);
            return this;
        }

        /// <inheritdoc />
        public IStructure Build()
        {
            return new Structure(this._attributes.ToImmutable());
        }
    }
}
