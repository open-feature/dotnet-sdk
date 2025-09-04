using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenFeature.Providers.Memory;

namespace OpenFeature.Hosting.Providers.Memory;

/// <summary>
/// Extension methods for configuring feature providers with <see cref="OpenFeatureBuilder"/>.
/// </summary>
#if NET8_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.Experimental(Diagnostics.FeatureCodes.NewDi)]
#endif
public static partial class FeatureBuilderExtensions
{
    /// <summary>
    /// Adds an in-memory feature provider to the <see cref="OpenFeatureBuilder"/> with a factory for flags.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance to configure.</param>
    /// <param name="flagsFactory">
    /// A factory function to provide an <see cref="IDictionary{TKey,TValue}"/> of flags. 
    /// If null, an empty provider will be created.
    /// </param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance for chaining.</returns>
    public static OpenFeatureBuilder AddInMemoryProvider(this OpenFeatureBuilder builder, Func<IServiceProvider, IDictionary<string, Flag>?> flagsFactory)
        => builder.AddProvider(provider =>
        {
            var flags = flagsFactory(provider);
            if (flags == null)
            {
                return new InMemoryProvider();
            }

            return new InMemoryProvider(flags);
        });

    /// <summary>
    /// Adds an in-memory feature provider to the <see cref="OpenFeatureBuilder"/> with a domain and factory for flags.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance to configure.</param>
    /// <param name="domain">The unique domain of the provider.</param>
    /// <param name="flagsFactory">
    /// A factory function to provide an <see cref="IDictionary{TKey,TValue}"/> of flags. 
    /// If null, an empty provider will be created.
    /// </param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance for chaining.</returns>
    public static OpenFeatureBuilder AddInMemoryProvider(this OpenFeatureBuilder builder, string domain, Func<IServiceProvider, IDictionary<string, Flag>?> flagsFactory)
        => builder.AddInMemoryProvider(domain, (provider, _) => flagsFactory(provider));

    /// <summary>
    /// Adds an in-memory feature provider to the <see cref="OpenFeatureBuilder"/> with a domain and contextual flag factory.
    /// If null, an empty provider will be created.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance to configure.</param>
    /// <param name="domain">The unique domain of the provider.</param>
    /// <param name="flagsFactory">
    /// A factory function to provide an <see cref="IDictionary{TKey,TValue}"/> of flags based on service provider and domain.
    /// </param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance for chaining.</returns>
    public static OpenFeatureBuilder AddInMemoryProvider(this OpenFeatureBuilder builder, string domain, Func<IServiceProvider, string, IDictionary<string, Flag>?> flagsFactory)
        => builder.AddProvider(domain, (provider, key) =>
        {
            var flags = flagsFactory(provider, key);
            if (flags == null)
            {
                return new InMemoryProvider();
            }

            return new InMemoryProvider(flags);
        });

    /// <summary>
    /// Adds an in-memory feature provider to the <see cref="OpenFeatureBuilder"/> with optional flag configuration.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance to configure.</param>
    /// <param name="configure">
    /// An optional delegate to configure feature flags in the in-memory provider. 
    /// If null, an empty provider will be created.
    /// </param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance for chaining.</returns>
    public static OpenFeatureBuilder AddInMemoryProvider(this OpenFeatureBuilder builder, Action<IDictionary<string, Flag>>? configure = null)
        => builder.AddProvider<InMemoryProviderOptions>(CreateProvider, options => ConfigureFlags(options, configure));

    /// <summary>
    /// Adds an in-memory feature provider with a specific domain to the <see cref="OpenFeatureBuilder"/> with optional flag configuration.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance to configure.</param>
    /// <param name="domain">The unique domain of the provider</param>
    /// <param name="configure">
    /// An optional delegate to configure feature flags in the in-memory provider. 
    /// If null, an empty provider will be created.
    /// </param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance for chaining.</returns>
    public static OpenFeatureBuilder AddInMemoryProvider(this OpenFeatureBuilder builder, string domain, Action<IDictionary<string, Flag>>? configure = null)
        => builder.AddProvider<InMemoryProviderOptions>(domain, CreateProvider, options => ConfigureFlags(options, configure));

    private static FeatureProvider CreateProvider(IServiceProvider provider, string domain)
    {
        var options = provider.GetRequiredService<IOptionsSnapshot<InMemoryProviderOptions>>().Get(domain);
        if (options.Flags == null)
        {
            return new InMemoryProvider();
        }

        return new InMemoryProvider(options.Flags);
    }

    private static FeatureProvider CreateProvider(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<IOptions<InMemoryProviderOptions>>().Value;
        if (options.Flags == null)
        {
            return new InMemoryProvider();
        }

        return new InMemoryProvider(options.Flags);
    }

    private static void ConfigureFlags(InMemoryProviderOptions options, Action<IDictionary<string, Flag>>? configure)
    {
        if (configure != null)
        {
            options.Flags = new Dictionary<string, Flag>();
            configure.Invoke(options.Flags);
        }
    }
}
