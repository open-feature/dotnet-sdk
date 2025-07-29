using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenFeature.Constant;
using OpenFeature.Error;
using OpenFeature.Extension;
using OpenFeature.Model;

namespace OpenFeature;

/// <summary>
/// OpenFeature Client implementation for resolving feature flags and tracking user interactions.
/// </summary>
public sealed partial class FeatureClient : IFeatureClient
{
    private readonly ClientMetadata _metadata;
    private readonly ConcurrentStack<Hook> _hooks = new ConcurrentStack<Hook>();
    private readonly ILogger _logger;
    private readonly Func<FeatureProvider> _providerAccessor;
    private readonly Api _api;
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
        var provider = this._api.GetProvider(this._metadata.Name!);

        return (method(provider), provider);
    }

    /// <inheritdoc />
    public ProviderStatus ProviderStatus => this._providerAccessor.Invoke().Status;

    /// <inheritdoc />
    public EvaluationContext GetContext()
    {
        lock (this._evaluationContextLock)
        {
            return this._evaluationContext;
        }
    }

    /// <inheritdoc />
    public void SetContext(EvaluationContext? context)
    {
        lock (this._evaluationContextLock)
        {
            this._evaluationContext = context ?? EvaluationContext.Empty;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureClient"/> class.
    /// </summary>
    /// <param name="providerAccessor">Function to retrieve current provider</param>
    /// <param name="name">Name of client <see cref="ClientMetadata"/></param>
    /// <param name="version">Version of client <see cref="ClientMetadata"/></param>
    /// <param name="api">The API instance for accessing global state and providers</param>
    /// <param name="logger">Logger used by client</param>
    /// <param name="context">Context given to this client</param>
    /// <exception cref="ArgumentNullException">Throws if any of the required parameters are null</exception>
    internal FeatureClient(Func<FeatureProvider> providerAccessor, string? name, string? version, Api? api = null, ILogger? logger = null, EvaluationContext? context = null)
    {
        this._metadata = new ClientMetadata(name, version);
        this._logger = logger ?? NullLogger<FeatureClient>.Instance;
        this._evaluationContext = context ?? EvaluationContext.Empty;
        this._providerAccessor = providerAccessor;
        this._api = api ?? Api.Instance;
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
        this._api.AddClientHandler(this._metadata.Name!, eventType, handler);
    }

    /// <inheritdoc />
    public void RemoveHandler(ProviderEventTypes type, EventHandlerDelegate handler)
    {
        this._api.RemoveClientHandler(this._metadata.Name!, type, handler);
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
    public async Task<bool> GetBooleanValueAsync(string flagKey, bool defaultValue, EvaluationContext? context = null,
        FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default) =>
        (await this.GetBooleanDetailsAsync(flagKey, defaultValue, context, config, cancellationToken).ConfigureAwait(false)).Value;

    /// <inheritdoc />
    public async Task<FlagEvaluationDetails<bool>> GetBooleanDetailsAsync(string flagKey, bool defaultValue,
        EvaluationContext? context = null, FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default) =>
        await this.EvaluateFlagAsync(this.ExtractProvider<bool>(provider => provider.ResolveBooleanValueAsync),
            FlagValueType.Boolean, flagKey,
            defaultValue, context, config, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<string> GetStringValueAsync(string flagKey, string defaultValue, EvaluationContext? context = null,
        FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default) =>
        (await this.GetStringDetailsAsync(flagKey, defaultValue, context, config, cancellationToken).ConfigureAwait(false)).Value;

    /// <inheritdoc />
    public async Task<FlagEvaluationDetails<string>> GetStringDetailsAsync(string flagKey, string defaultValue,
        EvaluationContext? context = null, FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default) =>
        await this.EvaluateFlagAsync(this.ExtractProvider<string>(provider => provider.ResolveStringValueAsync),
            FlagValueType.String, flagKey,
            defaultValue, context, config, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<int> GetIntegerValueAsync(string flagKey, int defaultValue, EvaluationContext? context = null,
        FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default) =>
        (await this.GetIntegerDetailsAsync(flagKey, defaultValue, context, config, cancellationToken).ConfigureAwait(false)).Value;

    /// <inheritdoc />
    public async Task<FlagEvaluationDetails<int>> GetIntegerDetailsAsync(string flagKey, int defaultValue,
        EvaluationContext? context = null, FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default) =>
        await this.EvaluateFlagAsync(this.ExtractProvider<int>(provider => provider.ResolveIntegerValueAsync),
            FlagValueType.Number, flagKey,
            defaultValue, context, config, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<double> GetDoubleValueAsync(string flagKey, double defaultValue,
        EvaluationContext? context = null,
        FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default) =>
        (await this.GetDoubleDetailsAsync(flagKey, defaultValue, context, config, cancellationToken).ConfigureAwait(false)).Value;

    /// <inheritdoc />
    public async Task<FlagEvaluationDetails<double>> GetDoubleDetailsAsync(string flagKey, double defaultValue,
        EvaluationContext? context = null, FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default) =>
        await this.EvaluateFlagAsync(this.ExtractProvider<double>(provider => provider.ResolveDoubleValueAsync),
            FlagValueType.Number, flagKey,
            defaultValue, context, config, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<Value> GetObjectValueAsync(string flagKey, Value defaultValue, EvaluationContext? context = null,
        FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default) =>
        (await this.GetObjectDetailsAsync(flagKey, defaultValue, context, config, cancellationToken).ConfigureAwait(false)).Value;

    /// <inheritdoc />
    public async Task<FlagEvaluationDetails<Value>> GetObjectDetailsAsync(string flagKey, Value defaultValue,
        EvaluationContext? context = null, FlagEvaluationOptions? config = null, CancellationToken cancellationToken = default) =>
        await this.EvaluateFlagAsync(this.ExtractProvider<Value>(provider => provider.ResolveStructureValueAsync),
            FlagValueType.Object, flagKey,
            defaultValue, context, config, cancellationToken).ConfigureAwait(false);

    private async Task<FlagEvaluationDetails<T>> EvaluateFlagAsync<T>(
        (Func<string, T, EvaluationContext, CancellationToken, Task<ResolutionDetails<T>>>, FeatureProvider) providerInfo,
        FlagValueType flagValueType, string flagKey, T defaultValue, EvaluationContext? context = null,
        FlagEvaluationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var resolveValueDelegate = providerInfo.Item1;
        var provider = providerInfo.Item2;

        // New up an evaluation context if one was not provided.
        context ??= EvaluationContext.Empty;

        // merge api, client, transaction and invocation context
        var evaluationContextBuilder = EvaluationContext.Builder();
        evaluationContextBuilder.Merge(this._api.GetContext()); // API context
        evaluationContextBuilder.Merge(this.GetContext()); // Client context
        evaluationContextBuilder.Merge(this._api.GetTransactionContext()); // Transaction context
        evaluationContextBuilder.Merge(context); // Invocation context

        var allHooks = ImmutableList.CreateBuilder<Hook>()
            .Concat(this._api.GetHooks())
            .Concat(this.GetHooks())
            .Concat(options?.Hooks ?? Enumerable.Empty<Hook>())
            .Concat(provider.GetProviderHooks())
            .ToImmutableList();

        var sharedHookContext = new SharedHookContext<T>(
            flagKey,
            defaultValue,
            flagValueType,
            this._metadata,
            provider.GetMetadata()
        );

        FlagEvaluationDetails<T>? evaluation = null;
        var hookRunner = new HookRunner<T>(allHooks, evaluationContextBuilder.Build(), sharedHookContext,
            this._logger);

        try
        {
            var evaluationContextFromHooks = await hookRunner.TriggerBeforeHooksAsync(options?.HookHints, cancellationToken)
                .ConfigureAwait(false);

            // short circuit evaluation entirely if provider is in a bad state
            if (provider.Status == ProviderStatus.NotReady)
            {
                throw new ProviderNotReadyException("Provider has not yet completed initialization.");
            }
            else if (provider.Status == ProviderStatus.Fatal)
            {
                throw new ProviderFatalException("Provider is in an irrecoverable error state.");
            }

            evaluation =
                (await resolveValueDelegate
                    .Invoke(flagKey, defaultValue, evaluationContextFromHooks, cancellationToken)
                    .ConfigureAwait(false))
                .ToFlagEvaluationDetails();

            if (evaluation.ErrorType == ErrorType.None)
            {
                await hookRunner.TriggerAfterHooksAsync(
                    evaluation,
                    options?.HookHints,
                    cancellationToken
                ).ConfigureAwait(false);
            }
            else
            {
                var exception = new FeatureProviderException(evaluation.ErrorType, evaluation.ErrorMessage);
                this.FlagEvaluationErrorWithDescription(flagKey, evaluation.ErrorType.GetDescription(), exception);
                await hookRunner.TriggerErrorHooksAsync(exception, options?.HookHints, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (FeatureProviderException ex)
        {
            this.FlagEvaluationErrorWithDescription(flagKey, ex.ErrorType.GetDescription(), ex);
            evaluation = new FlagEvaluationDetails<T>(flagKey, defaultValue, ex.ErrorType, Reason.Error,
                string.Empty, ex.Message);
            await hookRunner.TriggerErrorHooksAsync(ex, options?.HookHints, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var errorCode = ex is InvalidCastException ? ErrorType.TypeMismatch : ErrorType.General;
            evaluation = new FlagEvaluationDetails<T>(flagKey, defaultValue, errorCode, Reason.Error, string.Empty,
                ex.Message);
            await hookRunner.TriggerErrorHooksAsync(ex, options?.HookHints, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            evaluation ??= new FlagEvaluationDetails<T>(flagKey, defaultValue, ErrorType.General, Reason.Error,
                string.Empty,
                "Evaluation failed to return a result.");
            await hookRunner.TriggerFinallyHooksAsync(evaluation, options?.HookHints, cancellationToken)
                .ConfigureAwait(false);
        }

        return evaluation;
    }

    /// <inheritdoc />
    public void Track(string trackingEventName, EvaluationContext? evaluationContext = default, TrackingEventDetails? trackingEventDetails = default)
    {
        if (string.IsNullOrWhiteSpace(trackingEventName))
        {
            throw new ArgumentException("Tracking event cannot be null or empty.", nameof(trackingEventName));
        }

        var globalContext = this._api.GetContext();
        var clientContext = this.GetContext();

        var evaluationContextBuilder = EvaluationContext.Builder()
            .Merge(globalContext)
            .Merge(clientContext);
        if (evaluationContext != null) evaluationContextBuilder.Merge(evaluationContext);

        this._providerAccessor.Invoke().Track(trackingEventName, evaluationContextBuilder.Build(), trackingEventDetails);
    }

    [LoggerMessage(101, LogLevel.Error, "Error while evaluating flag {FlagKey}")]
    partial void FlagEvaluationError(string flagKey, Exception exception);

    [LoggerMessage(100, LogLevel.Debug, "Hook {HookName} returned null, nothing to merge back into context")]
    partial void HookReturnedNull(string hookName);

    [LoggerMessage(102, LogLevel.Error, "Error while evaluating flag {FlagKey}: {ErrorType}")]
    partial void FlagEvaluationErrorWithDescription(string flagKey, string errorType, Exception exception);
}
