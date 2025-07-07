using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature;

/// <summary>
/// This class manages the collection of providers, both default and named, contained by the API.
/// </summary>
internal sealed partial class ProviderRepository : IAsyncDisposable
{
    private ILogger _logger = NullLogger<ProviderRepository>.Instance;

    private FeatureProvider _defaultProvider = new NoOpFeatureProvider();

    private readonly ConcurrentDictionary<string, FeatureProvider> _featureProviders = new();

    /// The reader/writer locks is not disposed because the singleton instance should never be disposed.
    ///
    /// Mutations of the _defaultProvider or _featureProviders are done within this lock even though
    /// _featureProvider is a concurrent collection. This is for a couple of reasons, the first is that
    /// a provider should only be shutdown if it is not in use, and it could be in use as either a named or
    /// default provider.
    ///
    /// The second is that a concurrent collection doesn't provide any ordering, so we could check a provider
    /// as it was being added or removed such as two concurrent calls to SetProvider replacing multiple instances
    /// of that provider under different names.
    private readonly ReaderWriterLockSlim _providersLock = new();

    public async ValueTask DisposeAsync()
    {
        using (this._providersLock)
        {
            await this.ShutdownAsync().ConfigureAwait(false);
        }
    }

    internal void SetLogger(ILogger logger) => this._logger = logger;

    /// <summary>
    /// Set the default provider
    /// </summary>
    /// <param name="featureProvider">the provider to set as the default, passing null has no effect</param>
    /// <param name="context">the context to initialize the provider with</param>
    /// <param name="afterInitSuccess">
    /// called after the provider has initialized successfully, only called if the provider needed initialization
    /// </param>
    /// <param name="afterInitError">
    /// called if an error happens during the initialization of the provider, only called if the provider needed
    /// initialization
    /// </param>
    /// <param name="cancellationToken">a cancellation token to cancel the operation</param>
    internal async Task SetProviderAsync(
        FeatureProvider? featureProvider,
        EvaluationContext context,
        Func<FeatureProvider, Task>? afterInitSuccess = null,
        Func<FeatureProvider, Exception, Task>? afterInitError = null,
        CancellationToken cancellationToken = default)
    {
        // Cannot unset the feature provider.
        if (featureProvider == null)
        {
            return;
        }

        this._providersLock.EnterWriteLock();
        // Default provider is swapped synchronously, initialization and shutdown may happen asynchronously.
        try
        {
            // Setting the provider to the same provider should not have an effect.
            if (ReferenceEquals(featureProvider, this._defaultProvider))
            {
                return;
            }

            var oldProvider = this._defaultProvider;
            this._defaultProvider = featureProvider;
            // We want to allow shutdown to happen concurrently with initialization, and the caller to not
            // wait for it.
            _ = this.ShutdownIfUnusedAsync(oldProvider, cancellationToken);
        }
        finally
        {
            this._providersLock.ExitWriteLock();
        }

        await InitProviderAsync(this._defaultProvider, context, afterInitSuccess, afterInitError, cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task InitProviderAsync(
        FeatureProvider? newProvider,
        EvaluationContext context,
        Func<FeatureProvider, Task>? afterInitialization,
        Func<FeatureProvider, Exception, Task>? afterError,
        CancellationToken cancellationToken = default)
    {
        if (newProvider == null)
        {
            return;
        }
        if (newProvider.Status == ProviderStatus.NotReady)
        {
            try
            {
                await newProvider.InitializeAsync(context, cancellationToken).ConfigureAwait(false);
                if (afterInitialization != null)
                {
                    await afterInitialization.Invoke(newProvider).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                if (afterError != null)
                {
                    await afterError.Invoke(newProvider, ex).ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Set a named provider
    /// </summary>
    /// <param name="domain">an identifier which logically binds clients with providers</param>
    /// <param name="featureProvider">the provider to set as the default, passing null has no effect</param>
    /// <param name="context">the context to initialize the provider with</param>
    /// <param name="afterInitSuccess">
    /// called after the provider has initialized successfully, only called if the provider needed initialization
    /// </param>
    /// <param name="afterInitError">
    /// called if an error happens during the initialization of the provider, only called if the provider needed
    /// initialization
    /// </param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel any async side effects.</param>
    internal async Task SetProviderAsync(string? domain,
        FeatureProvider? featureProvider,
        EvaluationContext context,
        Func<FeatureProvider, Task>? afterInitSuccess = null,
        Func<FeatureProvider, Exception, Task>? afterInitError = null,
        CancellationToken cancellationToken = default)
    {
        // Cannot set a provider for a null domain.
#if NETFRAMEWORK || NETSTANDARD
        // This is a workaround for the issue in .NET Framework where string.IsNullOrEmpty is not nullable compatible.
        if (domain == null)
        {
            return;
        }
#else
        if (string.IsNullOrWhiteSpace(domain))
        {
            return;
        }
#endif

        this._providersLock.EnterWriteLock();

        try
        {
            this._featureProviders.TryGetValue(domain, out var oldProvider);
            if (featureProvider != null)
            {
                this._featureProviders.AddOrUpdate(domain, featureProvider,
                    (key, current) => featureProvider);
            }
            else
            {
                // If names of clients are programmatic, then setting the provider to null could result
                // in unbounded growth of the collection.
                this._featureProviders.TryRemove(domain, out _);
            }

            // We want to allow shutdown to happen concurrently with initialization, and the caller to not
            // wait for it.
            _ = this.ShutdownIfUnusedAsync(oldProvider, cancellationToken);
        }
        finally
        {
            this._providersLock.ExitWriteLock();
        }

        await InitProviderAsync(featureProvider, context, afterInitSuccess, afterInitError, cancellationToken).ConfigureAwait(false);
    }

    /// <remarks>
    /// Shutdown the feature provider if it is unused. This must be called within a write lock of the _providersLock.
    /// </remarks>
    private async Task ShutdownIfUnusedAsync(
        FeatureProvider? targetProvider, CancellationToken cancellationToken = default)
    {
        if (ReferenceEquals(this._defaultProvider, targetProvider))
        {
            return;
        }

        if (targetProvider != null && this._featureProviders.Values.Contains(targetProvider))
        {
            return;
        }

        await this.SafeShutdownProviderAsync(targetProvider, cancellationToken).ConfigureAwait(false);
    }

    /// <remarks>
    /// <para>
    /// Shut down the provider and capture any exceptions thrown.
    /// </para>
    /// <para>
    /// The provider is set either to a name or default before the old provider it shut down, so
    /// it would not be meaningful to emit an error.
    /// </para>
    /// </remarks>
    private async Task SafeShutdownProviderAsync(FeatureProvider? targetProvider, CancellationToken cancellationToken = default)
    {
        if (targetProvider == null)
        {
            return;
        }

        try
        {
            await targetProvider.ShutdownAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.ErrorShuttingDownProvider(targetProvider.GetMetadata()?.Name, ex);
        }
    }

    internal FeatureProvider GetProvider()
    {
        this._providersLock.EnterReadLock();
        try
        {
            return this._defaultProvider;
        }
        finally
        {
            this._providersLock.ExitReadLock();
        }
    }

    internal FeatureProvider GetProvider(string? domain)
    {
#if NETFRAMEWORK || NETSTANDARD
        // This is a workaround for the issue in .NET Framework where string.IsNullOrEmpty is not nullable compatible.
        if (domain == null)
        {
            return this.GetProvider();
        }
#else
        if (string.IsNullOrWhiteSpace(domain))
        {
            return this.GetProvider();
        }
#endif

        return this._featureProviders.TryGetValue(domain, out var featureProvider)
            ? featureProvider
            : this.GetProvider();
    }

    internal async Task ShutdownAsync(Action<FeatureProvider, Exception>? afterError = null, CancellationToken cancellationToken = default)
    {
        var providers = new HashSet<FeatureProvider>();
        this._providersLock.EnterWriteLock();
        try
        {
            providers.Add(this._defaultProvider);
            foreach (var featureProvidersValue in this._featureProviders.Values)
            {
                providers.Add(featureProvidersValue);
            }

            // Set a default provider so the Api is ready to be used again.
            this._defaultProvider = new NoOpFeatureProvider();
            this._featureProviders.Clear();
        }
        finally
        {
            this._providersLock.ExitWriteLock();
        }

        foreach (var targetProvider in providers)
        {
            // We don't need to take any actions after shutdown.
            await this.SafeShutdownProviderAsync(targetProvider, cancellationToken).ConfigureAwait(false);
        }
    }

    [LoggerMessage(EventId = 105, Level = LogLevel.Error, Message = "Error shutting down provider: {TargetProviderName}`")]
    partial void ErrorShuttingDownProvider(string? targetProviderName, Exception exception);
}
