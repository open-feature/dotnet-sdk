using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenFeature.Constant;
using OpenFeature.Error;
using OpenFeature.Extention;
using OpenFeature.Model;

namespace OpenFeature
{
    public class FeatureClient : IFeatureClient
    {
        private readonly ClientMetadata _metadata;
        private readonly IFeatureProvider _featureProvider;
        private readonly List<Hook> _hooks = new List<Hook>();
        private readonly ILogger _logger;

        public FeatureClient(IFeatureProvider featureProvider, string name, string version, ILogger logger = null)
        {
            this._featureProvider = featureProvider ?? throw new ArgumentNullException(nameof(featureProvider));
            this._metadata = new ClientMetadata(name, version);
            this._logger = logger ?? new Logger<OpenFeature>(new NullLoggerFactory());
        }

        public ClientMetadata GetMetadata() => this._metadata;

        public void AddHooks(Hook hook) => this._hooks.Add(hook);
        public void AddHooks(IEnumerable<Hook> hooks) => this._hooks.AddRange(hooks);
        public IReadOnlyList<Hook> GetHooks() => this._hooks.ToList();
        public void ClearHooks() => this._hooks.Clear();

        public async Task<bool> GetBooleanValue(string flagKey, bool defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            (await this.GetBooleanDetails(flagKey, defaultValue, context, config)).Value;

        public async Task<FlagEvaluationDetails<bool>> GetBooleanDetails(string flagKey, bool defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            await this.EvaluateFlag(this._featureProvider.ResolveBooleanValue, FlagValueType.Boolean, flagKey, defaultValue, context, config);

        public async Task<string> GetStringValue(string flagKey, string defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            (await this.GetStringDetails(flagKey, defaultValue, context, config)).Value;

        public async Task<FlagEvaluationDetails<string>> GetStringDetails(string flagKey, string defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            await this.EvaluateFlag(this._featureProvider.ResolveStringValue, FlagValueType.String, flagKey, defaultValue, context, config);

        public async Task<int> GetNumberValue(string flagKey, int defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            (await this.GetNumberDetails(flagKey, defaultValue, context, config)).Value;

        public async Task<FlagEvaluationDetails<int>> GetNumberDetails(string flagKey, int defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            await this.EvaluateFlag(this._featureProvider.ResolveNumberValue, FlagValueType.Number, flagKey, defaultValue, context, config);

        public async Task<T> GetObjectValue<T>(string flagKey, T defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            (await this.GetObjectDetails(flagKey, defaultValue, context, config)).Value;

        public async Task<FlagEvaluationDetails<T>> GetObjectDetails<T>(string flagKey, T defaultValue, EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            await this.EvaluateFlag(this._featureProvider.ResolveStructureValue, FlagValueType.Object, flagKey, defaultValue, context, config);

        private async Task<FlagEvaluationDetails<T>> EvaluateFlag<T>(
            Func<string, T, EvaluationContext, FlagEvaluationOptions, Task<ResolutionDetails<T>>> resolveValueDelegate,
            FlagValueType flagValueType, string flagKey, T defaultValue, EvaluationContext context = null, FlagEvaluationOptions options = null)
        {
            // New up a evaluation context if one was not provided.
            if (context == null)
            {
                context = new EvaluationContext();
            }

            var evaluationContext = OpenFeature.Instance.GetContext();
            evaluationContext.Merge(context);

            var allHooks = new List<Hook>()
                .Concat(OpenFeature.Instance.GetHooks())
                .Concat(this._hooks)
                .Concat(options?.Hooks ?? Enumerable.Empty<Hook>())
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
                flagValueType, this._metadata,
                OpenFeature.Instance.GetProviderMetadata(),
                evaluationContext
            );

            FlagEvaluationDetails<T> evaluation;
            try
            {
                await this.TriggerBeforeHooks(allHooks, hookContext, options);

                evaluation =
                    (await resolveValueDelegate.Invoke(flagKey, defaultValue, hookContext.EvaluationContext, options))
                    .ToFlagEvaluationDetails();

                await this.TriggerAfterHooks(allHooksReversed, hookContext, evaluation, options);
            }
            catch (FeatureProviderException ex)
            {
                this._logger.LogError(ex, "Error while evaluating flag {FlagKey}. Error {ErrorType}", flagKey, ex.ErrorTypeDescription);
                evaluation = new FlagEvaluationDetails<T>(flagKey, defaultValue, ex.ErrorType, Reason.Error, string.Empty);
                await this.TriggerErrorHooks(allHooksReversed, hookContext, ex, options);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error while evaluating flag {FlagKey}", flagKey);
                var errorCode = ex is InvalidCastException ? ErrorType.TypeMismatch : ErrorType.General;
                evaluation = new FlagEvaluationDetails<T>(flagKey, defaultValue, errorCode, Reason.Error, string.Empty);
                await this.TriggerErrorHooks(allHooksReversed, hookContext, ex, options);
            }
            finally
            {
                await this.TriggerFinallyHooks(allHooksReversed, hookContext, options);
            }

            return evaluation;
        }

        private async Task TriggerBeforeHooks<T>(IReadOnlyList<IHook> hooks, HookContext<T> context, FlagEvaluationOptions options)
        {
            foreach (var hook in hooks)
            {
                var resp = await hook.Before(context, options?.HookHints);
                if (resp != null)
                {
                    context.EvaluationContext.Merge(resp);
                }
                else
                {
                    this._logger.LogDebug("Hook {HookName} returned null, nothing to merge back into context",
                        hook.GetType().Name);
                }
            }
        }

        private async Task TriggerAfterHooks<T>(IReadOnlyList<IHook> hooks, HookContext<T> context, FlagEvaluationDetails<T> evaluationDetails, FlagEvaluationOptions options)
        {
            foreach (var hook in hooks)
            {
                await hook.After(context, evaluationDetails, options?.HookHints);
            }
        }

        private async Task TriggerErrorHooks<T>(IReadOnlyList<IHook> hooks, HookContext<T> context, Exception exception, FlagEvaluationOptions options)
        {
            foreach (var hook in hooks)
            {
                try
                {
                    await hook.Error(context, exception, options?.HookHints);
                }
                catch (Exception e)
                {
                    this._logger.LogError(e, "Error while executing Error hook {0}", hook.GetType().Name);
                }
            }
        }

        private async Task TriggerFinallyHooks<T>(IReadOnlyList<IHook> hooks, HookContext<T> context, FlagEvaluationOptions options)
        {
            foreach (var hook in hooks)
            {
                try
                {
                    await hook.Finally(context, options?.HookHints);
                }
                catch (Exception e)
                {
                    this._logger.LogError(e, "Error while executing Finally hook {0}", hook.GetType().Name);
                }
            }
        }
    }
}
