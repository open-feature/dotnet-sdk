using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.Strategies.Models;

namespace OpenFeature.Providers.MultiProvider;

internal static class ProviderExtensions
{
    internal static async Task<ProviderResolutionResult<T>> EvaluateAsync<T>(
        this FeatureProvider provider,
        StrategyPerProviderContext providerContext,
        EvaluationContext? evaluationContext,
        T defaultValue,
        CancellationToken cancellationToken)
    {
        var key = providerContext.FlagKey;

        try
        {
            // Perform the actual flag resolution
            var result = typeof(T) switch
            {
                { } t when t == typeof(bool) => (ResolutionDetails<T>)(object)await provider.ResolveBooleanValueAsync(key, (bool)(object)defaultValue!, evaluationContext, cancellationToken).ConfigureAwait(false),
                { } t when t == typeof(string) => (ResolutionDetails<T>)(object)await provider.ResolveStringValueAsync(key, (string)(object)defaultValue!, evaluationContext, cancellationToken).ConfigureAwait(false),
                { } t when t == typeof(int) => (ResolutionDetails<T>)(object)await provider.ResolveIntegerValueAsync(key, (int)(object)defaultValue!, evaluationContext, cancellationToken).ConfigureAwait(false),
                { } t when t == typeof(double) => (ResolutionDetails<T>)(object)await provider.ResolveDoubleValueAsync(key, (double)(object)defaultValue!, evaluationContext, cancellationToken).ConfigureAwait(false),
                { } t when t == typeof(Value) => (ResolutionDetails<T>)(object)await provider.ResolveStructureValueAsync(key, (Value)(object)defaultValue!, evaluationContext, cancellationToken).ConfigureAwait(false),
                _ => throw new ArgumentException($"Unsupported flag type: {typeof(T)}")
            };

            return new ProviderResolutionResult<T>(provider, providerContext.ProviderName, result);
        }
        catch (Exception ex)
        {
            // Create an error result
            var errorResult = new ResolutionDetails<T>(
                key,
                defaultValue,
                ErrorType.General,
                Reason.Error,
                errorMessage: ex.Message);

            return new ProviderResolutionResult<T>(provider, providerContext.ProviderName, errorResult, ex);
        }
    }
}
