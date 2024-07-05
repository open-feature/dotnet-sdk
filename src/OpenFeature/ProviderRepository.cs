using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenFeature.Constant;
using OpenFeature.Model;


namespace OpenFeature
{
    /// <summary>
    /// This class manages the collection of providers, both default and named, contained by the API.
    /// </summary>
    internal sealed class ProviderRepository : IAsyncDisposable
    {
        private ILogger _logger;

        private FeatureProvider _defaultProvider = new NoOpFeatureProvider();

        private readonly ConcurrentDictionary<string, FeatureProvider> _featureProviders =
            new ConcurrentDictionary<string, FeatureProvider>();

        /// The reader/writer locks is not disposed because the singleton instance should never be disposed.
        ///
        /// Mutations of the _defaultProvider or _featureProviders are done within this lock even though
        /// _featureProvider is a concurrent collection. This is for a couple reasons, the first is that
        /// a provider should only be shutdown if it is not in use, and it could be in use as either a named or
        /// default provider.
        ///
        /// The second is that a concurrent collection doesn't provide any ordering so we could check a provider
        /// as it was being added or removed such as two concurrent calls to SetProvider replacing multiple instances
        /// of that provider under different names..
        private readonly ReaderWriterLockSlim _providersLock = new ReaderWriterLockSlim();

        public ProviderRepository()
        {
            this._logger = NullLogger<EventExecutor>.Instance;
        }

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
        public async Task SetProviderAsync(
            FeatureProvider? featureProvider,
            EvaluationContext context,
            Func<FeatureProvider, Task>? afterInitSuccess = null,
            Func<FeatureProvider, Exception, Task>? afterInitError = null)
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
                _ = this.ShutdownIfUnusedAsync(oldProvider);
            }
            finally
            {
                this._providersLock.ExitWriteLock();
            }

            await InitProviderAsync(this._defaultProvider, context, afterInitSuccess, afterInitError)
                .ConfigureAwait(false);
        }

        private static async Task InitProviderAsync(
            FeatureProvider? newProvider,
            EvaluationContext context,
            Func<FeatureProvider, Task>? afterInitialization,
            Func<FeatureProvider, Exception, Task>? afterError)
        {
            if (newProvider == null)
            {
                return;
            }
            if (newProvider.Status == ProviderStatus.NotReady)
            {
                try
                {
                    await newProvider.InitializeAsync(context).ConfigureAwait(false);
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
        /// <param name="clientName">the name to associate with the provider</param>
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
        public async Task SetProviderAsync(string? clientName,
            FeatureProvider? featureProvider,
            EvaluationContext context,
            Func<FeatureProvider, Task>? afterInitSuccess = null,
            Func<FeatureProvider, Exception, Task>? afterInitError = null,
            CancellationToken cancellationToken = default)
        {
            // Cannot set a provider for a null clientName.
            if (clientName == null)
            {
                return;
            }

            this._providersLock.EnterWriteLock();

            try
            {
                this._featureProviders.TryGetValue(clientName, out var oldProvider);
                if (featureProvider != null)
                {
                    this._featureProviders.AddOrUpdate(clientName, featureProvider,
                        (key, current) => featureProvider);
                }
                else
                {
                    // If names of clients are programmatic, then setting the provider to null could result
                    // in unbounded growth of the collection.
                    this._featureProviders.TryRemove(clientName, out _);
                }

                // We want to allow shutdown to happen concurrently with initialization, and the caller to not
                // wait for it.
                _ = this.ShutdownIfUnusedAsync(oldProvider);
            }
            finally
            {
                this._providersLock.ExitWriteLock();
            }

            await InitProviderAsync(featureProvider, context, afterInitSuccess, afterInitError).ConfigureAwait(false);
        }

        /// <remarks>
        /// Shutdown the feature provider if it is unused. This must be called within a write lock of the _providersLock.
        /// </remarks>
        private async Task ShutdownIfUnusedAsync(
            FeatureProvider? targetProvider)
        {
            if (ReferenceEquals(this._defaultProvider, targetProvider))
            {
                return;
            }

            if (targetProvider != null && this._featureProviders.Values.Contains(targetProvider))
            {
                return;
            }

            await this.SafeShutdownProviderAsync(targetProvider).ConfigureAwait(false);
        }

        /// <remarks>
        /// <para>
        /// Shut down the provider and capture any exceptions thrown.
        /// </para>
        /// <para>
        /// The provider is set either to a name or default before the old provider it shutdown, so
        /// it would not be meaningful to emit an error.
        /// </para>
        /// </remarks>
        private async Task SafeShutdownProviderAsync(FeatureProvider? targetProvider)
        {
            if (targetProvider == null)
            {
                return;
            }

            try
            {
                await targetProvider.ShutdownAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Error shutting down provider: {targetProvider.GetMetadata().Name}");
            }
        }

        public FeatureProvider GetProvider()
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

        public FeatureProvider GetProvider(string? clientName)
        {
#if NET6_0_OR_GREATER
            if (string.IsNullOrEmpty(clientName))
            {
                return this.GetProvider();
            }
#else
            // This is a workaround for the issue in .NET Framework where string.IsNullOrEmpty is not nullable compatible.
            if (clientName == null || string.IsNullOrEmpty(clientName))
            {
                return this.GetProvider();
            }
#endif

            return this._featureProviders.TryGetValue(clientName, out var featureProvider)
                ? featureProvider
                : this.GetProvider();
        }

        public async Task ShutdownAsync(Action<FeatureProvider, Exception>? afterError = null, CancellationToken cancellationToken = default)
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
                await this.SafeShutdownProviderAsync(targetProvider).ConfigureAwait(false);
            }
        }
    }
}
