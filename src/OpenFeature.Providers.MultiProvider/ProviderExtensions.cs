using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using OpenFeature.Constant;
using OpenFeature.Error;
using OpenFeature.Extension;
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
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        var key = providerContext.FlagKey;

        try
        {
            // Execute provider hooks for this specific provider
            var providerHooks = provider.GetProviderHooks();
            EvaluationContext? contextForThisProvider = evaluationContext;

            if (providerHooks.Count > 0)
            {
                // Execute hooks for this provider with context isolation
                var (modifiedContext, hookResult) = await ExecuteBeforeEvaluationHooksAsync(
                    provider,
                    providerHooks,
                    key,
                    defaultValue,
                    evaluationContext,
                    logger,
                    cancellationToken).ConfigureAwait(false);

                if (hookResult != null)
                {
                    return hookResult;
                }

                contextForThisProvider = modifiedContext ?? evaluationContext;
            }

            // Evaluate the flag with the (possibly modified) context
            var result = defaultValue switch
            {
                bool boolDefaultValue => (ResolutionDetails<T>)(object)await provider.ResolveBooleanValueAsync(key, boolDefaultValue, contextForThisProvider, cancellationToken).ConfigureAwait(false),
                string stringDefaultValue => (ResolutionDetails<T>)(object)await provider.ResolveStringValueAsync(key, stringDefaultValue, contextForThisProvider, cancellationToken).ConfigureAwait(false),
                int intDefaultValue => (ResolutionDetails<T>)(object)await provider.ResolveIntegerValueAsync(key, intDefaultValue, contextForThisProvider, cancellationToken).ConfigureAwait(false),
                double doubleDefaultValue => (ResolutionDetails<T>)(object)await provider.ResolveDoubleValueAsync(key, doubleDefaultValue, contextForThisProvider, cancellationToken).ConfigureAwait(false),
                Value valueDefaultValue => (ResolutionDetails<T>)(object)await provider.ResolveStructureValueAsync(key, valueDefaultValue, contextForThisProvider, cancellationToken).ConfigureAwait(false),
                null when typeof(T) == typeof(string) => (ResolutionDetails<T>)(object)await provider.ResolveStringValueAsync(key, (string)(object)defaultValue!, contextForThisProvider, cancellationToken).ConfigureAwait(false),
                null when typeof(T) == typeof(Value) => (ResolutionDetails<T>)(object)await provider.ResolveStructureValueAsync(key, (Value)(object)defaultValue!, contextForThisProvider, cancellationToken).ConfigureAwait(false),
                _ => throw new ArgumentException($"Unsupported flag type: {typeof(T)}")
            };

            // Execute after/finally hooks for this provider if we have them
            if (providerHooks.Count > 0)
            {
                await ExecuteAfterEvaluationHooksAsync(provider, providerHooks, key, defaultValue, contextForThisProvider, result, logger, cancellationToken).ConfigureAwait(false);
            }

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

    private static async Task<(EvaluationContext?, ProviderResolutionResult<T>?)> ExecuteBeforeEvaluationHooksAsync<T>(
        FeatureProvider provider,
        IImmutableList<Hook> hooks,
        string key,
        T defaultValue,
        EvaluationContext? evaluationContext,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var sharedHookContext = new SharedHookContext<T>(
                key,
                defaultValue,
                GetFlagValueType<T>(),
                null, // Provide a client metadata instead of null?
                provider.GetMetadata()
            );

            var initialContext = evaluationContext ?? EvaluationContext.Empty;
            var hookRunner = new HookRunner<T>([.. hooks], initialContext, sharedHookContext, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);

            // Execute before hooks for this provider
            var modifiedContext = await hookRunner.TriggerBeforeHooksAsync(null, cancellationToken).ConfigureAwait(false);
            return (modifiedContext, null);
        }
        catch (Exception hookEx)
        {
            // If before hooks fail, return error result
            var errorResult = new ResolutionDetails<T>(
                key,
                defaultValue,
                ErrorType.General,
                Reason.Error,
                errorMessage: $"Provider hook execution failed: {hookEx.Message}");

            var result = new ProviderResolutionResult<T>(provider, provider.GetMetadata()?.Name ?? "unknown", errorResult, hookEx);
            return (null, result);
        }
    }

    private static async Task ExecuteAfterEvaluationHooksAsync<T>(
        FeatureProvider provider,
        IImmutableList<Hook> hooks,
        string key,
        T defaultValue,
        EvaluationContext? evaluationContext,
        ResolutionDetails<T> result,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var sharedHookContext = new SharedHookContext<T>(
                key,
                defaultValue,
                GetFlagValueType<T>(),
                null, // Provide a client metadata instead of null?
                provider.GetMetadata()
            );

            var hookRunner = new HookRunner<T>([.. hooks], evaluationContext ?? EvaluationContext.Empty, sharedHookContext, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);

            var evaluationDetails = result.ToFlagEvaluationDetails();

            if (result.ErrorType == ErrorType.None)
            {
                await hookRunner.TriggerAfterHooksAsync(evaluationDetails, null, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var exception = new FeatureProviderException(result.ErrorType, result.ErrorMessage);
                await hookRunner.TriggerErrorHooksAsync(exception, null, cancellationToken).ConfigureAwait(false);
            }

            await hookRunner.TriggerFinallyHooksAsync(evaluationDetails, null, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception hookEx)
        {
            // Log hook execution errors but don't fail the evaluation
            logger?.LogWarning(hookEx, "Provider after/finally hook execution failed for provider {ProviderName}", provider.GetMetadata()?.Name ?? "unknown");
        }
    }

    private static FlagValueType GetFlagValueType<T>()
    {
        return typeof(T) switch
        {
            _ when typeof(T) == typeof(bool) => FlagValueType.Boolean,
            _ when typeof(T) == typeof(string) => FlagValueType.String,
            _ when typeof(T) == typeof(int) => FlagValueType.Number,
            _ when typeof(T) == typeof(double) => FlagValueType.Number,
            _ when typeof(T) == typeof(Value) => FlagValueType.Object,
            _ => FlagValueType.Object // Default fallback
        };
    }
}
