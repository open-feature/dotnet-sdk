using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.Models;
using OpenFeature.Providers.MultiProvider.Strategies;
using OpenFeature.Providers.MultiProvider.Strategies.Models;

namespace OpenFeature.Providers.MultiProvider;

/// <summary>
/// A feature provider that enables the use of multiple underlying providers, allowing different providers
/// to be used for different flag keys or based on specific routing logic.
/// </summary>
/// <remarks>
/// The MultiProvider acts as a composite provider that can delegate flag resolution to different
/// underlying providers based on configuration or routing rules. This enables scenarios where
/// different feature flags may be served by different sources or providers within the same application.
/// </remarks>
/// <seealso href="https://openfeature.dev/specification/appendix-a/#multi-provider">Multi Provider specification</seealso>
public sealed partial class MultiProvider : FeatureProvider, IAsyncDisposable
{
    private readonly BaseEvaluationStrategy _evaluationStrategy;
    private readonly IReadOnlyList<RegisteredProvider> _registeredProviders;
    private readonly Metadata _metadata;

    private readonly SemaphoreSlim _initializationSemaphore = new(1, 1);
    private readonly SemaphoreSlim _shutdownSemaphore = new(1, 1);
    private readonly object _providerStatusLock = new();
    private ProviderStatus _providerStatus = ProviderStatus.NotReady;
    // 0 = Not disposed, 1 = Disposed
    // This is to handle the dispose pattern correctly with the async initialization and shutdown methods
    private volatile int _disposed;

    // Event handling infrastructure
    private readonly ConcurrentDictionary<FeatureProvider, Task> _eventListeningTasks = new();
    private readonly CancellationTokenSource _eventProcessingCancellation = new();
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiProvider"/> class with the specified provider entries and evaluation strategy.
    /// </summary>
    /// <param name="providerEntries">A collection of provider entries containing the feature providers and their optional names.</param>
    /// <param name="evaluationStrategy">The base evaluation strategy to use for determining how to evaluate features across multiple providers. If not specified, the first matching strategy will be used.</param>
    /// <param name="logger">The logger for the client.</param>
    public MultiProvider(IEnumerable<ProviderEntry> providerEntries, BaseEvaluationStrategy? evaluationStrategy = null, ILogger? logger = null)
    {
        if (providerEntries == null)
        {
            throw new ArgumentNullException(nameof(providerEntries));
        }

        var entries = providerEntries.ToList();
        if (entries.Count == 0)
        {
            throw new ArgumentException("At least one provider entry must be provided.", nameof(providerEntries));
        }

        this._evaluationStrategy = evaluationStrategy ?? new FirstMatchStrategy();
        this._registeredProviders = RegisterProviders(entries);

        // Create aggregate metadata
        this._metadata = new Metadata(MultiProviderConstants.ProviderName);

        // Start listening to events from all registered providers
        this.StartListeningToProviderEvents();

        // Set logger
        this._logger = logger ?? NullLogger<MultiProvider>.Instance;
    }

    /// <inheritdoc/>
    public override Metadata GetMetadata() => this._metadata;

    /// <inheritdoc/>
    internal override ProviderStatus Status
    {
        get
        {
            lock (this._providerStatusLock)
            {
                return this._providerStatus;
            }
        }
        set
        {
            lock (this._providerStatusLock)
            {
                this._providerStatus = value;
            }
        }
    }

    /// <inheritdoc/>
    public override Task<ResolutionDetails<bool>> ResolveBooleanValueAsync(string flagKey, bool defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default) =>
        this.EvaluateAsync(flagKey, defaultValue, context, cancellationToken);

    /// <inheritdoc/>
    public override Task<ResolutionDetails<double>> ResolveDoubleValueAsync(string flagKey, double defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default) =>
        this.EvaluateAsync(flagKey, defaultValue, context, cancellationToken);

    /// <inheritdoc/>
    public override Task<ResolutionDetails<int>> ResolveIntegerValueAsync(string flagKey, int defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default) =>
        this.EvaluateAsync(flagKey, defaultValue, context, cancellationToken);

    /// <inheritdoc/>
    public override Task<ResolutionDetails<string>> ResolveStringValueAsync(string flagKey, string defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default) =>
        this.EvaluateAsync(flagKey, defaultValue, context, cancellationToken);

    /// <inheritdoc/>
    public override Task<ResolutionDetails<Value>> ResolveStructureValueAsync(string flagKey, Value defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default) =>
        this.EvaluateAsync(flagKey, defaultValue, context, cancellationToken);

    /// <inheritdoc/>
    public override async Task InitializeAsync(EvaluationContext context, CancellationToken cancellationToken = default)
    {
        if (this._disposed == 1)
        {
            throw new ObjectDisposedException(nameof(MultiProvider));
        }

        await this._initializationSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (this.Status != ProviderStatus.NotReady || this._disposed == 1)
            {
                return;
            }

            var initializationTasks = this._registeredProviders.Select(async rp =>
            {
                try
                {
                    await rp.Provider.InitializeAsync(context, cancellationToken).ConfigureAwait(false);
                    rp.SetStatus(ProviderStatus.Ready);
                    return new ChildProviderStatus { ProviderName = rp.Name };
                }
                catch (Exception ex)
                {
                    rp.SetStatus(ProviderStatus.Fatal);
                    return new ChildProviderStatus { ProviderName = rp.Name, Error = ex };
                }
            });

            var results = await Task.WhenAll(initializationTasks).ConfigureAwait(false);
            var failures = results.Where(r => r.Error != null).ToList();

            if (failures.Count != 0)
            {
                var exceptions = failures.Select(f => f.Error!).ToList();
                var failedProviders = failures.Select(f => f.ProviderName).ToList();
                this.Status = ProviderStatus.Fatal;

                // Emit error event
                await this.EmitEvent(new ProviderEventPayload
                {
                    ProviderName = this._metadata.Name,
                    Type = ProviderEventTypes.ProviderError,
                    Message = $"Failed to initialize providers: {string.Join(", ", failedProviders)}",
                    ErrorType = ErrorType.ProviderFatal
                }, cancellationToken).ConfigureAwait(false);

                throw new AggregateException(
                    $"Failed to initialize providers: {string.Join(", ", failedProviders)}",
                    exceptions);
            }
            else
            {
                this.Status = ProviderStatus.Ready;

                // Emit ready event
                await this.EmitEvent(new ProviderEventPayload
                {
                    ProviderName = this._metadata.Name,
                    Type = ProviderEventTypes.ProviderReady,
                    Message = "MultiProvider successfully initialized"
                }, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            this._initializationSemaphore.Release();
        }
    }

    /// <inheritdoc/>
    public override async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        if (this._disposed == 1)
        {
            throw new ObjectDisposedException(nameof(MultiProvider));
        }

        await this.InternalShutdownAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<ResolutionDetails<T>> EvaluateAsync<T>(string key, T defaultValue, EvaluationContext? evaluationContext = null, CancellationToken cancellationToken = default)
    {
        // Check if the provider has been disposed
        // This is to handle the dispose pattern correctly with the async initialization and shutdown methods
        // It is checked here to avoid the check in every public EvaluateAsync method
        if (this._disposed == 1)
        {
            throw new ObjectDisposedException(nameof(MultiProvider));
        }

        try
        {
            var strategyContext = new StrategyEvaluationContext<T>(key);
            var resolutions = this._evaluationStrategy.RunMode switch
            {
                RunMode.Parallel => await this
                    .ParallelEvaluationAsync(key, defaultValue, evaluationContext, cancellationToken)
                    .ConfigureAwait(false),
                RunMode.Sequential => await this
                    .SequentialEvaluationAsync(key, defaultValue, evaluationContext, cancellationToken)
                    .ConfigureAwait(false),
                _ => throw new NotSupportedException($"Unsupported run mode: {this._evaluationStrategy.RunMode}")
            };

            var finalResult = this._evaluationStrategy.DetermineFinalResult(strategyContext, key, defaultValue,
                evaluationContext, resolutions);
            return finalResult.Details;
        }
        catch (NotSupportedException ex)
        {
            // Emit error event for unsupported run mode
            await this.EmitEvent(new ProviderEventPayload
            {
                ProviderName = this._metadata.Name,
                Type = ProviderEventTypes.ProviderError,
                Message = $"Error evaluating flag '{key}' with run mode {this._evaluationStrategy.RunMode}",
                ErrorType = ErrorType.ProviderFatal
            }, cancellationToken).ConfigureAwait(false);

            return new ResolutionDetails<T>(key, defaultValue, ErrorType.ProviderFatal, Reason.Error, errorMessage: ex.Message);
        }
        catch (Exception ex)
        {
            // Emit error event for evaluation failures
            await this.EmitEvent(new ProviderEventPayload
            {
                ProviderName = this._metadata.Name,
                Type = ProviderEventTypes.ProviderError,
                Message = $"Error evaluating flag '{key}': {ex.Message}",
                ErrorType = ErrorType.General,
                FlagsChanged = [key]
            }, cancellationToken).ConfigureAwait(false);

            return new ResolutionDetails<T>(key, defaultValue, ErrorType.General, Reason.Error, errorMessage: ex.Message);
        }
    }

    private async Task<List<ProviderResolutionResult<T>>> SequentialEvaluationAsync<T>(string key, T defaultValue, EvaluationContext? evaluationContext, CancellationToken cancellationToken)
    {
        var resolutions = new List<ProviderResolutionResult<T>>();

        foreach (var registeredProvider in this._registeredProviders)
        {
            var providerContext = new StrategyPerProviderContext<T>(
                registeredProvider.Provider,
                registeredProvider.Name,
                registeredProvider.Status,
                key);

            if (!this._evaluationStrategy.ShouldEvaluateThisProvider(providerContext, evaluationContext))
            {
                continue;
            }

            var result = await registeredProvider.Provider.EvaluateAsync(providerContext, evaluationContext, defaultValue, this._logger, cancellationToken).ConfigureAwait(false);
            resolutions.Add(result);

            if (!this._evaluationStrategy.ShouldEvaluateNextProvider(providerContext, evaluationContext, result))
            {
                break;
            }
        }

        return resolutions;
    }

    private async Task<List<ProviderResolutionResult<T>>> ParallelEvaluationAsync<T>(string key, T defaultValue, EvaluationContext? evaluationContext, CancellationToken cancellationToken)
    {
        var resolutions = new List<ProviderResolutionResult<T>>();
        var tasks = new List<Task<ProviderResolutionResult<T>>>();

        foreach (var registeredProvider in this._registeredProviders)
        {
            var providerContext = new StrategyPerProviderContext<T>(
                registeredProvider.Provider,
                registeredProvider.Name,
                registeredProvider.Status,
                key);

            if (this._evaluationStrategy.ShouldEvaluateThisProvider(providerContext, evaluationContext))
            {
                tasks.Add(registeredProvider.Provider.EvaluateAsync(providerContext, evaluationContext, defaultValue, this._logger, cancellationToken));
            }
        }

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        resolutions.AddRange(results);

        return resolutions;
    }

    private void StartListeningToProviderEvents()
    {
        foreach (var registeredProvider in this._registeredProviders)
        {
            if (!this._eventListeningTasks.TryAdd(registeredProvider.Provider, this.ProcessProviderEventsAsync(registeredProvider)))
            {
                // Log a warning if the provider is already being listened to
                this.LogProviderAlreadyBeingListenedTo(registeredProvider.Name);
            }
        }
    }

    private async Task ProcessProviderEventsAsync(RegisteredProvider registeredProvider)
    {
        var eventChannel = registeredProvider.Provider.GetEventChannel();

        // Get the cancellation token safely for this provider's event processing (this prevents ObjectDisposedException during concurrent shutdown)
        CancellationToken cancellationToken;
        try
        {
            cancellationToken = this._eventProcessingCancellation.Token;
        }
        catch (ObjectDisposedException)
        {
            // Already disposed, exit early
            return;
        }

        while (await eventChannel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
        {
            while (eventChannel.Reader.TryRead(out var item))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (item is not Event { EventPayload: { } eventPayload })
                {
                    continue;
                }

                await this.HandleProviderEventAsync(registeredProvider, eventPayload, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task HandleProviderEventAsync(RegisteredProvider registeredProvider, ProviderEventPayload eventPayload, CancellationToken cancellationToken = default)
    {
        try
        {
            // Handle PROVIDER_CONFIGURATION_CHANGED events specially - these are always re-emitted
            if (eventPayload.Type == ProviderEventTypes.ProviderConfigurationChanged)
            {
                await this.EmitEvent(new ProviderEventPayload
                {
                    ProviderName = $"{this._metadata.Name}/{registeredProvider.Name}",
                    Type = eventPayload.Type,
                    Message = eventPayload.Message ?? $"Configuration changed in provider {registeredProvider.Name}",
                    FlagsChanged = eventPayload.FlagsChanged,
                    EventMetadata = eventPayload.EventMetadata
                }, cancellationToken).ConfigureAwait(false);
                return;
            }

            // For status-changing events, update provider status and check if MultiProvider status should change
            UpdateProviderStatusFromEvent(registeredProvider, eventPayload);

            // Check if MultiProvider status has changed due to this provider's status change
            var providerStatuses = this._registeredProviders.Select(rp => rp.Status).ToList();
            var newMultiProviderStatus = DetermineAggregateStatus(providerStatuses);

            ProviderStatus previousStatus;
            ProviderEventTypes? eventType = null;

            // Only emit event if MultiProvider status actually changed
            lock (this._providerStatusLock)
            {
                if (newMultiProviderStatus != this._providerStatus)
                {
                    previousStatus = this._providerStatus;
                    this._providerStatus = newMultiProviderStatus;

                    eventType = newMultiProviderStatus switch
                    {
                        ProviderStatus.Ready => ProviderEventTypes.ProviderReady,
                        ProviderStatus.Error or ProviderStatus.Fatal => ProviderEventTypes.ProviderError,
                        ProviderStatus.Stale => ProviderEventTypes.ProviderStale,
                        _ => (ProviderEventTypes?)null
                    };
                }
                else
                {
                    return; // No status change, no event to emit
                }
            }

            if (eventType.HasValue)
            {
                await this.EmitEvent(new ProviderEventPayload
                {
                    ProviderName = this._metadata.Name,
                    Type = eventType.Value,
                    Message = $"MultiProvider status changed from {previousStatus} to {newMultiProviderStatus} due to provider {registeredProvider.Name}",
                    ErrorType = newMultiProviderStatus == ProviderStatus.Fatal ? ErrorType.ProviderFatal : eventPayload.ErrorType,
                    FlagsChanged = eventPayload.FlagsChanged,
                    EventMetadata = eventPayload.EventMetadata
                }, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            // If there's an error processing the event, emit an error event
            await this.EmitEvent(new ProviderEventPayload
            {
                ProviderName = this._metadata.Name,
                Type = ProviderEventTypes.ProviderError,
                Message = $"Error processing event from provider {registeredProvider.Name}: {ex.Message}",
                ErrorType = ErrorType.General
            }, cancellationToken).ConfigureAwait(false);
        }
    }

    private static void UpdateProviderStatusFromEvent(RegisteredProvider registeredProvider, ProviderEventPayload eventPayload)
    {
        var newStatus = eventPayload.Type switch
        {
            ProviderEventTypes.ProviderReady => ProviderStatus.Ready,
            ProviderEventTypes.ProviderError => eventPayload.ErrorType == ErrorType.ProviderFatal
                ? ProviderStatus.Fatal
                : ProviderStatus.Error,
            ProviderEventTypes.ProviderStale => ProviderStatus.Stale,
            _ => registeredProvider.Status // No status change for PROVIDER_CONFIGURATION_CHANGED
        };

        if (newStatus != registeredProvider.Status)
        {
            registeredProvider.SetStatus(newStatus);
        }
    }

    private async Task EmitEvent(ProviderEventPayload eventPayload, CancellationToken cancellationToken)
    {
        try
        {
            await this.EventChannel.Writer.WriteAsync(eventPayload, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // If we can't write to the event channel (e.g., it's closed), ignore the error
        }
    }

    private static ProviderStatus DetermineAggregateStatus(List<ProviderStatus> providerStatuses)
    {
        // Check in precedence order as per specification
        if (providerStatuses.Any(status => status == ProviderStatus.Fatal))
        {
            return ProviderStatus.Fatal;
        }

        if (providerStatuses.Any(status => status == ProviderStatus.NotReady))
        {
            return ProviderStatus.NotReady;
        }

        if (providerStatuses.Any(status => status == ProviderStatus.Error))
        {
            return ProviderStatus.Error;
        }

        if (providerStatuses.Any(status => status == ProviderStatus.Stale))
        {
            return ProviderStatus.Stale;
        }

        return providerStatuses.All(status => status == ProviderStatus.Ready) ? ProviderStatus.Ready :
            // Default to NotReady if we have mixed statuses not covered above
            ProviderStatus.NotReady;
    }

    private static ReadOnlyCollection<RegisteredProvider> RegisterProviders(IEnumerable<ProviderEntry> providerEntries)
    {
        var entries = providerEntries.ToList();
        var registeredProviders = new List<RegisteredProvider>();
        var nameGroups = entries.GroupBy(e => e.Name ?? e.Provider.GetMetadata()?.Name ?? "UnknownProvider").ToList();

        // Check for duplicate explicit names
        var duplicateExplicitNames = nameGroups
            .FirstOrDefault(g => g.Count(e => e.Name != null) > 1)?.Key;

        if (duplicateExplicitNames != null)
        {
            throw new ArgumentException($"Multiple providers cannot have the same explicit name: '{duplicateExplicitNames}'");
        }

        // Assign unique names
        foreach (var group in nameGroups)
        {
            var baseName = group.Key;
            var groupEntries = group.ToList();

            if (groupEntries.Count == 1)
            {
                var entry = groupEntries[0];
                registeredProviders.Add(new RegisteredProvider(entry.Provider, entry.Name ?? baseName));
            }
            else
            {
                // Multiple providers with same metadata name - add indices
                var index = 1;
                foreach (var entry in groupEntries)
                {
                    var finalName = entry.Name ?? $"{baseName}-{index++}";
                    registeredProviders.Add(new RegisteredProvider(entry.Provider, finalName));
                }
            }
        }

        return registeredProviders.AsReadOnly();
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref this._disposed, 1) == 1)
        {
            // Already disposed
            return;
        }

        try
        {
            await this.InternalShutdownAsync(CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            this._initializationSemaphore.Dispose();
            this._shutdownSemaphore.Dispose();
            this._providerStatus = ProviderStatus.Fatal;
            this._eventProcessingCancellation.Dispose();
        }
    }

    private async Task ShutdownEventProcessingAsync()
    {
        // Cancel event processing - protect against ObjectDisposedException during concurrent shutdown
        try
        {
            this._eventProcessingCancellation.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Expected if already disposed during concurrent shutdown
        }

        // Wait for all event listening tasks to complete, ignoring cancellation exceptions
        if (this._eventListeningTasks.Count != 0)
        {
            try
            {
                await Task.WhenAll(this._eventListeningTasks.Values).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected when shutting down
            }
        }
    }

    private async Task InternalShutdownAsync(CancellationToken cancellationToken)
    {
        await this.ShutdownEventProcessingAsync().ConfigureAwait(false);
        await this._shutdownSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // We should be able to shut down the provider when it is in Ready or Fatal status.
            if ((this.Status != ProviderStatus.Ready && this.Status != ProviderStatus.Fatal) || this._disposed == 1)
            {
                return;
            }

            var shutdownTasks = this._registeredProviders.Select(async rp =>
            {
                try
                {
                    await rp.Provider.ShutdownAsync(cancellationToken).ConfigureAwait(false);
                    rp.SetStatus(ProviderStatus.NotReady);
                    return new ChildProviderStatus { ProviderName = rp.Name };
                }
                catch (Exception ex)
                {
                    rp.SetStatus(ProviderStatus.Fatal);
                    return new ChildProviderStatus { ProviderName = rp.Name, Error = ex };
                }
            });

            var results = await Task.WhenAll(shutdownTasks).ConfigureAwait(false);
            var failures = results.Where(r => r.Error != null).ToList();

            if (failures.Count != 0)
            {
                var exceptions = failures.Select(f => f.Error!).ToList();
                var failedProviders = failures.Select(f => f.ProviderName).ToList();
                throw new AggregateException(
                    $"Failed to shutdown providers: {string.Join(", ", failedProviders)}",
                    exceptions);
            }

            this.Status = ProviderStatus.NotReady;
            this._eventListeningTasks.Clear();
        }
        finally
        {
            this._shutdownSemaphore.Release();
        }
    }

    /// <summary>
    /// This should only be used for testing purposes.
    /// </summary>
    /// <param name="providerStatus">The status to set.</param>
    internal void SetStatus(ProviderStatus providerStatus)
    {
        this.Status = providerStatus;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Provider {ProviderName} is already being listened to")]
    private partial void LogProviderAlreadyBeingListenedTo(string providerName);
}
