using OpenFeature.Model;

namespace OpenFeature.Extensions.MultiProvider;

/// <summary>
/// Return the first result returned by a provider. Skip providers that indicate they had no value due to FLAG_NOT_FOUND.
/// In all other cases, use the value returned by the provider. If any provider returns an error result other than FLAG_NOT_FOUND,
/// the whole evaluation should error and “bubble up” the individual provider’s error in the result.
/// As soon as a value is returned by a provider, the rest of the operation should short-circuit and not call the rest of the providers.
/// </summary>
public sealed class FirstMatchStrategy : BaseEvaluationStrategy
{
    /// <inheritdoc/>
    public override Task<ResolutionDetails<T>> EvaluateAsync<T>(Dictionary<string, FeatureProvider> providers, string key, T defaultValue, EvaluationContext? evaluationContext = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
