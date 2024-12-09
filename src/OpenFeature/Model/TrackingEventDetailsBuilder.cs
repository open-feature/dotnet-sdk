using System;

namespace OpenFeature.Model
{
    /// <summary>
    /// A builder which allows the specification of attributes for an <see cref="TrackingEventDetails"/>.
    /// <para>
    /// A <see cref="TrackingEventDetailsBuilder"/> object is intended for use by a single thread and should not be used
    /// from multiple threads. Once an <see cref="TrackingEventDetails"/> has been created it is immutable and safe for use
    /// from multiple threads.
    /// </para>
    /// </summary>
    public sealed class TrackingEventDetailsBuilder
    {
        private readonly StructureBuilder _attributes = Structure.Builder();
        private double? _value;

        /// <summary>
        /// Internal to only allow direct creation by <see cref="TrackingEventDetails.Builder()"/>.
        /// </summary>
        internal TrackingEventDetailsBuilder() { }

        /// <summary>
        /// Set the predefined value field for the tracking details.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public TrackingEventDetailsBuilder SetValue(double? value)
        {
            this._value = value;
            return this;
        }

        /// <summary>
        /// Set the key to the given <see cref="Value"/>.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        public TrackingEventDetailsBuilder Set(string key, Value value)
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
        public TrackingEventDetailsBuilder Set(string key, string value)
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
        public TrackingEventDetailsBuilder Set(string key, int value)
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
        public TrackingEventDetailsBuilder Set(string key, double value)
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
        public TrackingEventDetailsBuilder Set(string key, long value)
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
        public TrackingEventDetailsBuilder Set(string key, bool value)
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
        public TrackingEventDetailsBuilder Set(string key, Structure value)
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
        public TrackingEventDetailsBuilder Set(string key, DateTime value)
        {
            this._attributes.Set(key, value);
            return this;
        }

        /// <summary>
        /// Incorporate existing tracking details into the builder.
        /// <para>
        /// Any existing keys in the builder will be replaced by keys in the tracking details, including the Value set
        /// through <see cref="SetValue(double?)"/>.
        /// </para>
        /// </summary>
        /// <param name="trackingDetails">The tracking details to add merge</param>
        /// <returns>This builder</returns>
        public TrackingEventDetailsBuilder Merge(TrackingEventDetails trackingDetails)
        {
            this._value = trackingDetails.Value;
            foreach (var kvp in trackingDetails)
            {
                this.Set(kvp.Key, kvp.Value);
            }

            return this;
        }

        /// <summary>
        /// Build an immutable <see cref="TrackingEventDetails"/>.
        /// </summary>
        /// <returns>An immutable <see cref="TrackingEventDetails"/></returns>
        public TrackingEventDetails Build()
        {
            return new TrackingEventDetails(this._attributes.Build(), this._value);
        }
    }
}
