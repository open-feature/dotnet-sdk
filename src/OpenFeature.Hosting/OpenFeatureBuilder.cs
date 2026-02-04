using Microsoft.Extensions.DependencyInjection;
using OpenFeature.Providers.DependencyInjection;

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
    /// Adds an <see cref="IFeatureClient"/> to the container, optionally keyed by <paramref name="name"/>.
    /// If an evaluation context is configured, the client is created with that context.
    /// </summary>
    /// <param name="name">Optional key for a named client registration.</param>
    /// <returns>The current <see cref="OpenFeatureProviderBuilder"/>.</returns>
    protected override OpenFeatureProviderBuilder TryAddClient(string? name = null)
        => this.AddClient(name);
}
