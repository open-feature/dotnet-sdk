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
    public override Task<ResolutionDetails<T>> EvaluateAsync<T>(Dictionary<string, FeatureProvider> providers, string key, T defaultValue, EvaluationContext? evaluationContext = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
