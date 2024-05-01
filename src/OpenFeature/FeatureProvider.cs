using System.Collections.Immutable;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature
{
    /// <summary>
    /// The provider interface describes the abstraction layer for a feature flag provider.
    /// A provider acts as the translates layer between the generic feature flag structure to a target feature flag system.
    /// </summary>
    /// <seealso href="https://github.com/open-feature/spec/blob/v0.5.2/specification/sections/02-providers.md">Provider specification</seealso>
    public abstract class FeatureProvider
    {
        /// <summary>
        /// Gets a immutable list of hooks that belong to the provider.
        /// By default return a empty list
        ///
        /// Executed in the order of hooks
        /// before: API, Client, Invocation, Provider
        /// after: Provider, Invocation, Client, API
        /// error (if applicable): Provider, Invocation, Client, API
        /// finally: Provider, Invocation, Client, API
        /// </summary>
        /// <returns>Immutable list of hooks</returns>
        public virtual IImmutableList<Hook> GetProviderHooks() => ImmutableList<Hook>.Empty;

        /// <summary>
        /// The event channel of the provider.
        /// </summary>
        protected readonly Channel<object> EventChannel = Channel.CreateBounded<object>(1);

        /// <summary>
        /// Metadata describing the provider.
        /// </summary>
        /// <returns><see cref="Metadata"/></returns>
        public abstract Metadata GetMetadata();

        /// <summary>
        /// Resolves a boolean feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="ResolutionDetails{T}"/></returns>
        public abstract Task<ResolutionDetails<bool>> ResolveBooleanValueAsync(string flagKey, bool defaultValue,
            EvaluationContext? context = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolves a string feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="ResolutionDetails{T}"/></returns>
        public abstract Task<ResolutionDetails<string>> ResolveStringValueAsync(string flagKey, string defaultValue,
            EvaluationContext? context = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolves a integer feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="ResolutionDetails{T}"/></returns>
        public abstract Task<ResolutionDetails<int>> ResolveIntegerValueAsync(string flagKey, int defaultValue,
            EvaluationContext? context = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolves a double feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="ResolutionDetails{T}"/></returns>
        public abstract Task<ResolutionDetails<double>> ResolveDoubleValueAsync(string flagKey, double defaultValue,
            EvaluationContext? context = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolves a structured feature flag
        /// </summary>
        /// <param name="flagKey">Feature flag key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="context"><see cref="EvaluationContext"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="ResolutionDetails{T}"/></returns>
        public abstract Task<ResolutionDetails<Value>> ResolveStructureValueAsync(string flagKey, Value defaultValue,
            EvaluationContext? context = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the status of the provider.
        /// </summary>
        /// <returns>The current <see cref="ProviderStatus"/></returns>
        /// <remarks>
        /// If a provider does not override this method, then its status will be assumed to be
        /// <see cref="ProviderStatus.Ready"/>. If a provider implements this method, and supports initialization,
        /// then it should start in the <see cref="ProviderStatus.NotReady"/>status . If the status is
        /// <see cref="ProviderStatus.NotReady"/>, then the Api will call the <see cref="InitializeAsync" /> when the
        /// provider is set.
        /// </remarks>
        public virtual ProviderStatus GetStatus() => ProviderStatus.Ready;

        /// <summary>
        /// <para>
        /// This method is called before a provider is used to evaluate flags. Providers can overwrite this method,
        /// if they have special initialization needed prior being called for flag evaluation.
        /// </para>
        /// </summary>
        /// <param name="context"><see cref="EvaluationContext"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel any async side effects.</param>
        /// <returns>A task that completes when the initialization process is complete.</returns>
        /// <remarks>
        /// <para>
        /// A provider which supports initialization should override this method as well as
        /// <see cref="GetStatus"/>.
        /// </para>
        /// <para>
        /// The provider should return <see cref="ProviderStatus.Ready"/> or <see cref="ProviderStatus.Error"/> from
        /// the <see cref="GetStatus"/> method after initialization is complete.
        /// </para>
        /// </remarks>
        public virtual Task InitializeAsync(EvaluationContext context, CancellationToken cancellationToken = default)
        {
            // Intentionally left blank.
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method is called when a new provider is about to be used to evaluate flags, or the SDK is shut down.
        /// Providers can overwrite this method, if they have special shutdown actions needed.
        /// </summary>
        /// <returns>A task that completes when the shutdown process is complete.</returns>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel any async side effects.</param>
        public virtual Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            // Intentionally left blank.
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns the event channel of the provider.
        /// </summary>
        /// <returns>The event channel of the provider</returns>
        public virtual Channel<object> GetEventChannel() => this.EventChannel;
    }
}
