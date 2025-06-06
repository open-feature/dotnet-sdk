using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature.Extensions.MultiProvider;

/// <summary>
/// Return the first result returned by a provider. Skip providers that indicate they had no value due to FLAG_NOT_FOUND.
/// In all other cases, use the value returned by the provider. If any provider returns an error result other than FLAG_NOT_FOUND,
/// the whole evaluation should error and "bubble up" the individual provider's error in the result.
/// As soon as a value is returned by a provider, the rest of the operation should short-circuit and not call the rest of the providers.
/// </summary>
public sealed class FirstMatchStrategy : BaseEvaluationStrategy
{
    /// <inheritdoc/>
    public override async Task<ResolutionDetails<T>> EvaluateAsync<T>(Dictionary<string, FeatureProvider> providers, string key, T defaultValue, EvaluationContext? evaluationContext = null, CancellationToken cancellationToken = default)
    {
        foreach (var provider in providers.Values)
        {
            var result = typeof(T) switch
            {
                { } t when t == typeof(bool) => (ResolutionDetails<T>)(object)await provider.ResolveBooleanValueAsync(key, (bool)(object)defaultValue!, evaluationContext, cancellationToken).ConfigureAwait(false),
                { } t when t == typeof(string) => (ResolutionDetails<T>)(object)await provider.ResolveStringValueAsync(key, (string)(object)defaultValue!, evaluationContext, cancellationToken).ConfigureAwait(false),
                { } t when t == typeof(int) => (ResolutionDetails<T>)(object)await provider.ResolveIntegerValueAsync(key, (int)(object)defaultValue!, evaluationContext, cancellationToken).ConfigureAwait(false),
                { } t when t == typeof(double) => (ResolutionDetails<T>)(object)await provider.ResolveDoubleValueAsync(key, (double)(object)defaultValue!, evaluationContext, cancellationToken).ConfigureAwait(false),
                { } t when t == typeof(Value) => (ResolutionDetails<T>)(object)await provider.ResolveStructureValueAsync(key, (Value)(object)defaultValue!, evaluationContext, cancellationToken).ConfigureAwait(false),
                _ => new ResolutionDetails<T>(key, defaultValue, ErrorType.TypeMismatch, Reason.Error, errorMessage: $"Unsupported type: {typeof(T).Name}")
            };

            if (result.ErrorType is ErrorType.None or not ErrorType.FlagNotFound)
            {
                return result;
            }
        }

        return new ResolutionDetails<T>(key, defaultValue, ErrorType.FlagNotFound, Reason.Error, errorMessage: "Flag not found in any provider");
    }
}
