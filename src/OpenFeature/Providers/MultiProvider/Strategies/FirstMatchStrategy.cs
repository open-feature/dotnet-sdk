using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.Strategies.Models;

namespace OpenFeature.Providers.MultiProvider.Strategies;

/// <summary>
/// Return the first result that did not indicate "flag not found".
/// Providers are evaluated sequentially in the order they were configured.
/// If any provider in the course of evaluation returns or throws an error, throw that error
/// </summary>
public sealed class FirstMatchStrategy : BaseEvaluationStrategy
{
    /// <inheritdoc/>
    public override bool ShouldEvaluateNextProvider<T>(StrategyPerProviderContext<T> strategyContext, EvaluationContext? evaluationContext, ProviderResolutionResult<T> result)
    {
        return HasErrorWithCode(result, ErrorType.FlagNotFound);
    }

    /// <inheritdoc/>
    public override FinalResult<T> DetermineFinalResult<T>(StrategyEvaluationContext<T> strategyContext, string key, T defaultValue, EvaluationContext? evaluationContext, List<ProviderResolutionResult<T>> resolutions)
    {
        var lastResult = resolutions.LastOrDefault();
        if (lastResult != null)
        {
            return ToFinalResult(lastResult);
        }

        var errorDetails = new ResolutionDetails<T>(key, defaultValue, ErrorType.ProviderNotReady, Reason.Error, errorMessage: "No providers available or all providers failed");
        var errors = new List<ProviderError>
        {
            new("MultiProvider", new InvalidOperationException("No providers available or all providers failed"))
        };
        return new FinalResult<T>(errorDetails, null!, "MultiProvider", errors);
    }
}
