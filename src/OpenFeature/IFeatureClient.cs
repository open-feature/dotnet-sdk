using System.Collections.Generic;
using System.Threading.Tasks;
using OpenFeature.Model;

namespace OpenFeature
{
    /// <summary>
    /// Interface used to resolve flags of varying types.
    /// </summary>
    public interface IFeatureClient
    {
        /// <summary>
        /// Appends hooks to client
        /// <para>
        /// The appending operation will be atomic.
        /// </para>
        /// </summary>
        /// <param name="hooks">A list of Hooks that implement the <see cref="Hook"/> interface</param>
        void AddHooks(IEnumerable<Hook> hooks);

        /// <summary>
        /// Gets client metadata
        /// </summary>
        /// <returns>Client metadata <see cref="ClientMetadata"/></returns>
        ClientMetadata GetMetadata();

        /// <summary>
        /// Resolves a boolean feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        Task<bool> GetBooleanValue(string flagKey, bool defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);

        /// <summary>
        /// Resolves a boolean feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        Task<FlagEvaluationDetails<bool>> GetBooleanDetails(string flagKey, bool defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);

        /// <summary>
        /// Resolves a string feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        Task<string> GetStringValue(string flagKey, string defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);

        /// <summary>
        /// Resolves a string feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        Task<FlagEvaluationDetails<string>> GetStringDetails(string flagKey, string defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);

        /// <summary>
        /// Resolves a integer feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        Task<int> GetIntegerValue(string flagKey, int defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);

        /// <summary>
        /// Resolves a integer feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        Task<FlagEvaluationDetails<int>> GetIntegerDetails(string flagKey, int defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);

        /// <summary>
        /// Resolves a double feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        Task<double> GetDoubleValue(string flagKey, double defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);

        /// <summary>
        /// Resolves a double feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        Task<FlagEvaluationDetails<double>> GetDoubleDetails(string flagKey, double defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);

        /// <summary>
        /// Resolves a structure object feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        Task<Value> GetObjectValue(string flagKey, Value defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);

        /// <summary>
        /// Resolves a structure object feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        Task<FlagEvaluationDetails<Value>> GetObjectDetails(string flagKey, Value defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null);
    }
}
