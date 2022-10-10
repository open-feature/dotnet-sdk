using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using OpenFeatureSDK.Model;

namespace OpenFeatureSDK
{
    /// <summary>
    /// The evaluation API allows for the evaluation of feature flag values, independent of any flag control plane or vendor.
    /// In the absence of a provider the evaluation API uses the "No-op provider", which simply returns the supplied default flag value.
    /// </summary>
    /// <seealso href="https://github.com/open-feature/spec/blob/main/specification/flag-evaluation.md#flag-evaluation-api"/>
    public sealed class OpenFeature
    {
        private EvaluationContext _evaluationContext = EvaluationContext.Empty;
        private FeatureProvider _featureProvider = new NoOpFeatureProvider();
        private readonly ConcurrentStack<Hook> _hooks = new ConcurrentStack<Hook>();

        private readonly ReaderWriterLockSlim _evaluationContextLock = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim _featureProviderLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Singleton instance of OpenFeature
        /// </summary>
        public static OpenFeature Instance { get; } = new OpenFeature();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        // IE Lazy way of ensuring this is thread safe without using locks
        static OpenFeature() { }
        private OpenFeature() { }

        /// <summary>
        /// Sets the feature provider
        /// </summary>
        /// <param name="featureProvider">Implementation of <see cref="FeatureProvider"/></param>
        public void SetProvider(FeatureProvider featureProvider)
        {
            this._featureProviderLock.EnterWriteLock();
            this._featureProvider = featureProvider;
            this._featureProviderLock.ExitWriteLock();
        }

        /// <summary>
        /// Gets the feature provider
        /// </summary>
        /// <returns><see cref="FeatureProvider"/></returns>
        public FeatureProvider GetProvider()
        {
            this._featureProviderLock.EnterReadLock();
            try
            {
                return this._featureProvider;
            }
            finally
            {
                this._featureProviderLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets providers metadata
        /// </summary>
        /// <returns><see cref="ClientMetadata"/></returns>
        public Metadata GetProviderMetadata() => this.GetProvider().GetMetadata();

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
        /// </summary>
        /// <param name="hooks">A list of <see cref="Hook"/></param>
        public void AddHooks(IEnumerable<Hook> hooks) => this._hooks.PushRange(hooks.ToArray());

        /// <summary>
        /// Adds a hook to global hooks list
        /// </summary>
        /// <param name="hook">A list of <see cref="Hook"/></param>
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
    }
}
