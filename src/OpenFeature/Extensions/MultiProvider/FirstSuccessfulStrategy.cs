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
            var result = typeof(T) switch
            {
                { } t when t == typeof(bool) => (ResolutionDetails<T>)(object)await provider.ResolveBooleanValueAsync(key, (bool)(object)defaultValue!, evaluationContext, cancellationToken).ConfigureAwait(false),
                { } t when t == typeof(string) => (ResolutionDetails<T>)(object)await provider.ResolveStringValueAsync(key, (string)(object)defaultValue!, evaluationContext, cancellationToken).ConfigureAwait(false),
                { } t when t == typeof(int) => (ResolutionDetails<T>)(object)await provider.ResolveIntegerValueAsync(key, (int)(object)defaultValue!, evaluationContext, cancellationToken).ConfigureAwait(false),
                { } t when t == typeof(double) => (ResolutionDetails<T>)(object)await provider.ResolveDoubleValueAsync(key, (double)(object)defaultValue!, evaluationContext, cancellationToken).ConfigureAwait(false),
                { } t when t == typeof(Value) => (ResolutionDetails<T>)(object)await provider.ResolveStructureValueAsync(key, (Value)(object)defaultValue!, evaluationContext, cancellationToken).ConfigureAwait(false),
                _ => new ResolutionDetails<T>(key, defaultValue, ErrorType.TypeMismatch, Reason.Error, errorMessage: $"Unsupported type: {typeof(T).Name}")
            };

            // If the result is not FLAG_NOT_FOUND and is not an error, return it
            if (result.ErrorType is ErrorType.None or not ErrorType.FlagNotFound)
            {
                return result;
            }
        }

        return new ResolutionDetails<T>(key, defaultValue, ErrorType.FlagNotFound, Reason.Error, errorMessage: "Flag not found in any provider");
    }
}
