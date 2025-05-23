using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature;

/// <summary>
/// Interface used to resolve flags of varying types.
/// </summary>
public interface IFeatureClient : IEventBus
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
    /// Gets the <see cref="EvaluationContext"/> of this client
    /// <para>
    /// The evaluation context may be set from multiple threads, when accessing the client evaluation context
    /// it should be accessed once for an operation, and then that reference should be used for all dependent
    /// operations.
    /// </para>
    /// </summary>
    /// <returns><see cref="EvaluationContext"/>of this client</returns>
    EvaluationContext GetContext();

    /// <summary>
    /// Sets the <see cref="EvaluationContext"/> of the client
    /// </summary>
    /// <param name="context">The <see cref="EvaluationContext"/> to set</param>
    void SetContext(EvaluationContext context);

    /// <summary>
    /// Gets client metadata
    /// </summary>
    /// <returns>Client metadata <see cref="ClientMetadata"/></returns>
    ClientMetadata GetMetadata();

    /// <summary>
    /// Returns the current status of the associated provider.
    /// </summary>
    /// <returns><see cref="ProviderStatus"/></returns>
    ProviderStatus ProviderStatus { get; }

    /// <summary>
    /// Resolves a boolean feature flag
    /// </summary>
    /// <param name="flagKey">Feature flag key</param>
    /// <param name="defaultValue">Default value</param>
    /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
    /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>Resolved flag value.</returns>
    Task<bool> GetBooleanValueAsync(string flagKey, bool defaultValue, EvaluationContext? context = null, FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a boolean feature flag
    /// </summary>
    /// <param name="flagKey">Feature flag key</param>
    /// <param name="defaultValue">Default value</param>
    /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
    /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
    Task<FlagEvaluationDetails<bool>> GetBooleanDetailsAsync(string flagKey, bool defaultValue, EvaluationContext? context = null, FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a string feature flag
    /// </summary>
    /// <param name="flagKey">Feature flag key</param>
    /// <param name="defaultValue">Default value</param>
    /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
    /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>Resolved flag value.</returns>
    Task<string> GetStringValueAsync(string flagKey, string defaultValue, EvaluationContext? context = null, FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a string feature flag
    /// </summary>
    /// <param name="flagKey">Feature flag key</param>
    /// <param name="defaultValue">Default value</param>
    /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
    /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
    Task<FlagEvaluationDetails<string>> GetStringDetailsAsync(string flagKey, string defaultValue, EvaluationContext? context = null, FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a integer feature flag
    /// </summary>
    /// <param name="flagKey">Feature flag key</param>
    /// <param name="defaultValue">Default value</param>
    /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
    /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>Resolved flag value.</returns>
    Task<int> GetIntegerValueAsync(string flagKey, int defaultValue, EvaluationContext? context = null, FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a integer feature flag
    /// </summary>
    /// <param name="flagKey">Feature flag key</param>
    /// <param name="defaultValue">Default value</param>
    /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
    /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
    Task<FlagEvaluationDetails<int>> GetIntegerDetailsAsync(string flagKey, int defaultValue, EvaluationContext? context = null, FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a double feature flag
    /// </summary>
    /// <param name="flagKey">Feature flag key</param>
    /// <param name="defaultValue">Default value</param>
    /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
    /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>Resolved flag value.</returns>
    Task<double> GetDoubleValueAsync(string flagKey, double defaultValue, EvaluationContext? context = null, FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a double feature flag
    /// </summary>
    /// <param name="flagKey">Feature flag key</param>
    /// <param name="defaultValue">Default value</param>
    /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
    /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
    Task<FlagEvaluationDetails<double>> GetDoubleDetailsAsync(string flagKey, double defaultValue, EvaluationContext? context = null, FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a structure object feature flag
    /// </summary>
    /// <param name="flagKey">Feature flag key</param>
    /// <param name="defaultValue">Default value</param>
    /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
    /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>Resolved flag value.</returns>
    Task<Value> GetObjectValueAsync(string flagKey, Value defaultValue, EvaluationContext? context = null, FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a structure object feature flag
    /// </summary>
    /// <param name="flagKey">Feature flag key</param>
    /// <param name="defaultValue">Default value</param>
    /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
    /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
    Task<FlagEvaluationDetails<Value>> GetObjectDetailsAsync(string flagKey, Value defaultValue, EvaluationContext? context = null, FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default);
}
