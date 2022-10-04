using System;

namespace OpenFeatureSDK.Model
{
    /// <summary>
    /// A builder which allows the specification of attributes for an <see cref="EvaluationContext"/>.
    /// <para>
    /// A <see cref="EvaluationContextBuilder"/> object is intended for use by a single thread and should not be used
    /// from multiple threads. Once an <see cref="EvaluationContext"/> has been created it is immutable and safe for use
    /// from multiple threads.
    /// </para>
    /// </summary>
    public sealed class EvaluationContextBuilder
    {
        private readonly StructureBuilder _attributes = new StructureBuilder();

        /// <summary>
        /// Set the key to the given <see cref="Value"/>.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public EvaluationContextBuilder Set(string key, Value value)
        {
            this._attributes.Set(key, value);
            return this;
        }

        /// <summary>
        /// Set the key to the given string.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public EvaluationContextBuilder Set(string key, string value)
        {
            this._attributes.Set(key, value);
            return this;
        }

        /// <summary>
        /// Set the key to the given int.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public EvaluationContextBuilder Set(string key, int value)
        {
            this._attributes.Set(key, value);
            return this;
        }

        /// <summary>
        /// Set the key to the given double.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public EvaluationContextBuilder Set(string key, double value)
        {
            this._attributes.Set(key, value);
            return this;
        }

        /// <summary>
        /// Set the key to the given long.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public EvaluationContextBuilder Set(string key, long value)
        {
            this._attributes.Set(key, value);
            return this;
        }

        /// <summary>
        /// Set the key to the given bool.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public EvaluationContextBuilder Set(string key, bool value)
        {
            this._attributes.Set(key, value);
            return this;
        }

        /// <summary>
        /// Set the key to the given <see cref="Structure"/>.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public EvaluationContextBuilder Set(string key, Structure value)
        {
            this._attributes.Set(key, value);
            return this;
        }

        /// <summary>
        /// Set the key to the given DateTime.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public EvaluationContextBuilder Set(string key, DateTime value)
        {
            this._attributes.Set(key, value);
            return this;
        }

        /// <summary>
        /// Remove the given key from the context.
        /// </summary>
        /// <param name="key">The key to remove</param>
        /// <returns>This builder</returns>
        public EvaluationContextBuilder Remove(string key)
        {
            this._attributes.Remove(key);
            return this;
        }

        /// <summary>
        /// Incorporate an existing context into the builder.
        /// <para>
        /// Any existing keys in the builder will be replaced by keys in the context.
        /// </para>
        /// </summary>
        /// <param name="context">The context to add merge</param>
        /// <returns>This builder</returns>
        public EvaluationContextBuilder Merge(EvaluationContext context)
        {
            foreach (var kvp in context)
            {
                this.Set(kvp.Key, kvp.Value);
            }

            return this;
        }

        /// <summary>
        /// Build an immutable <see cref="EvaluationContext"/>.
        /// </summary>
        /// <returns>An immutable <see cref="EvaluationContext"/></returns>
        public EvaluationContext Build()
        {
            return new EvaluationContext(this._attributes.Build());
        }
    }
}
