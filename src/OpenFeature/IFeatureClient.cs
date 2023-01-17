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
        /// Enumerates the global hooks.
        /// <para>
        /// The items enumerated will reflect the registered hooks
        /// at the start of enumeration. Hooks added during enumeration
        /// will not be included.
        /// </para>
        /// </summary>
        /// <returns>Enumeration of <see cref="Hook"/></returns>
        IEnumerable<Hook> GetHooks();

        /// <summary>
        /// Gets the EvaluationContext of this client<see cref="EvaluationContext"/>
        /// <para>
        /// The evaluation context may be set from multiple threads, when accessing the client evaluation context
        /// it should be accessed once for an operation, and then that reference should be used for all dependent
        /// operations.
        /// </para>
        /// </summary>
        /// <returns><see cref="EvaluationContext"/>of this client</returns>
        EvaluationContext GetContext();

        /// <summary>
        /// Sets the EvaluationContext of the client<see cref="EvaluationContext"/>
        /// </summary>
        /// <param name="context">The <see cref="EvaluationContext"/> to set</param>
        void SetContext(EvaluationContext context);

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
        /// <returns>Resolved flag value.</returns>
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
        /// <returns>Resolved flag value.</returns>
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
        /// <returns>Resolved flag value.</returns>
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
        /// <returns>Resolved flag value.</returns>
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
        /// <returns>Resolved flag value.</returns>
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
