using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenFeature.DependencyInjection.Abstractions;
using OpenFeature.Model;

namespace OpenFeature.Hosting;

/// <summary>
/// Describes a <see cref="OpenFeatureBuilder"/> backed by an <see cref="IServiceCollection"/>.
/// </summary>
/// <param name="services">The services being configured.</param>
public class OpenFeatureBuilder(IServiceCollection services) : OpenFeatureProviderBuilder(services)
{
    /// <summary>
    /// Indicates whether the evaluation context has been configured.
    /// This property is used to determine if specific configurations or services
    /// should be initialized based on the presence of an evaluation context.
    /// </summary>
    public bool IsContextConfigured { get; internal set; }

    /// <summary>
    /// Internal convenience API to add a client by name (or the default client when <paramref name="name"/> is null/empty).
    /// Delegates to the overridable <see cref="TryAddClient(string?)"/>.
    /// </summary>
    /// <param name="name">Optional key for a named client registration.</param>
    /// <returns>The current <see cref="OpenFeatureProviderBuilder"/>.</returns>
    internal OpenFeatureProviderBuilder AddClient(string? name = null)
        => TryAddClient(name);

    /// <summary>
    /// Adds an <see cref="IFeatureClient"/> to the container, optionally keyed by <paramref name="name"/>.
    /// If an evaluation context is configured, the client is created with that context.
    /// </summary>
    /// <param name="name">Optional key for a named client registration.</param>
    /// <returns>The current <see cref="OpenFeatureProviderBuilder"/>.</returns>
    protected override OpenFeatureProviderBuilder TryAddClient(string? name = null)
        => string.IsNullOrWhiteSpace(name) ? AddClient() : AddDomainBoundClient(name!);

    /// <summary>
    /// Adds a global (domain-agnostic) <see cref="IFeatureClient"/>.
    /// The evaluation context (if configured) is resolved per scope at resolve-time.
    /// </summary>
    /// <returns>The current <see cref="OpenFeatureProviderBuilder"/>.</returns>
    private OpenFeatureProviderBuilder AddClient()
    {
        if (IsContextConfigured)
        {
            Services.TryAddScoped<IFeatureClient>(static provider =>
            {
                var api = provider.GetRequiredService<Api>();
                var client = api.GetClient();
                var context = provider.GetRequiredService<EvaluationContext>();
                client.SetContext(context);
                return client;
            });
        }
        else
        {
            Services.TryAddScoped<IFeatureClient>(static provider =>
            {
                var api = provider.GetRequiredService<Api>();
                return api.GetClient();
            });
        }

        return this;
    }

    /// <inheritdoc />
    private OpenFeatureProviderBuilder AddDomainBoundClient(string name)
    {
        if (IsContextConfigured)
        {
            Services.TryAddKeyedScoped<IFeatureClient>(name, static (provider, key) =>
            {
                var api = provider.GetRequiredService<Api>();
                var client = api.GetClient(key!.ToString());
                var context = provider.GetRequiredService<EvaluationContext>();
                client.SetContext(context);
                return client;
            });
        }
        else
        {
            Services.TryAddKeyedScoped<IFeatureClient>(name, static (provider, key) =>
            {
                var api = provider.GetRequiredService<Api>();
                return api.GetClient(key!.ToString());
            });
        }

        return this;
    }
}
