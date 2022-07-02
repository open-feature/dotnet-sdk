using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenFeature.Constant;
using OpenFeature.Extention;
using OpenFeature.Model;

namespace OpenFeature
{
    public class FeatureClient : IFeatureClient
    {
        private readonly ClientMetadata _metadata;
        private readonly IFeatureProvider _featureProvider;
        private readonly List<Hook> _hooks = new List<Hook>();

        public FeatureClient(IFeatureProvider featureProvider, string name, string version)
        {
            _featureProvider = featureProvider ?? throw new ArgumentNullException(nameof(featureProvider));
            _metadata = new ClientMetadata(name, version);
        }

        public ClientMetadata GetMetadata() => _metadata;

        public void AddHooks(Hook hook) => _hooks.Add(hook);
        public void AddHooks(IEnumerable<Hook> hooks) => _hooks.AddRange(hooks);
        public IReadOnlyList<Hook> GetHooks() => _hooks.ToList();
        public void ClearHooks() => _hooks.Clear();

        public async Task<bool> GetBooleanValue(string flagKey, bool defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            (await GetBooleanDetails(flagKey, defaultValue, context, config)).Value;

        public async Task<FlagEvaluationDetails<bool>> GetBooleanDetails(string flagKey, bool defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            await EvaluateFlag(_featureProvider.ResolveBooleanValue, FlagValueType.Boolean, flagKey, defaultValue, context, config);

        public async Task<string> GetStringValue(string flagKey, string defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            (await GetStringDetails(flagKey, defaultValue, context, config)).Value;

        public async Task<FlagEvaluationDetails<string>> GetStringDetails(string flagKey, string defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            await EvaluateFlag(_featureProvider.ResolveStringValue, FlagValueType.String, flagKey, defaultValue, context, config);

        public async Task<int> GetNumberValue(string flagKey, int defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            (await GetNumberDetails(flagKey, defaultValue, context, config)).Value;

        public async Task<FlagEvaluationDetails<int>> GetNumberDetails(string flagKey, int defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            await EvaluateFlag(_featureProvider.ResolveNumberValue, FlagValueType.Number, flagKey, defaultValue, context, config);

        public async Task<T> GetObjectValue<T>(string flagKey, T defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            (await GetObjectDetails(flagKey, defaultValue, context, config)).Value;

        public async Task<FlagEvaluationDetails<T>> GetObjectDetails<T>(string flagKey, T defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            await EvaluateFlag(_featureProvider.ResolveStructureValue, FlagValueType.Object, flagKey, defaultValue, context, config);

        private async Task<FlagEvaluationDetails<T>> EvaluateFlag<T>(
            Func<string, T, EvaluationContext, FlagEvaluationOptions, Task<ResolutionDetails<T>>> resolveValueDelegate,
            FlagValueType flagValueType, string flagKey, T defaultValue, EvaluationContext context = null, FlagEvaluationOptions options = null)
        {
            // New up a evaluation context if one was not provided.
            if (context == null)
            {
                context = new EvaluationContext();
            }

            var evaluationContext = OpenFeature.GetContext();
            evaluationContext.Merge(context);

            var allHooks = new List<IHook>()
                .Concat(OpenFeature.GetHooks())
                .Concat(_hooks)
                .ToList()
                .AsReadOnly();

            var allHooksReversed = allHooks
                .AsEnumerable()
                .Reverse()
                .ToList()
                .AsReadOnly();

            var hookContext = new HookContext<T>(
                flagKey,
                defaultValue,
                flagValueType,
                _metadata,
                OpenFeature.GetProviderMetadata(),
                evaluationContext
            );

            FlagEvaluationDetails<T> evaluation;
            try
            {
                await TriggerBeforeHooks(allHooks, hookContext, options);

                evaluation = (await resolveValueDelegate.Invoke(flagKey, defaultValue, hookContext.EvaluationContext, options))
                    .ToFlagEvaluationDetails();

                await TriggerAfterHooks(allHooksReversed, hookContext, evaluation, options);
            }
            catch (Exception e)
            {
                OpenFeature.Logger.LogError(e, "Error while evaluating flag {FlagKey}", flagKey);
                // TODO needs to handle error codes being thrown from provider that maps to Errors enums
                evaluation = new FlagEvaluationDetails<T>(flagKey, defaultValue, e.Message, Reason.Error, string.Empty);
                await TriggerErrorHooks(allHooksReversed, hookContext, e, options);
            }
            finally
            {
                await TriggerFinallyHooks(allHooksReversed, hookContext, options);
            }

            return evaluation;
        }

        private static async Task TriggerBeforeHooks<T>(IReadOnlyList<IHook> hooks, HookContext<T> context, FlagEvaluationOptions options)
        {
            foreach (var hook in hooks)
            {
                try
                {
                    var resp = await hook.Before(context, options?.HookHints);
                    if (resp != null)
                    {
                        context.EvaluationContext.Merge(resp);
                    }
                    else
                    {
                        OpenFeature.Logger.LogDebug("Hook {HookName} returned null, nothing to merge back into context", hook.GetType().Name);
                    }
                }
                catch (Exception e)
                {
                    OpenFeature.Logger.LogError(e, "Error while executing Before hook {0}", hook.GetType().Name);
                }
            }
        }

        private static async Task TriggerAfterHooks<T>(IReadOnlyList<IHook> hooks, HookContext<T> context, FlagEvaluationDetails<T> evaluationDetails, FlagEvaluationOptions options)
        {
            foreach (var hook in hooks)
            {
                try
                {
                    await hook.After(context, evaluationDetails, options?.HookHints);
                }
                catch (Exception e)
                {
                    OpenFeature.Logger.LogError(e, "Error while executing After hook {0}", hook.GetType().Name);
                }
            }
        }

        private static async Task TriggerErrorHooks<T>(IReadOnlyList<IHook> hooks, HookContext<T> context, Exception exception, FlagEvaluationOptions options)
        {
            foreach (var hook in hooks)
            {
                try
                {
                    await hook.Error(context, exception, options?.HookHints);
                }
                catch (Exception e)
                {
                    OpenFeature.Logger.LogError(e, "Error while executing Error hook {0}", hook.GetType().Name);
                }
            }
        }

        private static async Task TriggerFinallyHooks<T>(IReadOnlyList<IHook> hooks, HookContext<T> context, FlagEvaluationOptions options)
        {
            foreach (var hook in hooks)
            {
                try
                {
                    await hook.Finally(context, options?.HookHints);
                }
                catch (Exception e)
                {
                    OpenFeature.Logger.LogError(e, "Error while executing Finally hook {0}", hook.GetType().Name);
                }
            }
        }
    }
}
