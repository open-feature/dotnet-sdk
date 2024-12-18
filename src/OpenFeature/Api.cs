using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenFeature.Constant;
using OpenFeature.Error;
using OpenFeature.Model;

namespace OpenFeature
{
    /// <summary>
    /// The evaluation API allows for the evaluation of feature flag values, independent of any flag control plane or vendor.
    /// In the absence of a provider the evaluation API uses the "No-op provider", which simply returns the supplied default flag value.
    /// </summary>
    /// <seealso href="https://github.com/open-feature/spec/blob/v0.5.2/specification/sections/01-flag-evaluation.md#1-flag-evaluation-api"/>
    public sealed class Api : IEventBus
    {
        private EvaluationContext _evaluationContext = EvaluationContext.Empty;
        private EventExecutor _eventExecutor = new EventExecutor();
        private ProviderRepository _repository = new ProviderRepository();
        private readonly ConcurrentStack<Hook> _hooks = new ConcurrentStack<Hook>();
        private ITransactionContextPropagator _transactionContextPropagator = new NoOpTransactionContextPropagator();
        private readonly object _transactionContextPropagatorLock = new();

        /// The reader/writer locks are not disposed because the singleton instance should never be disposed.
        private readonly ReaderWriterLockSlim _evaluationContextLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Singleton instance of Api
        /// </summary>
        public static Api Instance { get; } = new Api();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforeFieldInit
        // IE Lazy way of ensuring this is thread safe without using locks
        static Api() { }
        private Api() { }

        /// <summary>
        /// Sets the default feature provider. In order to wait for the provider to be set, and initialization to complete,
        /// await the returned task.
        /// </summary>
        /// <remarks>The provider cannot be set to null. Attempting to set the provider to null has no effect.</remarks>
        /// <param name="featureProvider">Implementation of <see cref="FeatureProvider"/></param>
        public async Task SetProviderAsync(FeatureProvider featureProvider)
        {
            this._eventExecutor.RegisterDefaultFeatureProvider(featureProvider);
            await this._repository.SetProviderAsync(featureProvider, this.GetContext(), this.AfterInitialization, this.AfterError).ConfigureAwait(false);

        }

        /// <summary>
        /// Binds the feature provider to the given domain. In order to wait for the provider to be set, and
        /// initialization to complete, await the returned task.
        /// </summary>
        /// <param name="domain">An identifier which logically binds clients with providers</param>
        /// <param name="featureProvider">Implementation of <see cref="FeatureProvider"/></param>
        public async Task SetProviderAsync(string domain, FeatureProvider featureProvider)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentNullException(nameof(domain));
            }
            this._eventExecutor.RegisterClientFeatureProvider(domain, featureProvider);
            await this._repository.SetProviderAsync(domain, featureProvider, this.GetContext(), this.AfterInitialization, this.AfterError).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the feature provider
        /// <para>
        /// The feature provider may be set from multiple threads, when accessing the global feature provider
        /// it should be accessed once for an operation, and then that reference should be used for all dependent
        /// operations. For instance, during an evaluation the flag resolution method, and the provider hooks
        /// should be accessed from the same reference, not two independent calls to
        /// <see cref="GetProvider()"/>.
        /// </para>
        /// </summary>
        /// <returns><see cref="FeatureProvider"/></returns>
        public FeatureProvider GetProvider()
        {
            return this._repository.GetProvider();
        }

        /// <summary>
        /// Gets the feature provider with given domain
        /// </summary>
        /// <param name="domain">An identifier which logically binds clients with providers</param>
        /// <returns>A provider associated with the given domain, if domain is empty or doesn't
        /// have a corresponding provider the default provider will be returned</returns>
        public FeatureProvider GetProvider(string domain)
        {
            return this._repository.GetProvider(domain);
        }

        /// <summary>
        /// Gets providers metadata
        /// <para>
        /// This method is not guaranteed to return the same provider instance that may be used during an evaluation
        /// in the case where the provider may be changed from another thread.
        /// For multiple dependent provider operations see <see cref="GetProvider()"/>.
        /// </para>
        /// </summary>
        /// <returns><see cref="ClientMetadata"/></returns>
        public Metadata? GetProviderMetadata() => this.GetProvider().GetMetadata();

        /// <summary>
        /// Gets providers metadata assigned to the given domain. If the domain has no provider
        /// assigned to it the default provider will be returned
        /// </summary>
        /// <param name="domain">An identifier which logically binds clients with providers</param>
        /// <returns>Metadata assigned to provider</returns>
        public Metadata? GetProviderMetadata(string domain) => this.GetProvider(domain).GetMetadata();

        /// <summary>
        /// Create a new instance of <see cref="FeatureClient"/> using the current provider
        /// </summary>
        /// <param name="name">Name of client</param>
        /// <param name="version">Version of client</param>
        /// <param name="logger">Logger instance used by client</param>
        /// <param name="context">Context given to this client</param>
        /// <returns><see cref="FeatureClient"/></returns>
        public FeatureClient GetClient(string? name = null, string? version = null, ILogger? logger = null,
            EvaluationContext? context = null) =>
            new FeatureClient(() => this._repository.GetProvider(name), name, version, logger, context);

        /// <summary>
        /// Appends list of hooks to global hooks list
        /// <para>
        /// The appending operation will be atomic.
        /// </para>
        /// </summary>
        /// <param name="hooks">A list of <see cref="Hook"/></param>
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

        /// <summary>
        /// Adds a hook to global hooks list
        /// <para>
        /// Hooks which are dependent on each other should be provided in a collection
        /// using the <see cref="AddHooks(IEnumerable{Hook})"/>.
        /// </para>
        /// </summary>
        /// <param name="hook">Hook that implements the <see cref="Hook"/> interface</param>
        public void AddHooks(Hook hook) => this._hooks.Push(hook);

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
        /// Removes all hooks from global hooks list
        /// </summary>
        public void ClearHooks() => this._hooks.Clear();

        /// <summary>
        /// Sets the global <see cref="EvaluationContext"/>
        /// </summary>
        /// <param name="context">The <see cref="EvaluationContext"/> to set</param>
        public void SetContext(EvaluationContext? context)
        {
            this._evaluationContextLock.EnterWriteLock();
            try
            {
                this._evaluationContext = context ?? EvaluationContext.Empty;
            }
            finally
            {
                this._evaluationContextLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets the global <see cref="EvaluationContext"/>
        /// <para>
        /// The evaluation context may be set from multiple threads, when accessing the global evaluation context
        /// it should be accessed once for an operation, and then that reference should be used for all dependent
        /// operations.
        /// </para>
        /// </summary>
        /// <returns>An <see cref="EvaluationContext"/></returns>
        public EvaluationContext GetContext()
        {
            this._evaluationContextLock.EnterReadLock();
            try
            {
                return this._evaluationContext;
            }
            finally
            {
                this._evaluationContextLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Return the transaction context propagator.
        /// </summary>
        /// <returns><see cref="ITransactionContextPropagator"/>the registered transaction context propagator</returns>
        public ITransactionContextPropagator GetTransactionContextPropagator()
        {
            return this._transactionContextPropagator;
        }

        /// <summary>
        /// Sets the transaction context propagator.
        /// </summary>
        /// <param name="transactionContextPropagator">the transaction context propagator to be registered</param>
        /// <exception cref="ArgumentNullException">Transaction context propagator cannot be null</exception>
        public void SetTransactionContextPropagator(ITransactionContextPropagator transactionContextPropagator)
        {
            if (transactionContextPropagator == null)
            {
                throw new ArgumentNullException(nameof(transactionContextPropagator),
                    "Transaction context propagator cannot be null");
            }

            lock (this._transactionContextPropagatorLock)
            {
                this._transactionContextPropagator = transactionContextPropagator;
            }
        }

        /// <summary>
        /// Returns the currently defined transaction context using the registered transaction context propagator.
        /// </summary>
        /// <returns><see cref="EvaluationContext"/>The current transaction context</returns>
        public EvaluationContext? GetTransactionContext()
        {
            return this._transactionContextPropagator.GetTransactionContext();
        }

        /// <summary>
        /// Sets the transaction context using the registered transaction context propagator.
        /// </summary>
        /// <param name="evaluationContext">The <see cref="EvaluationContext"/> to set</param>
        /// <exception cref="InvalidOperationException">Transaction context propagator is not set.</exception>
        /// <exception cref="ArgumentNullException">Evaluation context cannot be null</exception>
        public void SetTransactionContext(EvaluationContext evaluationContext)
        {
            if (evaluationContext == null)
            {
                throw new ArgumentNullException(nameof(evaluationContext), "Evaluation context cannot be null");
            }

            this._transactionContextPropagator.SetTransactionContext(evaluationContext);
        }

        /// <summary>
        /// <para>
        /// Shut down and reset the current status of OpenFeature API.
        /// </para>
        /// <para>
        /// This call cleans up all active providers and attempts to shut down internal event handling mechanisms.
        /// Once shut down is complete, API is reset and ready to use again.
        /// </para>
        /// </summary>
        public async Task ShutdownAsync()
        {
            await using (this._eventExecutor.ConfigureAwait(false))
            await using (this._repository.ConfigureAwait(false))
            {
                this._evaluationContext = EvaluationContext.Empty;
                this._hooks.Clear();
                this._transactionContextPropagator = new NoOpTransactionContextPropagator();

                // TODO: make these lazy to avoid extra allocations on the common cleanup path?
                this._eventExecutor = new EventExecutor();
                this._repository = new ProviderRepository();
            }
        }

        /// <inheritdoc />
        public void AddHandler(ProviderEventTypes type, EventHandlerDelegate handler)
        {
            this._eventExecutor.AddApiLevelHandler(type, handler);
        }

        /// <inheritdoc />
        public void RemoveHandler(ProviderEventTypes type, EventHandlerDelegate handler)
        {
            this._eventExecutor.RemoveApiLevelHandler(type, handler);
        }

        /// <summary>
        /// Sets the logger for the API
        /// </summary>
        /// <param name="logger">The logger to be used</param>
        public void SetLogger(ILogger logger)
        {
            this._eventExecutor.SetLogger(logger);
            this._repository.SetLogger(logger);
        }

        internal void AddClientHandler(string client, ProviderEventTypes eventType, EventHandlerDelegate handler)
            => this._eventExecutor.AddClientHandler(client, eventType, handler);

        internal void RemoveClientHandler(string client, ProviderEventTypes eventType, EventHandlerDelegate handler)
            => this._eventExecutor.RemoveClientHandler(client, eventType, handler);

        /// <summary>
        /// Update the provider state to READY and emit a READY event after successful init.
        /// </summary>
        private async Task AfterInitialization(FeatureProvider provider)
        {
            provider.Status = ProviderStatus.Ready;
            var eventPayload = new ProviderEventPayload
            {
                Type = ProviderEventTypes.ProviderReady,
                Message = "Provider initialization complete",
                ProviderName = provider.GetMetadata()?.Name,
            };

            await this._eventExecutor.EventChannel.Writer.WriteAsync(new Event { Provider = provider, EventPayload = eventPayload }).ConfigureAwait(false);
        }

        /// <summary>
        /// Update the provider state to ERROR and emit an ERROR after failed init.
        /// </summary>
        private async Task AfterError(FeatureProvider provider, Exception? ex)
        {
            provider.Status = typeof(ProviderFatalException) == ex?.GetType() ? ProviderStatus.Fatal : ProviderStatus.Error;
            var eventPayload = new ProviderEventPayload
            {
                Type = ProviderEventTypes.ProviderError,
                Message = $"Provider initialization error: {ex?.Message}",
                ProviderName = provider.GetMetadata()?.Name,
            };

            await this._eventExecutor.EventChannel.Writer.WriteAsync(new Event { Provider = provider, EventPayload = eventPayload }).ConfigureAwait(false);
        }
    }
}
