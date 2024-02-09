using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using OpenFeature.Constant;
using OpenFeature.Error;
using OpenFeature.Model;

#nullable enable
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
        public async ValueTask UpdateFlags(IDictionary<string, Flag> flags)
        {
            if (flags is null)
                throw new ArgumentNullException(nameof(flags));
            this._flags = new Dictionary<string, Flag>(flags); // shallow copy
            var @event = new ProviderEventPayload
            {
                Type = ProviderEventTypes.ProviderConfigurationChanged,
                ProviderName = _metadata.Name,
                FlagsChanged = flags.Keys.ToList(), // emit all
                Message = "flags changed",
            };
            await this.EventChannel.Writer.WriteAsync(@event).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<bool>> ResolveBooleanValue(
            string flagKey,
            bool defaultValue,
            EvaluationContext? context = null)
        {
            return Task.FromResult(Resolve(flagKey, defaultValue, context));
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<string>> ResolveStringValue(
            string flagKey,
            string defaultValue,
            EvaluationContext? context = null)
        {
            return Task.FromResult(Resolve(flagKey, defaultValue, context));
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<int>> ResolveIntegerValue(
            string flagKey,
            int defaultValue,
            EvaluationContext? context = null)
        {
            return Task.FromResult(Resolve(flagKey, defaultValue, context));
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<double>> ResolveDoubleValue(
            string flagKey,
            double defaultValue,
            EvaluationContext? context = null)
        {
            return Task.FromResult(Resolve(flagKey, defaultValue, context));
        }

        /// <inheritdoc/>
        public override Task<ResolutionDetails<Value>> ResolveStructureValue(
            string flagKey,
            Value defaultValue,
            EvaluationContext? context = null)
        {
            return Task.FromResult(Resolve(flagKey, defaultValue, context));
        }

        private ResolutionDetails<T> Resolve<T>(string flagKey, T defaultValue, EvaluationContext? context)
        {
            if (!this._flags.TryGetValue(flagKey, out var flag))
            {
                throw new FlagNotFoundException($"flag {flag} not found");
            }
            else
            {
                if (typeof(Flag<T>).Equals(flag.GetType()))
                {
                    return ((Flag<T>)flag).Evaluate(flagKey, defaultValue, context);
                }
                else
                {
                    throw new TypeMismatchException($"flag {flag} not found");
                }
            }
        }
    }
}
