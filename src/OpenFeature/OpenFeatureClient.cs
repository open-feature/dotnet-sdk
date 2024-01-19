using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenFeature.Constant;
using OpenFeature.Error;
using OpenFeature.Extension;
using OpenFeature.Model;

namespace OpenFeature
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

        private readonly object _evaluationContextLock = new object();

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
        private (Func<string, T, EvaluationContext, CancellationToken, Task<ResolutionDetails<T>>>, FeatureProvider)
            ExtractProvider<T>(
                Func<FeatureProvider, Func<string, T, EvaluationContext, CancellationToken, Task<ResolutionDetails<T>>>> method)
        {
            // Alias the provider reference so getting the method and returning the provider are
            // guaranteed to be the same object.
            var provider = Api.Instance.GetProvider(this._metadata.Name);

            return (method(provider), provider);
        }

        /// <inheritdoc />
        public EvaluationContext GetContext()
        {
            lock (this._evaluationContextLock)
            {
                return this._evaluationContext;
            }
        }

        /// <inheritdoc />
        public void SetContext(EvaluationContext context)
        {
            lock (this._evaluationContextLock)
            {
                this._evaluationContext = context ?? EvaluationContext.Empty;
            }
        }

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
            this._logger = logger ?? new Logger<Api>(new NullLoggerFactory());
            this._evaluationContext = context ?? EvaluationContext.Empty;
        }

        /// <inheritdoc />
        public ClientMetadata GetMetadata() => this._metadata;

        /// <summary>
        /// Add hook to client
        /// <para>
        /// Hooks which are dependent on each other should be provided in a collection
        /// using the <see cref="AddHooks(IEnumerable{Hook})"/>.
        /// </para>
        /// </summary>
        /// <param name="hook">Hook that implements the <see cref="Hook"/> interface</param>
        public void AddHooks(Hook hook) => this._hooks.Push(hook);

        /// <inheritdoc />
        public void AddHandler(ProviderEventTypes eventType, EventHandlerDelegate handler)
        {
            Api.Instance.EventExecutor.AddClientHandler(this._metadata.Name, eventType, handler);
        }

        /// <inheritdoc />
        public void RemoveHandler(ProviderEventTypes type, EventHandlerDelegate handler)
        {
            Api.Instance.EventExecutor.RemoveClientHandler(this._metadata.Name, type, handler);
        }

        /// <inheritdoc />
        public void AddHooks(IEnumerable<Hook> hooks)
#if NET7_0_OR_GREATER
            => this._hooks.PushRange(hooks as Hook[] ?? hooks.ToArray());
#else
        {
            // See: https://github.com/dotnet/runtime/issues/62121
            if (hooks is Hook[] array)
            {
                if (array.Length > 0)
                    this._hooks.PushRange(array);

                return;
            }

            array = hooks.ToArray();

            if (array.Length > 0)
                this._hooks.PushRange(array);
        }
#endif

        /// <inheritdoc />
        public IEnumerable<Hook> GetHooks() => this._hooks.Reverse();

        /// <summary>
        /// Removes all hooks from the client
        /// </summary>
        public void ClearHooks() => this._hooks.Clear();

        /// <inheritdoc />
        public async Task<bool> GetBooleanValueAsync(string flagKey, bool defaultValue, EvaluationContext context = null,
            FlagEvaluationOptions config = null, CancellationToken cancellationToken = default) =>
            (await this.GetBooleanDetailsAsync(flagKey, defaultValue, context, config, cancellationToken).ConfigureAwait(false)).Value;

        /// <inheritdoc />
        public async Task<FlagEvaluationDetails<bool>> GetBooleanDetailsAsync(string flagKey, bool defaultValue,
            EvaluationContext context = null, FlagEvaluationOptions config = null, CancellationToken cancellationToken = default) =>
            await this.EvaluateFlagAsync(this.ExtractProvider<bool>(provider => provider.ResolveBooleanValueAsync),
                FlagValueType.Boolean, flagKey,
                defaultValue, context, config, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<string> GetStringValueAsync(string flagKey, string defaultValue, EvaluationContext context = null,
            FlagEvaluationOptions config = null, CancellationToken cancellationToken = default) =>
            (await this.GetStringDetailsAsync(flagKey, defaultValue, context, config, cancellationToken).ConfigureAwait(false)).Value;

        /// <inheritdoc />
        public async Task<FlagEvaluationDetails<string>> GetStringDetailsAsync(string flagKey, string defaultValue,
            EvaluationContext context = null, FlagEvaluationOptions config = null, CancellationToken cancellationToken = default) =>
            await this.EvaluateFlagAsync(this.ExtractProvider<string>(provider => provider.ResolveStringValueAsync),
                FlagValueType.String, flagKey,
                defaultValue, context, config, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<int> GetIntegerValueAsync(string flagKey, int defaultValue, EvaluationContext context = null,
            FlagEvaluationOptions config = null, CancellationToken cancellationToken = default) =>
            (await this.GetIntegerDetailsAsync(flagKey, defaultValue, context, config, cancellationToken).ConfigureAwait(false)).Value;

        /// <inheritdoc />
        public async Task<FlagEvaluationDetails<int>> GetIntegerDetailsAsync(string flagKey, int defaultValue,
            EvaluationContext context = null, FlagEvaluationOptions config = null, CancellationToken cancellationToken = default) =>
            await this.EvaluateFlagAsync(this.ExtractProvider<int>(provider => provider.ResolveIntegerValueAsync),
                FlagValueType.Number, flagKey,
                defaultValue, context, config, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<double> GetDoubleValueAsync(string flagKey, double defaultValue,
            EvaluationContext context = null,
            FlagEvaluationOptions config = null, CancellationToken cancellationToken = default) =>
            (await this.GetDoubleDetailsAsync(flagKey, defaultValue, context, config, cancellationToken).ConfigureAwait(false)).Value;

        /// <inheritdoc />
        public async Task<FlagEvaluationDetails<double>> GetDoubleDetailsAsync(string flagKey, double defaultValue,
            EvaluationContext context = null, FlagEvaluationOptions config = null, CancellationToken cancellationToken = default) =>
            await this.EvaluateFlagAsync(this.ExtractProvider<double>(provider => provider.ResolveDoubleValueAsync),
                FlagValueType.Number, flagKey,
                defaultValue, context, config, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<Value> GetObjectValueAsync(string flagKey, Value defaultValue, EvaluationContext context = null,
            FlagEvaluationOptions config = null, CancellationToken cancellationToken = default) =>
            (await this.GetObjectDetailsAsync(flagKey, defaultValue, context, config, cancellationToken).ConfigureAwait(false)).Value;

        /// <inheritdoc />
        public async Task<FlagEvaluationDetails<Value>> GetObjectDetailsAsync(string flagKey, Value defaultValue,
            EvaluationContext context = null, FlagEvaluationOptions config = null, CancellationToken cancellationToken = default) =>
            await this.EvaluateFlagAsync(this.ExtractProvider<Value>(provider => provider.ResolveStructureValueAsync),
                FlagValueType.Object, flagKey,
                defaultValue, context, config, cancellationToken).ConfigureAwait(false);

        private async Task<FlagEvaluationDetails<T>> EvaluateFlagAsync<T>(
            (Func<string, T, EvaluationContext, CancellationToken, Task<ResolutionDetails<T>>>, FeatureProvider) providerInfo,
            FlagValueType flagValueType, string flagKey, T defaultValue, EvaluationContext context = null,
            FlagEvaluationOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var resolveValueDelegate = providerInfo.Item1;
            var provider = providerInfo.Item2;

            // New up a evaluation context if one was not provided.
            if (context == null)
            {
                context = EvaluationContext.Empty;
            }

            // merge api, client, and invocation context.
            var evaluationContext = Api.Instance.GetContext();
            var evaluationContextBuilder = EvaluationContext.Builder();
            evaluationContextBuilder.Merge(evaluationContext);
            evaluationContextBuilder.Merge(this.GetContext());
            evaluationContextBuilder.Merge(context);

            var allHooks = new List<Hook>()
                .Concat(Api.Instance.GetHooks())
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
                var contextFromHooks = await this.TriggerBeforeHooksAsync(allHooks, hookContext, options, cancellationToken).ConfigureAwait(false);

                evaluation =
                    (await resolveValueDelegate.Invoke(flagKey, defaultValue, contextFromHooks.EvaluationContext, cancellationToken).ConfigureAwait(false))
                    .ToFlagEvaluationDetails();

                await this.TriggerAfterHooksAsync(allHooksReversed, hookContext, evaluation, options, cancellationToken).ConfigureAwait(false);
            }
            catch (FeatureProviderException ex)
            {
                this._logger.LogError(ex, "Error while evaluating flag {FlagKey}. Error {ErrorType}", flagKey,
                    ex.ErrorType.GetDescription());
                evaluation = new FlagEvaluationDetails<T>(flagKey, defaultValue, ex.ErrorType, Reason.Error,
                    string.Empty, ex.Message);
                await this.TriggerErrorHooksAsync(allHooksReversed, hookContext, ex, options, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error while evaluating flag {FlagKey}", flagKey);
                var errorCode = ex is InvalidCastException ? ErrorType.TypeMismatch : ErrorType.General;
                evaluation = new FlagEvaluationDetails<T>(flagKey, defaultValue, errorCode, Reason.Error, string.Empty);
                await this.TriggerErrorHooksAsync(allHooksReversed, hookContext, ex, options, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await this.TriggerFinallyHooksAsync(allHooksReversed, hookContext, options, cancellationToken).ConfigureAwait(false);
            }

            return evaluation;
        }

        private async ValueTask<HookContext<T>> TriggerBeforeHooksAsync<T>(IReadOnlyList<Hook> hooks, HookContext<T> context,
            FlagEvaluationOptions options, CancellationToken cancellationToken = default)
        {
            var evalContextBuilder = EvaluationContext.Builder();
            evalContextBuilder.Merge(context.EvaluationContext);

            foreach (var hook in hooks)
            {
                var resp = await hook.BeforeAsync(context, options?.HookHints, cancellationToken).ConfigureAwait(false);
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

        private async ValueTask TriggerAfterHooksAsync<T>(IReadOnlyList<Hook> hooks, HookContext<T> context,
            FlagEvaluationDetails<T> evaluationDetails, FlagEvaluationOptions options, CancellationToken cancellationToken = default)
        {
            foreach (var hook in hooks)
            {
                await hook.AfterAsync(context, evaluationDetails, options?.HookHints, cancellationToken).ConfigureAwait(false);
            }
        }

        private async ValueTask TriggerErrorHooksAsync<T>(IReadOnlyList<Hook> hooks, HookContext<T> context, Exception exception,
            FlagEvaluationOptions options, CancellationToken cancellationToken = default)
        {
            foreach (var hook in hooks)
            {
                try
                {
                    await hook.ErrorAsync(context, exception, options?.HookHints, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    this._logger.LogError(e, "Error while executing Error hook {0}", hook.GetType().Name);
                }
            }
        }

        private async ValueTask TriggerFinallyHooksAsync<T>(IReadOnlyList<Hook> hooks, HookContext<T> context,
            FlagEvaluationOptions options, CancellationToken cancellationToken = default)
        {
            foreach (var hook in hooks)
            {
                try
                {
                    await hook.FinallyAsync(context, options?.HookHints, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    this._logger.LogError(e, "Error while executing Finally hook {0}", hook.GetType().Name);
                }
            }
        }
    }
}
