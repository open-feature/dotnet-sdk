using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenFeature.Constant;
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
        private readonly ProviderRepository _repository = new ProviderRepository();
        private readonly ConcurrentStack<Hook> _hooks = new ConcurrentStack<Hook>();

        /// The reader/writer locks are not disposed because the singleton instance should never be disposed.
        private readonly ReaderWriterLockSlim _evaluationContextLock = new ReaderWriterLockSlim();

        internal readonly EventExecutor EventExecutor = new EventExecutor();


        /// <summary>
        /// Singleton instance of Api
        /// </summary>
        public static Api Instance { get; } = new Api();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        // IE Lazy way of ensuring this is thread safe without using locks
        static Api() { }
        private Api() { }

        /// <summary>
        /// Sets the feature provider. In order to wait for the provider to be set, and initialization to complete,
        /// await the returned task.
        /// </summary>
        /// <remarks>The provider cannot be set to null. Attempting to set the provider to null has no effect.</remarks>
        /// <param name="featureProvider">Implementation of <see cref="FeatureProvider"/></param>
        public async Task SetProvider(FeatureProvider featureProvider)
        {
            this.EventExecutor.RegisterDefaultFeatureProvider(featureProvider);
            await this._repository.SetProvider(featureProvider, this.GetContext()).ConfigureAwait(false);
        }


        /// <summary>
        /// Sets the feature provider to given clientName. In order to wait for the provider to be set, and
        /// initialization to complete, await the returned task.
        /// </summary>
        /// <param name="clientName">Name of client</param>
        /// <param name="featureProvider">Implementation of <see cref="FeatureProvider"/></param>
        public async Task SetProvider(string clientName, FeatureProvider featureProvider)
        {
            this.EventExecutor.RegisterClientFeatureProvider(clientName, featureProvider);
            await this._repository.SetProvider(clientName, featureProvider, this.GetContext()).ConfigureAwait(false);
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
        /// Gets the feature provider with given clientName
        /// </summary>
        /// <param name="clientName">Name of client</param>
        /// <returns>A provider associated with the given clientName, if clientName is empty or doesn't
        /// have a corresponding provider the default provider will be returned</returns>
        public FeatureProvider GetProvider(string clientName)
        {
            return this._repository.GetProvider(clientName);
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
        public Metadata GetProviderMetadata() => this.GetProvider().GetMetadata();

        /// <summary>
        /// Gets providers metadata assigned to the given clientName. If the clientName has no provider
        /// assigned to it the default provider will be returned
        /// </summary>
        /// <param name="clientName">Name of client</param>
        /// <returns>Metadata assigned to provider</returns>
        public Metadata GetProviderMetadata(string clientName) => this.GetProvider(clientName).GetMetadata();

        /// <summary>
        /// Create a new instance of <see cref="FeatureClient"/> using the current provider
        /// </summary>
        /// <param name="name">Name of client</param>
        /// <param name="version">Version of client</param>
        /// <param name="logger">Logger instance used by client</param>
        /// <param name="context">Context given to this client</param>
        /// <returns><see cref="FeatureClient"/></returns>
        public FeatureClient GetClient(string name = null, string version = null, ILogger logger = null,
            EvaluationContext context = null) =>
            new FeatureClient(name, version, logger, context);

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
        public void SetContext(EvaluationContext context)
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
        /// <para>
        /// Shut down and reset the current status of OpenFeature API.
        /// </para>
        /// <para>
        /// This call cleans up all active providers and attempts to shut down internal event handling mechanisms.
        /// Once shut down is complete, API is reset and ready to use again.
        /// </para>
        /// </summary>
        public async Task Shutdown()
        {
            await this._repository.Shutdown().ConfigureAwait(false);
            await this.EventExecutor.Shutdown().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void AddHandler(ProviderEventTypes type, EventHandlerDelegate handler)
        {
            this.EventExecutor.AddApiLevelHandler(type, handler);
        }

        /// <inheritdoc />
        public void RemoveHandler(ProviderEventTypes type, EventHandlerDelegate handler)
        {
            this.EventExecutor.RemoveApiLevelHandler(type, handler);
        }

        /// <summary>
        /// Sets the logger for the API
        /// </summary>
        /// <param name="logger">The logger to be used</param>
        public void SetLogger(ILogger logger)
        {
            this.EventExecutor.Logger = logger;
        }
    }
}
