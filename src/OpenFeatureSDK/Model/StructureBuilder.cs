using System;
using System.Collections.Immutable;

namespace OpenFeatureSDK.Model
{
    public sealed class StructureBuilder
    {
        private readonly ImmutableDictionary<string, Value>.Builder _attributes =
            ImmutableDictionary.CreateBuilder<string, Value>();

        public StructureBuilder Set(string key, Value value)
        {
            // Remove the attribute. Will not throw an exception if not present.
            this._attributes.Remove(key);
            this._attributes.Add(key, value);
            return this;
        }

        public StructureBuilder Set(string key, string value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        public StructureBuilder Set(string key, int value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        public StructureBuilder Set(string key, double value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        public StructureBuilder Set(string key, long value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        public StructureBuilder Set(string key, bool value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        public StructureBuilder Set(string key, Structure value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        public StructureBuilder Set(string key, DateTime value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        public StructureBuilder Set(string key, IImmutableList<Value> value)
        {
            this.Set(key, new Value(value));
            return this;
        }

        public StructureBuilder Remove(string key)
        {
            this._attributes.Remove(key);
            return this;
        }


        public Structure Build()
        {
            return new Structure(this._attributes.ToImmutable());
        }
    }
}
