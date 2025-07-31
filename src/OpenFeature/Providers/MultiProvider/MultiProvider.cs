using System.Collections.ObjectModel;
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
public sealed class MultiProvider : FeatureProvider, IDisposable
{
    private readonly BaseEvaluationStrategy _evaluationStrategy;
    private readonly IReadOnlyList<RegisteredProvider> _registeredProviders;
    private readonly Metadata _metadata;

    private readonly SemaphoreSlim _initializationSemaphore = new(1, 1);
    private readonly SemaphoreSlim _shutdownSemaphore = new(1, 1);
    private ProviderStatus _providerStatus = ProviderStatus.NotReady;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiProvider"/> class with the specified provider entries and evaluation strategy.
    /// </summary>
    /// <param name="providerEntries">A collection of provider entries containing the feature providers and their optional names.</param>
    /// <param name="evaluationStrategy">The base evaluation strategy to use for determining how to evaluate features across multiple providers. If not specified, the first matching strategy will be used.</param>
    public MultiProvider(IEnumerable<ProviderEntry> providerEntries, BaseEvaluationStrategy? evaluationStrategy = null)
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
    }

    /// <inheritdoc/>
    public override Metadata GetMetadata() => this._metadata;

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
        if (this._disposed)
        {
            throw new ObjectDisposedException(nameof(MultiProvider));
        }

        await this._initializationSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (this._providerStatus != ProviderStatus.NotReady || this._disposed)
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
                this._providerStatus = ProviderStatus.Fatal;
                throw new AggregateException(
                    $"Failed to initialize providers: {string.Join(", ", failedProviders)}",
                    exceptions);
            }
            else
            {
                this._providerStatus = ProviderStatus.Ready;
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
        if (this._disposed)
        {
            throw new ObjectDisposedException(nameof(MultiProvider));
        }

        await this._shutdownSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // We should be able to shutdown the provider when it is in Ready or Fatal status.
            if ((this._providerStatus != ProviderStatus.Ready && this._providerStatus != ProviderStatus.Fatal) || this._disposed)
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

            this._providerStatus = ProviderStatus.NotReady;
        }
        finally
        {
            this._shutdownSemaphore.Release();
        }
    }

    private async Task<ResolutionDetails<T>> EvaluateAsync<T>(string key, T defaultValue, EvaluationContext? evaluationContext = null, CancellationToken cancellationToken = default)
    {
        var strategyContext = new StrategyEvaluationContext<T>(key);
        var resolutions = this._evaluationStrategy.RunMode switch
        {
            RunMode.Parallel => await this.ParallelEvaluationAsync(key, defaultValue, evaluationContext, cancellationToken).ConfigureAwait(false),
            RunMode.Sequential => await this.SequentialEvaluationAsync(key, defaultValue, evaluationContext, cancellationToken).ConfigureAwait(false),
            _ => throw new NotSupportedException($"Unsupported run mode: {this._evaluationStrategy.RunMode}")
        };

        var finalResult = this._evaluationStrategy.DetermineFinalResult(strategyContext, key, defaultValue, evaluationContext, resolutions);
        return finalResult.Details;
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

            var result = await registeredProvider.Provider.EvaluateAsync(providerContext, evaluationContext, defaultValue, cancellationToken).ConfigureAwait(false);
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
                tasks.Add(registeredProvider.Provider.EvaluateAsync(providerContext, evaluationContext, defaultValue, cancellationToken));
            }
        }

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        resolutions.AddRange(results);

        return resolutions;
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
    public void Dispose()
    {
        if (!this._disposed)
        {
            this._initializationSemaphore.Dispose();
            this._shutdownSemaphore.Dispose();
            this._disposed = true;
        }
    }

    /// <summary>
    /// This should only be used for testing purposes.
    /// </summary>
    /// <param name="providerStatus">The status to set.</param>
    internal void SetStatus(ProviderStatus providerStatus)
    {
        this._providerStatus = providerStatus;
    }
}
