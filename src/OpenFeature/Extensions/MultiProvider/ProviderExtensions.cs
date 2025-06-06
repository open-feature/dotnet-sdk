using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature.Extensions.MultiProvider;

internal static class ProviderExtensions
{
    internal static async Task<ResolutionDetails<T>> EvaluateAsync<T>(this FeatureProvider provider, string key, T defaultValue, EvaluationContext? evaluationContext,
        CancellationToken cancellationToken)
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
        return result;
    }
}
