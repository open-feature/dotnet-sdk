using OpenFeature.Model;

namespace OpenFeature.Extensions.MultiProvider;

/// <summary>
/// Provides a base class for implementing evaluation strategies that determine how feature flags are evaluated across multiple feature providers.
/// </summary>
/// <remarks>
/// This abstract class serves as the foundation for creating custom evaluation strategies that can handle feature flag resolution
/// across multiple providers. Implementations define the specific logic for how providers are selected, prioritized, or combined
/// when evaluating feature flags.
/// </remarks>
/// <seealso href="https://openfeature.dev/specification/appendix-a/#multi-provider">Multi Provider specification</seealso>
public abstract class BaseEvaluationStrategy
{
    /// <summary>
    /// Evaluates a feature flag across multiple providers using the strategy's specific logic.
    /// </summary>
    /// <typeparam name="T">The type of the feature flag value to be evaluated.</typeparam>
    /// <param name="providers">A dictionary of feature providers keyed by their identifier.</param>
    /// <param name="key">The feature flag key to evaluate.</param>
    /// <param name="defaultValue">The default value to return if evaluation fails or the flag is not found.</param>
    /// <param name="evaluationContext">Optional context information for the evaluation.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous evaluation operation, containing the resolution details with the evaluated value and metadata.</returns>
    public abstract Task<ResolutionDetails<T>> EvaluateAsync<T>(Dictionary<string, FeatureProvider> providers, string key, T defaultValue, EvaluationContext? evaluationContext = null, CancellationToken cancellationToken = default);
}
