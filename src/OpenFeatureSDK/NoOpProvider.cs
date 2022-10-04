using System.Threading.Tasks;
using OpenFeatureSDK.Constant;
using OpenFeatureSDK.Model;

namespace OpenFeatureSDK
{
    internal class NoOpFeatureProvider : FeatureProvider
    {
        private readonly Metadata _metadata = new Metadata(NoOpProvider.NoOpProviderName);

        public override Metadata GetMetadata()
        {
            return this._metadata;
        }

        public override Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue, IEvaluationContext context = null)
        {
            return Task.FromResult(NoOpResponse(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<string>> ResolveStringValue(string flagKey, string defaultValue, IEvaluationContext context = null)
        {
            return Task.FromResult(NoOpResponse(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<int>> ResolveIntegerValue(string flagKey, int defaultValue, IEvaluationContext context = null)
        {
            return Task.FromResult(NoOpResponse(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<double>> ResolveDoubleValue(string flagKey, double defaultValue, IEvaluationContext context = null)
        {
            return Task.FromResult(NoOpResponse(flagKey, defaultValue));
        }

        public override Task<ResolutionDetails<Value>> ResolveStructureValue(string flagKey, Value defaultValue, IEvaluationContext context = null)
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
