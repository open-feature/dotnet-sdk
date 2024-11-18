using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenFeature.DependencyInjection;
using OpenFeature.DependencyInjection.Internal;

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
        services.TryAddSingleton(Api.Instance);
        services.TryAddSingleton<IFeatureLifecycleManager, FeatureLifecycleManager>();

        var builder = new OpenFeatureBuilder(services);
        configure(builder);

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
                    return options.ProviderNames.First();
                };
            });
        }

        builder.AddDefaultClient((provider, policy) =>
        {
            var name = policy.DefaultNameSelector.Invoke(provider);
            if (name == null)
            {
                return provider.GetRequiredService<IFeatureClient>();
            }
            return provider.GetRequiredKeyedService<IFeatureClient>(name);
        });

        return services;
    }
}
