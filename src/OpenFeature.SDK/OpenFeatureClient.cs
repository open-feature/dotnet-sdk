using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenFeature.SDK.Constant;
using OpenFeature.SDK.Error;
using OpenFeature.SDK.Extension;
using OpenFeature.SDK.Model;

namespace OpenFeature.SDK
{
    /// <summary>
    ///
    /// </summary>
    public sealed class FeatureClient : IFeatureClient
    {
        private readonly ClientMetadata _metadata;
        private readonly FeatureProvider _featureProvider;
        private readonly List<Hook> _hooks = new List<Hook>();
        private readonly ILogger _logger;
        private EvaluationContext _evaluationContext;

        /// <summary>
        /// Gets the EvaluationContext of this client<see cref="EvaluationContext"/>
        /// </summary>
        /// <returns><see cref="EvaluationContext"/>of this client</returns>
        public EvaluationContext GetContext() => this._evaluationContext;

        /// <summary>
        /// Sets the EvaluationContext of the client<see cref="EvaluationContext"/>
        /// </summary>
        public void SetContext(EvaluationContext evaluationContext) => this._evaluationContext = evaluationContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureClient"/> class.
        /// </summary>
        /// <param name="featureProvider">Feature provider used by client <see cref="FeatureProvider"/></param>
        /// <param name="name">Name of client <see cref="ClientMetadata"/></param>
        /// <param name="version">Version of client <see cref="ClientMetadata"/></param>
        /// <param name="logger">Logger used by client</param>
        /// <param name="context">Context given to this client</param>
        /// <exception cref="ArgumentNullException">Throws if any of the required parameters are null</exception>
        public FeatureClient(FeatureProvider featureProvider, string name, string version, ILogger logger = null, EvaluationContext context = null)
        {
            this._featureProvider = featureProvider ?? throw new ArgumentNullException(nameof(featureProvider));
            this._metadata = new ClientMetadata(name, version);
            this._logger = logger ?? new Logger<OpenFeature>(new NullLoggerFactory());
            this._evaluationContext = context ?? new EvaluationContext();
        }

        /// <summary>
        /// Gets client metadata
        /// </summary>
        /// <returns>Client metadata <see cref="ClientMetadata"/></returns>
        public ClientMetadata GetMetadata() => this._metadata;

        /// <summary>
        /// Add hook to client
        /// </summary>
        /// <param name="hook">Hook that implements the <see cref="Hook"/> interface</param>
        public void AddHooks(Hook hook) => this._hooks.Add(hook);

        /// <summary>
        /// Appends hooks to client
        /// </summary>
        /// <param name="hooks">A list of Hooks that implement the <see cref="Hook"/> interface</param>
        public void AddHooks(IEnumerable<Hook> hooks) => this._hooks.AddRange(hooks);

        /// <summary>
        /// Return a immutable list of hooks that are registered against the client
        /// </summary>
        /// <returns>A list of immutable hooks</returns>
        public IReadOnlyList<Hook> GetHooks() => this._hooks.ToList().AsReadOnly();

        /// <summary>
        /// Removes all hooks from the client
        /// </summary>
        public void ClearHooks() => this._hooks.Clear();

        /// <summary>
        /// Resolves a boolean feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        public async Task<bool> GetBooleanValue(string flagKey, bool defaultValue, EvaluationContext context = null,
            FlagEvaluationOptions config = null) =>
            (await this.GetBooleanDetails(flagKey, defaultValue, context, config)).Value;

        /// <summary>
        /// Resolves a boolean feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        public async Task<FlagEvaluationDetails<bool>> GetBooleanDetails(string flagKey, bool defaultValue,
            EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            await this.EvaluateFlag(this._featureProvider.ResolveBooleanValue, FlagValueType.Boolean, flagKey,
                defaultValue, context, config);

        /// <summary>
        /// Resolves a string feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        public async Task<string> GetStringValue(string flagKey, string defaultValue, EvaluationContext context = null,
            FlagEvaluationOptions config = null) =>
            (await this.GetStringDetails(flagKey, defaultValue, context, config)).Value;

        /// <summary>
        /// Resolves a string feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        public async Task<FlagEvaluationDetails<string>> GetStringDetails(string flagKey, string defaultValue,
            EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            await this.EvaluateFlag(this._featureProvider.ResolveStringValue, FlagValueType.String, flagKey,
                defaultValue, context, config);

        /// <summary>
        /// Resolves a integer feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        public async Task<int> GetIntegerValue(string flagKey, int defaultValue, EvaluationContext context = null,
            FlagEvaluationOptions config = null) =>
            (await this.GetIntegerDetails(flagKey, defaultValue, context, config)).Value;

        /// <summary>
        /// Resolves a integer feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        public async Task<FlagEvaluationDetails<int>> GetIntegerDetails(string flagKey, int defaultValue,
            EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            await this.EvaluateFlag(this._featureProvider.ResolveIntegerValue, FlagValueType.Number, flagKey,
                defaultValue, context, config);

        /// <summary>
        /// Resolves a double feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        public async Task<double> GetDoubleValue(string flagKey, double defaultValue,
            EvaluationContext context = null,
            FlagEvaluationOptions config = null) =>
            (await this.GetDoubleDetails(flagKey, defaultValue, context, config)).Value;

        /// <summary>
        /// Resolves a double feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        public async Task<FlagEvaluationDetails<double>> GetDoubleDetails(string flagKey, double defaultValue,
            EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            await this.EvaluateFlag(this._featureProvider.ResolveDoubleValue, FlagValueType.Number, flagKey,
                defaultValue, context, config);

        /// <summary>
        /// Resolves a structure object feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        public async Task<Structure> GetObjectValue(string flagKey, Structure defaultValue, EvaluationContext context = null,
            FlagEvaluationOptions config = null) =>
            (await this.GetObjectDetails(flagKey, defaultValue, context, config)).Value;

        /// <summary>
        /// Resolves a structure object feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        public async Task<FlagEvaluationDetails<Structure>> GetObjectDetails(string flagKey, Structure defaultValue,
            EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            await this.EvaluateFlag(this._featureProvider.ResolveStructureValue, FlagValueType.Object, flagKey,
                defaultValue, context, config);

        private async Task<FlagEvaluationDetails<T>> EvaluateFlag<T>(
            Func<string, T, EvaluationContext, Task<ResolutionDetails<T>>> resolveValueDelegate,
            FlagValueType flagValueType, string flagKey, T defaultValue, EvaluationContext context = null,
            FlagEvaluationOptions options = null)
        {
            // New up a evaluation context if one was not provided.
            if (context == null)
            {
                context = new EvaluationContext();
            }

            // merge api, client, and invocation context.
            var evaluationContext = OpenFeature.Instance.GetContext();
            evaluationContext.Merge(this.GetContext());
            evaluationContext.Merge(context);

            var allHooks = new List<Hook>()
                .Concat(OpenFeature.Instance.GetHooks())
                .Concat(this._hooks)
                .Concat(options?.Hooks ?? Enumerable.Empty<Hook>())
                .Concat(this._featureProvider.GetProviderHooks())
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
                    (await resolveValueDelegate.Invoke(flagKey, defaultValue, hookContext.EvaluationContext))
                    .ToFlagEvaluationDetails();

                await this.TriggerAfterHooks(allHooksReversed, hookContext, evaluation, options);
            }
            catch (FeatureProviderException ex)
            {
                this._logger.LogError(ex, "Error while evaluating flag {FlagKey}. Error {ErrorType}", flagKey,
                    ex.ErrorDescription);
                evaluation = new FlagEvaluationDetails<T>(flagKey, defaultValue, ex.ErrorDescription, Reason.Error,
                    string.Empty);
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

        private async Task TriggerBeforeHooks<T>(IReadOnlyList<Hook> hooks, HookContext<T> context,
            FlagEvaluationOptions options)
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

        private async Task TriggerAfterHooks<T>(IReadOnlyList<Hook> hooks, HookContext<T> context,
            FlagEvaluationDetails<T> evaluationDetails, FlagEvaluationOptions options)
        {
            foreach (var hook in hooks)
            {
                await hook.After(context, evaluationDetails, options?.HookHints);
            }
        }

        private async Task TriggerErrorHooks<T>(IReadOnlyList<Hook> hooks, HookContext<T> context, Exception exception,
            FlagEvaluationOptions options)
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

        private async Task TriggerFinallyHooks<T>(IReadOnlyList<Hook> hooks, HookContext<T> context,
            FlagEvaluationOptions options)
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
