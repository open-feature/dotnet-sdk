using System.Threading;
using System.Threading.Tasks;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature
{
    internal sealed class NoOpFeatureProvider : FeatureProvider
    {
        private readonly Metadata _metadata = new Metadata(NoOpProvider.NoOpProviderName);

        public override Metadata GetMetadata()
        {
            return this._metadata;
        }

        public override Task<ResolutionDetails<bool>> ResolveBooleanValueAsync(string flagKey, bool defaultValue, EvaluationContext context = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(NoOpResponse(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<string>> ResolveStringValueAsync(string flagKey, string defaultValue, EvaluationContext context = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(NoOpResponse(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<int>> ResolveIntegerValueAsync(string flagKey, int defaultValue, EvaluationContext context = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(NoOpResponse(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<double>> ResolveDoubleValueAsync(string flagKey, double defaultValue, EvaluationContext context = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(NoOpResponse(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<Value>> ResolveStructureValueAsync(string flagKey, Value defaultValue, EvaluationContext context = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(NoOpResponse(flagKey, defaultValue));
        }

        private static ResolutionDetails<T> NoOpResponse<T>(string flagKey, T defaultValue)
        {
            return new ResolutionDetails<T>(
                flagKey,
                defaultValue,
                reason: NoOpProvider.ReasonNoOp,
                variant: NoOpProvider.Variant
            );
        }
    }
}
