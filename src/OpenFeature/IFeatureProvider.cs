using System.Threading.Tasks;
using OpenFeature.SDK.Model;

namespace OpenFeature.SDK
{
    /// <summary>
    /// The provider interface describes the abstraction layer for a feature flag provider.
    /// A provider acts as the translates layer between the generic feature flag structure to a target feature flag system.
    /// </summary>
    /// <seealso href="https://github.com/open-feature/spec/blob/main/specification/providers.md">Provider specification</seealso>
    public interface IFeatureProvider
    {
        /// <summary>
        /// Metadata describing the provider.
        /// </summary>
        /// <returns><see cref="Metadata"/></returns>
        Metadata GetMetadata();

        /// <summary>
        /// Resolves a boolean feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext"/></param>
        /// <param name="config"><see cref="FlagEvaluationOptions"/></param>
        /// <returns><see cref="ResolutionDetails{T}"/></returns>
        Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue,
            EvaluationContext context = null, FlagEvaluationOptions config = null);

        /// <summary>
        /// Resolves a string feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext"/></param>
        /// <param name="config"><see cref="FlagEvaluationOptions"/></param>
        /// <returns><see cref="ResolutionDetails{T}"/></returns>
        Task<ResolutionDetails<string>> ResolveStringValue(string flagKey, string defaultValue,
            EvaluationContext context = null, FlagEvaluationOptions config = null);

        /// <summary>
        /// Resolves a integer feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext"/></param>
        /// <param name="config"><see cref="FlagEvaluationOptions"/></param>
        /// <returns><see cref="ResolutionDetails{T}"/></returns>
        Task<ResolutionDetails<int>> ResolveIntegerValue(string flagKey, int defaultValue,
            EvaluationContext context = null, FlagEvaluationOptions config = null);

        /// <summary>
        /// Resolves a double feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext"/></param>
        /// <param name="config"><see cref="FlagEvaluationOptions"/></param>
        /// <returns><see cref="ResolutionDetails{T}"/></returns>
        Task<ResolutionDetails<double>> ResolveDoubleValue(string flagKey, double defaultValue,
            EvaluationContext context = null, FlagEvaluationOptions config = null);

        /// <summary>
        /// Resolves a structured feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext"/></param>
        /// <param name="config"><see cref="FlagEvaluationOptions"/></param>
        /// <typeparam name="T">Type of object</typeparam>
        /// <returns><see cref="ResolutionDetails{T}"/></returns>
        Task<ResolutionDetails<T>> ResolveStructureValue<T>(string flagKey, T defaultValue,
            EvaluationContext context = null, FlagEvaluationOptions config = null);
    }
}
