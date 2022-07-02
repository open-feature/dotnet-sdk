using System.Threading.Tasks;
using OpenFeature.Model;

namespace OpenFeature
{
    /// <summary>
    /// The provider interface describes the abstraction layer for a feature flag provider.
    /// A provider acts as the translates layer between the generic feature flag structure to a target feature flag system.
    ///
    /// More information about the provider interface can be found here:
    /// https://github.com/open-feature/spec/blob/main/specification/providers.md
    /// </summary>
    public interface IFeatureProvider
    {
        Metadata GetMetadata();
        Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);
        Task<ResolutionDetails<string>> ResolveStringValue(string flagKey, string defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);
        Task<ResolutionDetails<int>> ResolveNumberValue(string flagKey, int defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);
        Task<ResolutionDetails<T>> ResolveStructureValue<T>(string flagKey, T defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);
    }
}
