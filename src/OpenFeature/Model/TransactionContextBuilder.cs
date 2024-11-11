using System;

namespace OpenFeature.Model
{
    /// <summary>
    /// A builder which allows the specification of attributes for an <see cref="TransactionContext"/>.
    /// <para>
    /// A <see cref="TransactionContextBuilder"/> object is intended for use by a single thread and should not be used
    /// from multiple threads. Once an <see cref="TransactionContext"/> has been created it is immutable and safe for use
    /// from multiple threads.
    /// </para>
    /// </summary>
    public sealed class TransactionContextBuilder
    {
        private readonly StructureBuilder _attributes = Structure.Builder();

        /// <summary>
        /// Internal to only allow direct creation by <see cref="TransactionContext.Builder()"/>.
        /// </summary>
        internal TransactionContextBuilder() { }

        /// <summary>
        /// Set the targeting key for the context.
        /// </summary>
        /// <param name="targetingKey">The targeting key</param>
        /// <returns>This builder</returns>
        public TransactionContextBuilder SetTargetingKey(string targetingKey)
        {
            this._attributes.Set(TransactionContext.TargetingKeyIndex, targetingKey);
            return this;
        }

        /// <summary>
        /// Set the key to the given <see cref="Value"/>.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public TransactionContextBuilder Set(string key, Value value)
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
        public TransactionContextBuilder Set(string key, string value)
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
        public TransactionContextBuilder Set(string key, int value)
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
        public TransactionContextBuilder Set(string key, double value)
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
        public TransactionContextBuilder Set(string key, long value)
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
        public TransactionContextBuilder Set(string key, bool value)
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
        public TransactionContextBuilder Set(string key, Structure value)
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
        public TransactionContextBuilder Set(string key, DateTime value)
        {
            this._attributes.Set(key, value);
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
        public TransactionContextBuilder Merge(TransactionContext context)
        {
            foreach (var kvp in context)
            {
                this.Set(kvp.Key, kvp.Value);
            }

            return this;
        }

        /// <summary>
        /// Build an immutable <see cref="TransactionContext"/>.
        /// </summary>
        /// <returns>An immutable <see cref="TransactionContext"/></returns>
        public TransactionContext Build()
        {
            return new TransactionContext(this._attributes.Build());
        }
    }
}
