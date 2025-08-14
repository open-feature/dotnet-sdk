using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.Strategies.Models;

namespace OpenFeature.Providers.MultiProvider;

internal static class ProviderExtensions
{
    internal static async Task<ProviderResolutionResult<T>> EvaluateAsync<T>(
        this FeatureProvider provider,
        StrategyPerProviderContext<T> providerContext,
        EvaluationContext? evaluationContext,
        T defaultValue,
        CancellationToken cancellationToken)
    {
        var key = providerContext.FlagKey;

        try
        {
            var result = defaultValue switch
            {
                bool boolDefaultValue => (ResolutionDetails<T>)(object)await provider.ResolveBooleanValueAsync(key, boolDefaultValue, evaluationContext, cancellationToken).ConfigureAwait(false),
                string stringDefaultValue => (ResolutionDetails<T>)(object)await provider.ResolveStringValueAsync(key, stringDefaultValue, evaluationContext, cancellationToken).ConfigureAwait(false),
                int intDefaultValue => (ResolutionDetails<T>)(object)await provider.ResolveIntegerValueAsync(key, intDefaultValue, evaluationContext, cancellationToken).ConfigureAwait(false),
                double doubleDefaultValue => (ResolutionDetails<T>)(object)await provider.ResolveDoubleValueAsync(key, doubleDefaultValue, evaluationContext, cancellationToken).ConfigureAwait(false),
                Value valueDefaultValue => (ResolutionDetails<T>)(object)await provider.ResolveStructureValueAsync(key, valueDefaultValue, evaluationContext, cancellationToken).ConfigureAwait(false),
                null when typeof(T) == typeof(string) => (ResolutionDetails<T>)(object)await provider.ResolveStringValueAsync(key, (string)(object)defaultValue!, evaluationContext, cancellationToken).ConfigureAwait(false),
                null when typeof(T) == typeof(Value) => (ResolutionDetails<T>)(object)await provider.ResolveStructureValueAsync(key, (Value)(object)defaultValue!, evaluationContext, cancellationToken).ConfigureAwait(false),
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
