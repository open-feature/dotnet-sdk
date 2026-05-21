using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenFeature.Hosting;
using OpenFeature.Hosting.Internal;
using OpenFeature.Isolated;

namespace OpenFeature;

/// <summary>
/// Contains extension methods for the <see cref="IServiceCollection"/> class.
/// </summary>
public static partial class OpenFeatureServiceCollectionExtensions
{
    /// <summary>
    /// Adds and configures OpenFeature services to the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <param name="configure">A configuration action for customizing OpenFeature setup via <see cref="OpenFeatureBuilder"/></param>
    /// <returns>The modified <see cref="IServiceCollection"/> instance</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> or <paramref name="configure"/> is null.</exception>
    public static IServiceCollection AddOpenFeature(this IServiceCollection services, Action<OpenFeatureBuilder> configure)
    {
        Guard.ThrowIfNull(services);
        Guard.ThrowIfNull(configure);

        // Register core OpenFeature services as singletons.
#pragma warning disable OFISO001 // Registering the isolated API instance as a singleton is intentional to ensure that all components within the application use the same isolated instance of the OpenFeature API. This design choice allows for consistent state management and behavior across the application while still providing isolation from the global API instance.
        services.TryAddSingleton(OpenFeatureFactory.CreateIsolated());
#pragma warning restore OFISO001
        services.TryAddSingleton<IFeatureLifecycleManager, FeatureLifecycleManager>();

        var builder = new OpenFeatureBuilder(services);
        configure(builder);

        builder.Services.Configure<OpenFeatureOptions>(c => { }); // Ensures IOptions<OpenFeatureOptions> is available even when no providers are configured.
        builder.Services.AddHostedService<HostedFeatureLifecycleService>();

        // If a default provider is specified without additional providers,
        // return early as no extra configuration is needed.
        if (builder.HasDefaultProvider && builder.DomainBoundProviderRegistrationCount == 0)
        {
            return services;
        }

        // Validate builder configuration to ensure consistency and required setup.
        builder.Validate();

        if (!builder.IsPolicyConfigured)
        {
            // Add a default name selector policy to use the first registered provider name as the default.
            builder.AddPolicyName(options =>
            {
                options.DefaultNameSelector = provider =>
                {
                    var options = provider.GetRequiredService<IOptions<OpenFeatureOptions>>().Value;
                    return options.ProviderNames.FirstOrDefault();
                };
            });
        }

        builder.AddPolicyBasedClient();

        return services;
    }
}
