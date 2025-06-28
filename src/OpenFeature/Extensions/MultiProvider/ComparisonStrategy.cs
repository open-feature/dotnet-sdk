using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature.Extensions.MultiProvider;

/// <summary>
/// Require that all providers agree on a value. If every provider returns a non-error result, and the values do not agree,
/// the Multi-Provider should return the result from a configurable "fallback" provider. It will also call an optional
/// "onMismatch" callback that can be used to monitor cases where mismatches of evaluation occurred.
/// Otherwise the value of the result will be the result of the first provider in precedence order.
/// </summary>
public sealed class ComparisonStrategy : BaseEvaluationStrategy
{
    private readonly string? _fallbackProviderName;
    private readonly Action<string, object?, Dictionary<string, object?>>? _onMismatch;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComparisonStrategy"/> class.
    /// </summary>
    /// <param name="fallbackProviderName">The name of the provider to use as fallback when values don't match. If null, uses the first provider's result.</param>
    /// <param name="onMismatch">Optional callback that is called when providers return different values.</param>
    public ComparisonStrategy(string? fallbackProviderName = null, Action<string, object?, Dictionary<string, object?>>? onMismatch = null)
    {
        _fallbackProviderName = fallbackProviderName;
        _onMismatch = onMismatch;
    }

    /// <inheritdoc/>
    public override async Task<ResolutionDetails<T>> EvaluateAsync<T>(Dictionary<string, FeatureProvider> providers, string key, T defaultValue, EvaluationContext? evaluationContext = null, CancellationToken cancellationToken = default)
    {
        var results = new List<(string providerName, ResolutionDetails<T> result)>();

        // Evaluate all providers
        foreach (var (providerName, provider) in providers)
        {
            var result = await provider.EvaluateAsync(key, defaultValue, evaluationContext, cancellationToken).ConfigureAwait(false);
            results.Add((providerName, result));

            // If any provider returns an error, return that error immediately
            if (result.ErrorType != ErrorType.None)
            {
                return result;
            }
        }

        // If no results, return flag not found
        if (results.Count == 0)
        {
            return new ResolutionDetails<T>(key, defaultValue, ErrorType.FlagNotFound, Reason.Error, errorMessage: "No providers available");
        }

        // Check if all successful results agree
        var successfulResults = results.Where(r => r.result.ErrorType == ErrorType.None).ToList();
        if (successfulResults.Count == 0)
        {
            return new ResolutionDetails<T>(key, defaultValue, ErrorType.FlagNotFound, Reason.Error, errorMessage: "No successful results from any provider");
        }

        var firstResult = successfulResults.First().result;
        var allAgree = successfulResults.All(r => EqualityComparer<T>.Default.Equals(r.result.Value, firstResult.Value));

        if (allAgree)
        {
            // All providers agree, return the first result
            return firstResult;
        }

        // Values don't agree, trigger mismatch callback if provided
        if (_onMismatch != null)
        {
            var mismatchValues = successfulResults.ToDictionary(
                r => r.providerName,
                r => (object?)r.result.Value
            );
            _onMismatch(key, firstResult.Value, mismatchValues);
        }

        // Return fallback provider result if specified and available
        if (!string.IsNullOrEmpty(_fallbackProviderName))
        {
            var fallbackResult = successfulResults.FirstOrDefault(r => r.providerName == _fallbackProviderName);
            if (fallbackResult.result != null)
            {
                return fallbackResult.result;
            }
        }

        // Default to first provider's result
        return firstResult;
    }
}
