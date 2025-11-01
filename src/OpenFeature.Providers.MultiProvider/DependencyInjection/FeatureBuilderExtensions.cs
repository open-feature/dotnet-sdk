using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenFeature.Hosting;

namespace OpenFeature.Providers.MultiProvider.DependencyInjection;

/// <summary>
/// Extension methods for configuring the multi-provider with <see cref="OpenFeatureBuilder"/>.
/// </summary>
public static class FeatureBuilderExtensions
{
    /// <summary>
    /// Adds a multi-provider to the <see cref="OpenFeatureBuilder"/> with a configuration builder.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance to configure.</param>
    /// <param name="configure">
    /// A delegate to configure the multi-provider using the <see cref="MultiProviderBuilder"/>.
    /// </param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance for chaining.</returns>
    public static OpenFeatureBuilder AddMultiProvider(
        this OpenFeatureBuilder builder,
        Action<MultiProviderBuilder> configure)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        return builder.AddProvider<MultiProviderOptions>(
            serviceProvider => CreateMultiProviderFromConfigure(serviceProvider, configure),
            null);
    }

    /// <summary>
    /// Adds a multi-provider with a specific domain to the <see cref="OpenFeatureBuilder"/> with a configuration builder.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance to configure.</param>
    /// <param name="domain">The unique domain of the provider.</param>
    /// <param name="configure">
    /// A delegate to configure the multi-provider using the <see cref="MultiProviderBuilder"/>.
    /// </param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance for chaining.</returns>
    public static OpenFeatureBuilder AddMultiProvider(
        this OpenFeatureBuilder builder,
        string domain,
        Action<MultiProviderBuilder> configure)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (string.IsNullOrWhiteSpace(domain))
        {
            throw new ArgumentException("Domain cannot be null or empty.", nameof(domain));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        return builder.AddProvider<MultiProviderOptions>(
            domain,
            (serviceProvider, _) => CreateMultiProviderFromConfigure(serviceProvider, configure),
            null);
    }

    private static MultiProvider CreateMultiProviderFromConfigure(IServiceProvider serviceProvider, Action<MultiProviderBuilder> configure)
    {
        // Build the multi-provider configuration using the builder
        var multiProviderBuilder = new MultiProviderBuilder();

        // Apply the configuration action
        configure(multiProviderBuilder);

        // Build provider entries and strategy from the builder using the service provider
        var providerEntries = multiProviderBuilder.BuildProviderEntries(serviceProvider);
        var evaluationStrategy = multiProviderBuilder.BuildEvaluationStrategy(serviceProvider);

        if (providerEntries.Count == 0)
        {
            throw new InvalidOperationException("At least one provider must be configured for the multi-provider.");
        }

        // Get logger from DI
        var logger = serviceProvider.GetService<ILogger<MultiProvider>>();

        return new MultiProvider(providerEntries, evaluationStrategy, logger);
    }
}
