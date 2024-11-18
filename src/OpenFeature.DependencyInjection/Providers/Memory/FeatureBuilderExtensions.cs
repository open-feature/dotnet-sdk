using OpenFeature.Providers.Memory;

namespace OpenFeature.DependencyInjection.Providers.Memory;

/// <summary>
/// Extension methods for configuring feature providers with <see cref="OpenFeatureBuilder"/>.
/// </summary>
#if NET8_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.Experimental(Diagnostics.FeatureCodes.NewDi)]
#endif
public static partial class FeatureBuilderExtensions
{
    /// <summary>
    /// Adds an in-memory feature provider to the <see cref="OpenFeatureBuilder"/> with optional flag configuration.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance to configure.</param>
    /// <param name="configure">
    /// An optional delegate to configure feature flags in the in-memory provider. 
    /// If provided, it allows setting up the initial flags.
    /// </param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance for chaining.</returns>
    public static OpenFeatureBuilder AddInMemoryProvider(this OpenFeatureBuilder builder, Action<IDictionary<string, Flag>>? configure = null)
        => builder.AddProvider<InMemoryProviderFactory>(factory => ConfigureFlags(factory, configure));

    /// <summary>
    /// Adds an in-memory feature provider with a specific domain to the <see cref="OpenFeatureBuilder"/> 
    /// with optional flag configuration.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance to configure.</param>
    /// <param name="domain">The unique domain of the provider</param>
    /// <param name="configure">
    /// An optional delegate to configure feature flags in the in-memory provider. 
    /// If provided, it allows setting up the initial flags.
    /// </param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance for chaining.</returns>
    public static OpenFeatureBuilder AddInMemoryProvider(this OpenFeatureBuilder builder, string domain, Action<IDictionary<string, Flag>>? configure = null)
        => builder.AddProvider<InMemoryProviderFactory>(domain, factory => ConfigureFlags(factory, configure));

    /// <summary>
    /// Configures the feature flags for an <see cref="InMemoryProviderFactory"/> instance.
    /// </summary>
    /// <param name="factory">The <see cref="InMemoryProviderFactory"/> to configure.</param>
    /// <param name="configure">
    /// An optional delegate that sets up the initial flags in the provider's flag dictionary.
    /// </param>
    private static void ConfigureFlags(InMemoryProviderFactory factory, Action<IDictionary<string, Flag>>? configure)
    {
        if (configure == null)
            return;

        var flag = new Dictionary<string, Flag>();
        configure.Invoke(flag);
        factory.Flags = flag;
    }
}
