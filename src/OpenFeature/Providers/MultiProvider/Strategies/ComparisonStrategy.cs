using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.Strategies.Models;

namespace OpenFeature.Providers.MultiProvider.Strategies;

/// <summary>
/// Evaluate all providers in parallel and compare the results.
/// If the values agree, return the value.
/// If the values disagree, return the value from the configured "fallback provider" and execute the "onMismatch"
/// callback if defined.
/// </summary>
public sealed class ComparisonStrategy : BaseEvaluationStrategy
{
    private readonly FeatureProvider? _fallbackProvider;
    private readonly Action<IDictionary<string, object>>? _onMismatch;

    /// <inheritdoc/>
    public override RunMode RunMode => RunMode.Parallel;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComparisonStrategy"/> class.
    /// </summary>
    /// <param name="fallbackProvider">The provider to use as fallback when values don't match.</param>
    /// <param name="onMismatch">Optional callback that is called when providers return different values.</param>
    public ComparisonStrategy(FeatureProvider? fallbackProvider = null, Action<IDictionary<string, object>>? onMismatch = null)
    {
        this._fallbackProvider = fallbackProvider;
        this._onMismatch = onMismatch;
    }

    /// <inheritdoc/>
    public override FinalResult<T> DetermineFinalResult<T>(StrategyEvaluationContext<T> strategyContext, string key, T defaultValue, EvaluationContext? evaluationContext, List<ProviderResolutionResult<T>> resolutions)
    {
        var successfulResolutions = resolutions.Where(r => !HasError(r)).ToList();

        if (successfulResolutions.Count == 0)
        {
            var errorDetails = new ResolutionDetails<T>(key, defaultValue, ErrorType.ProviderNotReady, Reason.Error, errorMessage: "No providers available or all providers failed");
            var errors = resolutions.Select(r => new ProviderError(r.ProviderName, new InvalidOperationException($"Provider {r.ProviderName} failed"))).ToList();
            return new FinalResult<T>(errorDetails, null!, MultiProviderConstants.ProviderName, errors);
        }

        var firstResult = successfulResolutions.First();

        // Check if all successful results agree on the value
        var allAgree = successfulResolutions.All(r => EqualityComparer<T>.Default.Equals(r.ResolutionDetails.Value, firstResult.ResolutionDetails.Value));

        if (allAgree)
        {
            return ToFinalResult(firstResult);
        }

        ProviderResolutionResult<T>? fallbackResolution = null;

        // Find fallback provider if specified
        if (this._fallbackProvider != null)
        {
            fallbackResolution = successfulResolutions.FirstOrDefault(r => ReferenceEquals(r.Provider, this._fallbackProvider));
        }

        // Values don't agree, trigger mismatch callback if provided
        if (this._onMismatch != null)
        {
            // Create a dictionary with provider names and their values for the callback
            var mismatchDetails = successfulResolutions.ToDictionary(
                r => r.ProviderName,
                r => (object)r.ResolutionDetails.Value!
            );
            this._onMismatch(mismatchDetails);
        }

        // Return fallback provider result if available
        return fallbackResolution != null
            ? ToFinalResult(fallbackResolution)
            :
            // Default to first provider's result
            ToFinalResult(firstResult);
    }
}
