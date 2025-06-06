using OpenFeature.Model;

namespace OpenFeature.Extensions.MultiProvider;

/// <summary>
/// Require that all providers agree on a value. If every provider returns a non-error result, and the values do not agree,
/// the Multi-Provider should return the result from a configurable “fallback” provider. It will also call an optional “onMismatch”
/// callback that can be used to monitor cases where mismatches of evaluation occurred. Otherwise the value of the result will be
/// the result of the first provider in precedence order.
/// </summary>
public sealed class ComparisonStrategy : BaseEvaluationStrategy
{
    /// <inheritdoc/>
    public override Task<ResolutionDetails<T>> EvaluateAsync<T>(Dictionary<string, FeatureProvider> providers, string key, T defaultValue, EvaluationContext? evaluationContext = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
