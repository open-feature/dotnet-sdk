using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenFeature.Constant;
using OpenFeature.Error;
using OpenFeature.Model;

namespace OpenFeature.Providers.Memory
{
    /// <summary>
    /// The in memory provider.
    /// Useful for testing and demonstration purposes.
    /// </summary>
    /// <seealso href="https://openfeature.dev/specification/appendix-a#in-memory-provider">In Memory Provider specification</seealso>
    public class InMemoryProvider : FeatureProvider
    {

        private readonly Metadata _metadata = new Metadata("InMemory");

        private Dictionary<string, Flag> _flags;

        /// <inheritdoc/>
        public override Metadata GetMetadata()
        {
            return this._metadata;
        }

        /// <summary>
        /// Construct a new InMemoryProvider.
        /// </summary>
        /// <param name="flags">dictionary of Flags</param>
        public InMemoryProvider(IDictionary<string, Flag>? flags = null)
        {
            if (flags == null)
            {
                this._flags = new Dictionary<string, Flag>();
            }
            else
            {
                this._flags = new Dictionary<string, Flag>(flags); // shallow copy
            }
        }

        /// <summary>
        /// Updating provider flags configuration, replacing all flags.
        /// </summary>
        /// <param name="flags">the flags to use instead of the previous flags.</param>
        public async Task UpdateFlags(IDictionary<string, Flag>? flags = null)
        {
            var changed = this._flags.Keys.ToList();
            if (flags == null)
            {
                this._flags = new Dictionary<string, Flag>();
            }
            else
            {
                this._flags = new Dictionary<string, Flag>(flags); // shallow copy
            }
            changed.AddRange(this._flags.Keys.ToList());
            var @event = new ProviderEventPayload
            {
                Type = ProviderEventTypes.ProviderConfigurationChanged,
                ProviderName = this._metadata.Name,
                FlagsChanged = changed, // emit all
                Message = "flags changed",
            };
            await this.EventChannel.Writer.WriteAsync(@event).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<bool>> ResolveBooleanValueAsync(string flagKey, bool defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(this.Resolve(flagKey, defaultValue, context));
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<string>> ResolveStringValueAsync(string flagKey, string defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(this.Resolve(flagKey, defaultValue, context));
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<int>> ResolveIntegerValueAsync(string flagKey, int defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(this.Resolve(flagKey, defaultValue, context));
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<double>> ResolveDoubleValueAsync(string flagKey, double defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(this.Resolve(flagKey, defaultValue, context));
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<Value>> ResolveStructureValueAsync(string flagKey, Value defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(this.Resolve(flagKey, defaultValue, context));
        }

        private ResolutionDetails<T> Resolve<T>(string flagKey, T defaultValue, EvaluationContext? context)
        {
            if (!this._flags.TryGetValue(flagKey, out var flag))
            {
                throw new FlagNotFoundException($"flag {flagKey} not found");
            }

            // This check returns False if a floating point flag is evaluated as an integer flag, and vice-versa.
            // In a production provider, such behavior is probably not desirable; consider supporting conversion.
            if (flag is Flag<T> value)
            {
                return value.Evaluate(flagKey, defaultValue, context);
            }

            throw new TypeMismatchException($"flag {flagKey} is not of type ${typeof(T)}");
        }
    }
}
