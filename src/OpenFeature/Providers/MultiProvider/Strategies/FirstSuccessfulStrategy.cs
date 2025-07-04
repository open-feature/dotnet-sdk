using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.Strategies.Models;

namespace OpenFeature.Providers.MultiProvider.Strategies;

/// <summary>
/// Return the first result that did not result in an error.
/// If any provider in the course of evaluation returns or throws an error, ignore it as long as there is a successful result.
/// If there is no successful result, throw all errors.
/// </summary>
public sealed class FirstSuccessfulStrategy : BaseEvaluationStrategy
{
    /// <inheritdoc/>
    public override bool ShouldEvaluateNextProvider<T>(StrategyPerProviderContext strategyContext, EvaluationContext? evaluationContext, ProviderResolutionResult<T> result)
    {
        // evaluate next only if there was an error
        return HasError(result);
    }

    /// <inheritdoc/>
    public override FinalResult<T> DetermineFinalResult<T>(StrategyEvaluationContext strategyContext, string key, T defaultValue, EvaluationContext? evaluationContext, List<ProviderResolutionResult<T>> resolutions)
    {
        if (resolutions.Count == 0)
        {
            var noProvidersDetails = new ResolutionDetails<T>(key, defaultValue, ErrorType.ProviderNotReady, Reason.Error, errorMessage: "No providers available or all providers failed");
            var noProvidersErrors = new List<ProviderError>
            {
                new("MultiProvider", new InvalidOperationException("No providers available or all providers failed"))
            };
            return new FinalResult<T>(noProvidersDetails, null!, "MultiProvider", noProvidersErrors);
        }

        // Find the first successful result
        var successfulResult = resolutions.FirstOrDefault(r => !HasError(r));
        if (successfulResult != null)
        {
            return ToFinalResult(successfulResult);
        }

        // All results had errors - collect them and throw
        var collectedErrors = CollectProviderErrors(resolutions);
        var allFailedDetails = new ResolutionDetails<T>(key, defaultValue, ErrorType.General, Reason.Error, errorMessage: "All providers failed");
        return new FinalResult<T>(allFailedDetails, null!, "MultiProvider", collectedErrors);
    }
}
