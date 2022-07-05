using System.Threading.Tasks;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature
{
    /// <summary>
    /// The No-op provider is set by default, and is used when no other provider is set.
    /// This simply returns the default value for the given type.
    /// </summary>
    public class NoOpFeatureProvider : IFeatureProvider
    {
        private readonly Metadata _metadata = new Metadata(NoOpProvider.NoOpProviderName);

        public Metadata GetMetadata()
        {
            return _metadata;
        }

        public Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null)
        {
            return Task.FromResult(NoOpResponse(flagKey, defaultValue));
        }

        public Task<ResolutionDetails<string>> ResolveStringValue(string flagKey, string defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null)
        {
            return Task.FromResult(NoOpResponse(flagKey, defaultValue));
        }

        public Task<ResolutionDetails<int>> ResolveNumberValue(string flagKey, int defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null)
        {
            return Task.FromResult(NoOpResponse(flagKey, defaultValue));
        }

        public Task<ResolutionDetails<T>> ResolveStructureValue<T>(string flagKey, T defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null)
        {
            return Task.FromResult(NoOpResponse(flagKey, defaultValue));
        }

        private static ResolutionDetails<T> NoOpResponse<T>(string flagKey, T defaultValue)
        {
            return new ResolutionDetails<T>(
                flagKey,
                defaultValue,
                reason: NoOpProvider.ReasonNoOp
            );
        }
    }
}
