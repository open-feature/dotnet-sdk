using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenFeatureSDK.Model;

namespace OpenFeatureSDK
{
    /// <summary>
    /// The provider interface describes the abstraction layer for a feature flag provider.
    /// A provider acts as the translates layer between the generic feature flag structure to a target feature flag system.
    /// </summary>
    /// <seealso href="https://github.com/open-feature/spec/blob/main/specification/providers.md">Provider specification</seealso>
    public abstract class FeatureProvider
    {
        /// <summary>
        /// Gets a immutable list of hooks that belong to the provider.
        /// By default return a empty list
        ///
        /// Executed in the order of hooks
        /// before: API, Client, Invocation, Provider
        /// after: Provider, Invocation, Client, API
        /// error (if applicable): Provider, Invocation, Client, API
        /// finally: Provider, Invocation, Client, API
        /// </summary>
        /// <returns></returns>
        public virtual IReadOnlyList<Hook> GetProviderHooks() => Array.Empty<Hook>();

        /// <summary>
        /// Metadata describing the provider.
        /// </summary>
        /// <returns><see cref="Metadata"/></returns>
        public abstract Metadata GetMetadata();

        /// <summary>
        /// Resolves a boolean feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext"/></param>
        /// <returns><see cref="ResolutionDetails{T}"/></returns>
        public abstract Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue,
            EvaluationContext context = null);

        /// <summary>
        /// Resolves a string feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext"/></param>
        /// <returns><see cref="ResolutionDetails{T}"/></returns>
        public abstract Task<ResolutionDetails<string>> ResolveStringValue(string flagKey, string defaultValue,
            EvaluationContext context = null);

        /// <summary>
        /// Resolves a integer feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext"/></param>
        /// <returns><see cref="ResolutionDetails{T}"/></returns>
        public abstract Task<ResolutionDetails<int>> ResolveIntegerValue(string flagKey, int defaultValue,
            EvaluationContext context = null);

        /// <summary>
        /// Resolves a double feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext"/></param>
        /// <returns><see cref="ResolutionDetails{T}"/></returns>
        public abstract Task<ResolutionDetails<double>> ResolveDoubleValue(string flagKey, double defaultValue,
            EvaluationContext context = null);

        /// <summary>
        /// Resolves a structured feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext"/></param>
        /// <returns><see cref="ResolutionDetails{T}"/></returns>
        public abstract Task<ResolutionDetails<Value>> ResolveStructureValue(string flagKey, Value defaultValue,
            EvaluationContext context = null);
    }
}
