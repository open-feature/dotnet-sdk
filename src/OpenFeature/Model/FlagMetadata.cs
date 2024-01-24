using System;
using System.Collections.Immutable;

#nullable enable
namespace OpenFeature.Model
{
    public sealed class FlagMetadata
    {
        private readonly ImmutableDictionary<string, object> _metadata;

        public FlagMetadata(ImmutableDictionary<string, object> metadata)
        {
            this._metadata = metadata;
        }

        public T? GetValue<T>(string key) where T : struct
        {
            var hasValue = this._metadata.TryGetValue(key, out var value);
            if (!hasValue)
            {
                return null;
            }

            return value is T tValue ? tValue : throw new InvalidCastException($"Cannot cast {value?.GetType().ToString() ?? "Nullable"} to {typeof(T)}");
        }
    }
}
