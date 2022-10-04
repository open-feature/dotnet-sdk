using System;
using System.Collections.Immutable;

namespace OpenFeatureSDK.Model
{
    public sealed class EvaluationContextBuilder
    {
        private readonly StructureBuilder _attributes = new StructureBuilder();

        public EvaluationContextBuilder Set(string key, Value value)
        {
            this._attributes.Set(key, value);
            return this;
        }

        public EvaluationContextBuilder Set(string key, string value)
        {
            this._attributes.Set(key, value);
            return this;
        }

        public EvaluationContextBuilder Set(string key, int value)
        {
            this._attributes.Set(key, value);
            return this;
        }

        public EvaluationContextBuilder Set(string key, double value)
        {
            this._attributes.Set(key, value);
            return this;
        }

        public EvaluationContextBuilder Set(string key, long value)
        {
            this._attributes.Set(key, value);
            return this;
        }

        public EvaluationContextBuilder Set(string key, bool value)
        {
            this._attributes.Set(key, value);
            return this;
        }

        public EvaluationContextBuilder Set(string key, Structure value)
        {
            this._attributes.Set(key, value);
            return this;
        }

        public EvaluationContextBuilder Set(string key, DateTime time)
        {
            this._attributes.Set(key, time);
            return this;
        }

        public EvaluationContextBuilder Set(string key, IImmutableList<Value> value)
        {
            this._attributes.Set(key, value);
            return this;
        }

        public EvaluationContextBuilder Remove(string key)
        {
            this._attributes.Remove(key);
            return this;
        }

        public EvaluationContextBuilder Merge(EvaluationContext context)
        {
            foreach (var kvp in context)
            {
                this.Set(kvp.Key, kvp.Value);
            }

            return this;
        }

        public EvaluationContext Build()
        {
            return new EvaluationContext(this._attributes.Build());
        }
    }
}
