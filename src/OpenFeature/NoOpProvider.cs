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

        public override Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue, EvaluationContext context = null)
        {
            return Task.FromResult(NoOpResponse(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<string>> ResolveStringValue(string flagKey, string defaultValue, EvaluationContext context = null)
        {
            return Task.FromResult(NoOpResponse(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<int>> ResolveIntegerValue(string flagKey, int defaultValue, EvaluationContext context = null)
        {
            return Task.FromResult(NoOpResponse(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<double>> ResolveDoubleValue(string flagKey, double defaultValue, EvaluationContext context = null)
        {
            return Task.FromResult(NoOpResponse(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<Value>> ResolveStructureValue(string flagKey, Value defaultValue, EvaluationContext context = null)
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
