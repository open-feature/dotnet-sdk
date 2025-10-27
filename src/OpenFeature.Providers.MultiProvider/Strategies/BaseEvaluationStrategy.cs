using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.Strategies.Models;

namespace OpenFeature.Providers.MultiProvider.Strategies;

/// <summary>
/// Provides a base class for implementing evaluation strategies that determine how feature flags are evaluated across multiple feature providers.
/// </summary>
/// <remarks>
/// This abstract class serves as the foundation for creating custom evaluation strategies that can handle feature flag resolution
/// across multiple providers. Implementations define the specific logic for how providers are selected, prioritized, or combined
/// when evaluating feature flags.
/// </remarks>
public abstract class BaseEvaluationStrategy
{
    /// <summary>
    /// Determines whether providers should be evaluated in parallel or sequentially.
    /// </summary>
    public virtual RunMode RunMode => RunMode.Sequential;

    /// <summary>
    /// Determines whether a specific provider should be evaluated.
    /// </summary>
    /// <typeparam name="T">The type of the flag value.</typeparam>
    /// <param name="strategyContext">Context information about the provider and evaluation.</param>
    /// <param name="evaluationContext">The evaluation context for the flag resolution.</param>
    /// <returns>True if the provider should be evaluated, false otherwise.</returns>
    public virtual bool ShouldEvaluateThisProvider<T>(StrategyPerProviderContext<T> strategyContext, EvaluationContext? evaluationContext)
    {
        // Skip providers that are not ready or have fatal errors
        return strategyContext.ProviderStatus is not (ProviderStatus.NotReady or ProviderStatus.Fatal);
    }

    /// <summary>
    /// Determines whether the next provider should be evaluated after the current one.
    /// This method is only called in sequential mode.
    /// </summary>
    /// <typeparam name="T">The type of the flag value.</typeparam>
    /// <param name="strategyContext">Context information about the provider and evaluation.</param>
    /// <param name="evaluationContext">The evaluation context for the flag resolution.</param>
    /// <param name="result">The result from the current provider evaluation.</param>
    /// <returns>True if the next provider should be evaluated, false otherwise.</returns>
    public virtual bool ShouldEvaluateNextProvider<T>(StrategyPerProviderContext<T> strategyContext, EvaluationContext? evaluationContext, ProviderResolutionResult<T> result)
    {
        return true;
    }

    /// <summary>
    /// Determines the final result from all provider evaluation results.
    /// </summary>
    /// <typeparam name="T">The type of the flag value.</typeparam>
    /// <param name="strategyContext">Context information about the evaluation.</param>
    /// <param name="key">The feature flag key to evaluate.</param>
    /// <param name="defaultValue">The default value to return if evaluation fails or the flag is not found.</param>
    /// <param name="evaluationContext">The evaluation context for the flag resolution.</param>
    /// <param name="resolutions">All resolution results from provider evaluations.</param>
    /// <returns>The final evaluation result.</returns>
    public abstract FinalResult<T> DetermineFinalResult<T>(StrategyEvaluationContext<T> strategyContext, string key, T defaultValue, EvaluationContext? evaluationContext, List<ProviderResolutionResult<T>> resolutions);

    /// <summary>
    /// Determines whether a specific provider should receive tracking events.
    /// </summary>
    /// <param name="strategyContext">Context information about the provider.</param>
    /// <param name="evaluationContext">The evaluation context for the tracking event.</param>
    /// <param name="trackingEventName">The name of the tracking event.</param>
    /// <param name="trackingEventDetails">The tracking event details.</param>
    /// <returns>True if the provider should receive tracking events, false otherwise.</returns>
    public virtual bool ShouldTrackWithThisProvider(StrategyPerProviderContext<object> strategyContext, EvaluationContext? evaluationContext, string trackingEventName, TrackingEventDetails? trackingEventDetails)
    {
        // By default, track with providers that are ready
        return strategyContext.ProviderStatus == ProviderStatus.Ready;
    }

    /// <summary>
    /// Checks if a resolution result represents an error.
    /// </summary>
    /// <typeparam name="T">The type of the resolved value.</typeparam>
    /// <param name="resolution">The resolution result to check.</param>
    /// <returns>True if the result represents an error, false otherwise.</returns>
    protected static bool HasError<T>(ProviderResolutionResult<T> resolution)
    {
        return resolution.ThrownError is not null || resolution.ResolutionDetails switch
        {
            { } success => success.ErrorType != ErrorType.None,
            _ => false
        };
    }

    /// <summary>
    /// Collects errors from provider resolution results.
    /// </summary>
    /// <typeparam name="T">The type of the flag value.</typeparam>
    /// <param name="resolutions">The provider resolution results to collect errors from.</param>
    /// <returns>A list of provider errors.</returns>
    protected static List<ProviderError> CollectProviderErrors<T>(List<ProviderResolutionResult<T>> resolutions)
    {
        var errors = new List<ProviderError>();

        foreach (var resolution in resolutions)
        {
            if (resolution.ThrownError is not null)
            {
                errors.Add(new ProviderError(resolution.ProviderName, resolution.ThrownError));
            }
            else if (resolution.ResolutionDetails?.ErrorType != ErrorType.None)
            {
                var errorMessage = resolution.ResolutionDetails?.ErrorMessage ?? "unknown error";
                var error = new Exception(errorMessage); // Adjust based on your ErrorWithCode implementation
                errors.Add(new ProviderError(resolution.ProviderName, error));
            }
        }

        return errors;
    }

    /// <summary>
    /// Checks if a resolution result has a specific error code.
    /// </summary>
    /// <typeparam name="T">The type of the resolved value.</typeparam>
    /// <param name="resolution">The resolution result to check.</param>
    /// <param name="errorType">The error type to check for.</param>
    /// <returns>True if the result has the specified error type, false otherwise.</returns>
    protected static bool HasErrorWithCode<T>(ProviderResolutionResult<T> resolution, ErrorType errorType)
    {
        return resolution.ResolutionDetails switch
        {
            { } success => success.ErrorType == errorType,
            _ => false
        };
    }

    /// <summary>
    /// Converts a resolution result to a final result.
    /// </summary>
    /// <typeparam name="T">The type of the resolved value.</typeparam>
    /// <param name="resolution">The resolution result to convert.</param>
    /// <returns>The converted final result.</returns>
    protected static FinalResult<T> ToFinalResult<T>(ProviderResolutionResult<T> resolution)
    {
        return new FinalResult<T>(resolution.ResolutionDetails, resolution.Provider, resolution.ProviderName, null);
    }
}
