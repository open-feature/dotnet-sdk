using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenFeatureSDK.Constant;
using OpenFeatureSDK.Error;
using OpenFeatureSDK.Extension;
using OpenFeatureSDK.Model;

namespace OpenFeatureSDK
{
    /// <summary>
    ///
    /// </summary>
    public sealed class FeatureClient : IFeatureClient
    {
        private readonly ClientMetadata _metadata;
        private readonly ConcurrentStack<Hook> _hooks = new ConcurrentStack<Hook>();
        private readonly ILogger _logger;
        private EvaluationContext _evaluationContext;

        /// <summary>
        /// Get a provider and an associated typed flag resolution method.
        /// <para>
        /// The global provider could change between two accesses, so in order to safely get provider information we
        /// must first alias it and then use that alias to access everything we need.
        /// </para>
        /// </summary>
        /// <param name="method">
        ///     This method should return the desired flag resolution method from the given provider reference.
        /// </param>
        /// <typeparam name="T">The type of the resolution method</typeparam>
        /// <returns>A tuple containing a resolution method and the provider it came from.</returns>
        private (Func<string, T, EvaluationContext, Task<ResolutionDetails<T>>>, FeatureProvider)
            ExtractProvider<T>(
                Func<FeatureProvider, Func<string, T, EvaluationContext, Task<ResolutionDetails<T>>>> method)
        {
            // Alias the provider reference so getting the method and returning the provider are
            // guaranteed to be the same object.
            var provider = OpenFeature.Instance.GetProvider();

            if (provider == null)
            {
                provider = new NoOpFeatureProvider();
                this._logger.LogDebug("No provider configured, using no-op provider");
            }

            return (method(provider), provider);
        }

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
        /// <param name="name">Name of client <see cref="ClientMetadata"/></param>
        /// <param name="version">Version of client <see cref="ClientMetadata"/></param>
        /// <param name="logger">Logger used by client</param>
        /// <param name="context">Context given to this client</param>
        /// <exception cref="ArgumentNullException">Throws if any of the required parameters are null</exception>
        public FeatureClient(string name, string version, ILogger logger = null, EvaluationContext context = null)
        {
            this._metadata = new ClientMetadata(name, version);
            this._logger = logger ?? new Logger<OpenFeature>(new NullLoggerFactory());
            this._evaluationContext = context ?? EvaluationContext.Empty;
        }

        /// <summary>
        /// Gets client metadata
        /// </summary>
        /// <returns>Client metadata <see cref="ClientMetadata"/></returns>
        public ClientMetadata GetMetadata() => this._metadata;

        /// <summary>
        /// Add hook to client
        /// <para>
        /// Hooks which are dependent on each other should be provided in a collection
        /// using the <see cref="AddHooks(System.Collections.Generic.IEnumerable{OpenFeatureSDK.Hook})"/>.
        /// </para>
        /// </summary>
        /// <param name="hook">Hook that implements the <see cref="Hook"/> interface</param>
        public void AddHooks(Hook hook) => this._hooks.Push(hook);

        /// <summary>
        /// Appends hooks to client
        /// <para>
        /// The appending operation will be atomic.
        /// </para>
        /// </summary>
        /// <param name="hooks">A list of Hooks that implement the <see cref="Hook"/> interface</param>
        public void AddHooks(IEnumerable<Hook> hooks) => this._hooks.PushRange(hooks.ToArray());

        /// <summary>
        /// Enumerates the global hooks.
        /// <para>
        /// The items enumerated will reflect the registered hooks
        /// at the start of enumeration. Hooks added during enumeration
        /// will not be included.
        /// </para>
        /// </summary>
        /// <returns>Enumeration of <see cref="Hook"/></returns>
        public IEnumerable<Hook> GetHooks() => this._hooks.Reverse();

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
            await this.EvaluateFlag(this.ExtractProvider<bool>(provider => provider.ResolveBooleanValue),
                FlagValueType.Boolean, flagKey,
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
            await this.EvaluateFlag(this.ExtractProvider<string>(provider => provider.ResolveStringValue),
                FlagValueType.String, flagKey,
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
            await this.EvaluateFlag(this.ExtractProvider<int>(provider => provider.ResolveIntegerValue),
                FlagValueType.Number, flagKey,
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
            await this.EvaluateFlag(this.ExtractProvider<double>(provider => provider.ResolveDoubleValue),
                FlagValueType.Number, flagKey,
                defaultValue, context, config);

        /// <summary>
        /// Resolves a structure object feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext">Evaluation Context</see></param>
        /// <param name="config"><see cref="EvaluationContext">Flag Evaluation Options</see></param>
        /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
        public async Task<Value> GetObjectValue(string flagKey, Value defaultValue, EvaluationContext context = null,
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
        public async Task<FlagEvaluationDetails<Value>> GetObjectDetails(string flagKey, Value defaultValue,
            EvaluationContext context = null, FlagEvaluationOptions config = null) =>
            await this.EvaluateFlag(this.ExtractProvider<Value>(provider => provider.ResolveStructureValue),
                FlagValueType.Object, flagKey,
                defaultValue, context, config);

        private async Task<FlagEvaluationDetails<T>> EvaluateFlag<T>(
            (Func<string, T, EvaluationContext, Task<ResolutionDetails<T>>>, FeatureProvider) providerInfo,
            FlagValueType flagValueType, string flagKey, T defaultValue, EvaluationContext context = null,
            FlagEvaluationOptions options = null)
        {
            var resolveValueDelegate = providerInfo.Item1;
            var provider = providerInfo.Item2;

            // New up a evaluation context if one was not provided.
            if (context == null)
            {
                context = EvaluationContext.Empty;
            }

            // merge api, client, and invocation context.
            var evaluationContext = OpenFeature.Instance.GetContext();
            var evaluationContextBuilder = EvaluationContext.Builder();
            evaluationContextBuilder.Merge(evaluationContext);
            evaluationContextBuilder.Merge(this.GetContext());
            evaluationContextBuilder.Merge(context);

            var allHooks = new List<Hook>()
                .Concat(OpenFeature.Instance.GetHooks())
                .Concat(this.GetHooks())
                .Concat(options?.Hooks ?? Enumerable.Empty<Hook>())
                .Concat(provider.GetProviderHooks())
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
                provider.GetMetadata(),
                evaluationContextBuilder.Build()
            );

            FlagEvaluationDetails<T> evaluation;
            try
            {
                var contextFromHooks = await this.TriggerBeforeHooks(allHooks, hookContext, options);

                evaluation =
                    (await resolveValueDelegate.Invoke(flagKey, defaultValue, contextFromHooks.EvaluationContext))
                    .ToFlagEvaluationDetails();

                await this.TriggerAfterHooks(allHooksReversed, hookContext, evaluation, options);
            }
            catch (FeatureProviderException ex)
            {
                this._logger.LogError(ex, "Error while evaluating flag {FlagKey}. Error {ErrorType}", flagKey,
                    ex.ErrorType.GetDescription());
                evaluation = new FlagEvaluationDetails<T>(flagKey, defaultValue, ex.ErrorType, Reason.Error,
                    string.Empty, ex.Message);
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

        private async Task<HookContext<T>> TriggerBeforeHooks<T>(IReadOnlyList<Hook> hooks, HookContext<T> context,
            FlagEvaluationOptions options)
        {
            var evalContextBuilder = EvaluationContext.Builder();
            evalContextBuilder.Merge(context.EvaluationContext);

            foreach (var hook in hooks)
            {
                var resp = await hook.Before(context, options?.HookHints);
                if (resp != null)
                {
                    evalContextBuilder.Merge(resp);
                    context = context.WithNewEvaluationContext(evalContextBuilder.Build());
                }
                else
                {
                    this._logger.LogDebug("Hook {HookName} returned null, nothing to merge back into context",
                        hook.GetType().Name);
                }
            }

            return context.WithNewEvaluationContext(evalContextBuilder.Build());
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
