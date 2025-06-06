using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature.Extensions.MultiProvider;

/// <summary>
/// Return the first result returned by a provider. Errors from evaluated providers do not halt execution.
/// Instead, it will return the first successful result from a provider.
/// If no provider successfully responds, it will throw an error result.
/// </summary>
public sealed class FirstSuccessfulStrategy : BaseEvaluationStrategy
{
    /// <inheritdoc/>
    public override async Task<ResolutionDetails<T>> EvaluateAsync<T>(Dictionary<string, FeatureProvider> providers, string key, T defaultValue, EvaluationContext? evaluationContext = null, CancellationToken cancellationToken = default)
    {
        foreach (var provider in providers.Values)
        {
            var result = await provider.EvaluateAsync(key, defaultValue, evaluationContext, cancellationToken).ConfigureAwait(false);

            // If the result is not an error, return it
            if (result.ErrorType is ErrorType.None)
            {
                return result;
            }
        }

        return new ResolutionDetails<T>(key, defaultValue, ErrorType.FlagNotFound, Reason.Error, errorMessage: "Flag not found in any provider");
    }
}
