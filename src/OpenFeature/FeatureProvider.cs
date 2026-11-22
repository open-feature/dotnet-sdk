using System.Collections.Immutable;
using System.Threading.Channels;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature;

/// <summary>
/// The provider interface describes the abstraction layer for a feature flag provider.
/// A provider acts as it translates layer between the generic feature flag structure to a target feature flag system.
/// </summary>
/// <seealso href="https://github.com/open-feature/spec/blob/v0.5.2/specification/sections/02-providers.md">Provider specification</seealso>
public abstract class FeatureProvider
{
    /// <summary>
    /// Gets an immutable list of hooks that belong to the provider.
    /// By default, return an empty list
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
    public abstract Metadata? GetMetadata();

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
    /// Internally-managed provider status.
    /// The SDK uses this field to track the status of the provider.
    /// Not visible outside OpenFeature assembly
    /// </summary>
    internal virtual ProviderStatus Status { get; set; } = ProviderStatus.NotReady;

    /// <summary>
    /// Tracks which Api instance this provider is currently bound to.
    /// A provider should not be registered with more than one API instance simultaneously (spec 1.8.4).
    /// </summary>
    private Api? _boundApiInstance;

    private readonly object _domainBindingLock = new();
    private bool _isDomainBound;
    private string? _boundDomain;

    /// <summary>
    /// Attempts to bind this provider to the given API instance.
    /// Uses <see cref="Interlocked.CompareExchange{T}"/> for thread-safe check-and-set.
    /// </summary>
    /// <param name="api">The API instance to bind to.</param>
    /// <returns><c>true</c> if the provider was successfully bound (or was already bound to the same instance);
    /// <c>false</c> if the provider is already bound to a different API instance.</returns>
    internal bool TryBindApiInstance(Api api)
    {
        var previous = Interlocked.CompareExchange(ref this._boundApiInstance, api, null);
        return previous is null || ReferenceEquals(previous, api);
    }

    /// <summary>
    /// Clears the API instance binding, allowing this provider to be registered with another API instance.
    /// </summary>
    internal void UnbindApiInstance()
    {
        this._boundApiInstance = null;
    }

    /// <summary>
    /// Binds this provider to the given domain.
    /// </summary>
    /// <remarks>
    /// A <c>null</c> domain means the provider is bound as the default provider. This is not the same as unbound.
    /// </remarks>
    /// <param name="domain">The domain to bind to, or <c>null</c> for the default provider.</param>
    /// <returns><c>true</c> if the provider is now bound to <paramref name="domain"/>, or was already bound to it.
    /// <c>false</c> if the provider is already bound to a different domain.</returns>
    internal bool TryBindDomain(string? domain)
    {
        lock (this._domainBindingLock)
        {
            if (!this._isDomainBound)
            {
                this._isDomainBound = true;
                this._boundDomain = domain;
                return true;
            }

            return string.Equals(this._boundDomain, domain, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Reports whether this provider can be bound to the given domain. This method does not record a binding.
    /// </summary>
    /// <remarks>
    /// Use this method to reject a registration before any state changes. Only <see cref="TryBindDomain"/>
    /// records the binding, and only its result is final.
    /// </remarks>
    /// <param name="domain">The domain to test, or <c>null</c> for the default provider.</param>
    /// <returns><c>true</c> if the provider is unbound or already bound to <paramref name="domain"/>.</returns>
    internal bool CanBindDomain(string? domain)
    {
        lock (this._domainBindingLock)
        {
            return !this._isDomainBound || string.Equals(this._boundDomain, domain, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Clears the domain binding. The provider can then be bound to another domain.
    /// </summary>
    internal void UnbindDomain()
    {
        lock (this._domainBindingLock)
        {
            this._isDomainBound = false;
            this._boundDomain = null;
        }
    }

    /// <summary>
    /// Reports whether this provider keeps state for a single domain, for example a persistent cache, that it
    /// cannot share across domains. The default is <c>false</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Most providers keep no state per domain, and one instance can serve several domains. A provider that
    /// stores or caches data per domain can return <c>true</c>. The API then binds the instance to one domain
    /// at most. The domain passed to <see cref="InitializeAsync(EvaluationContext, string?, CancellationToken)"/>
    /// is the only domain the instance serves while that registration lasts.
    /// </para>
    /// <para>
    /// If you bind a domain-scoped instance to a second domain, the API throws an
    /// <see cref="InvalidOperationException"/>. The first binding stays in place.
    /// </para>
    /// <para>
    /// A provider that returns <c>true</c> must use the bound domain during initialization.
    /// </para>
    /// </remarks>
    /// <seealso href="https://openfeature.dev/specification/sections/providers#requirement-243">Specification 2.4.3</seealso>
    /// <seealso href="https://openfeature.dev/specification/sections/providers#requirement-244">Specification 2.4.4</seealso>
    public virtual bool IsDomainScoped => false;

    /// <summary>
    /// <para>
    /// This method is called before a provider is used to evaluate flags. Providers can overwrite this method,
    /// if they have special initialization needed prior being called for flag evaluation.
    /// When this method completes, the provider will be considered ready for use.
    /// </para>
    /// </summary>
    /// <param name="context"><see cref="EvaluationContext"/></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel any async side effects.</param>
    /// <returns>A task that completes when the initialization process is complete.</returns>
    /// <remarks>
    /// <para>
    /// Providers not implementing this method will be considered ready immediately.
    /// </para>
    /// <para>
    /// The SDK always calls <see cref="InitializeAsync(EvaluationContext, string?, CancellationToken)"/>.
    /// By default, that overload calls this method. Override this method if you do not need the bound domain.
    /// </para>
    /// </remarks>
    public virtual Task InitializeAsync(EvaluationContext context, CancellationToken cancellationToken = default)
    {
        // Intentionally left blank.
        return Task.CompletedTask;
    }

    /// <summary>
    /// <para>
    /// The SDK calls this method before it uses the provider to evaluate flags, and passes the domain that the
    /// provider is bound to. Override this method if the provider needs setup before flag evaluation.
    /// When this method completes, the provider is ready for use.
    /// </para>
    /// </summary>
    /// <param name="context"><see cref="EvaluationContext"/></param>
    /// <param name="domain">
    /// The domain that the provider is bound to, or <c>null</c> if it is the default provider.
    /// </param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel any async side effects.</param>
    /// <returns>A task that completes when the initialization process is complete.</returns>
    /// <remarks>
    /// <para>
    /// The SDK calls this overload. The default implementation calls
    /// <see cref="InitializeAsync(EvaluationContext, CancellationToken)"/>. A provider that overrides only
    /// that overload works without changes.
    /// </para>
    /// <para>
    /// The SDK initializes a provider instance once. If the same instance is registered again, under the same
    /// domain or under a new one, the SDK does not initialize it a second time. The domain passed is the
    /// domain that the instance was first registered under. A provider that keeps state per domain must
    /// override <see cref="IsDomainScoped"/> to return <c>true</c>. The SDK then binds the instance to one
    /// domain at most while it is registered.
    /// </para>
    /// </remarks>
    /// <seealso href="https://openfeature.dev/specification/sections/providers#requirement-241">Specification 2.4.1</seealso>
    public virtual Task InitializeAsync(EvaluationContext context, string? domain, CancellationToken cancellationToken = default)
        => this.InitializeAsync(context, cancellationToken);

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
    public Channel<object> GetEventChannel() => this.EventChannel;

    /// <summary>
    /// Track a user action or application state, usually representing a business objective or outcome. The implementation of this method is optional.
    /// </summary>
    /// <param name="trackingEventName">The name associated with this tracking event</param>
    /// <param name="evaluationContext">The evaluation context used in the evaluation of the flag (optional)</param>
    /// <param name="trackingEventDetails">Data pertinent to the tracking event (Optional)</param>
    public virtual void Track(string trackingEventName, EvaluationContext? evaluationContext = default, TrackingEventDetails? trackingEventDetails = default)
    {
        // Intentionally left blank.
    }
}
