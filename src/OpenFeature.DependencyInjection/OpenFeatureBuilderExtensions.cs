using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenFeature.DependencyInjection;
using OpenFeature.Model;

namespace OpenFeature;

/// <summary>
/// Contains extension methods for the <see cref="OpenFeatureBuilder"/> class.
/// </summary>
#if NET8_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.Experimental(DependencyInjection.Diagnostics.FeatureCodes.NewDi)]
#endif
public static partial class OpenFeatureBuilderExtensions
{
    /// <summary>
    /// This method is used to add a new context to the service collection.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="configure">the desired configuration</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="builder"/> or <paramref name="configure"/> action is null.</exception>
    public static OpenFeatureBuilder AddContext(
        this OpenFeatureBuilder builder,
        Action<EvaluationContextBuilder> configure)
    {
        Guard.ThrowIfNull(builder);
        Guard.ThrowIfNull(configure);

        return builder.AddContext((b, _) => configure(b));
    }

    /// <summary>
    /// This method is used to add a new context to the service collection.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="configure">the desired configuration</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="builder"/> or <paramref name="configure"/> action is null.</exception>
    public static OpenFeatureBuilder AddContext(
        this OpenFeatureBuilder builder,
        Action<EvaluationContextBuilder, IServiceProvider> configure)
    {
        Guard.ThrowIfNull(builder);
        Guard.ThrowIfNull(configure);

        builder.IsContextConfigured = true;
        builder.Services.TryAddTransient(provider =>
        {
            var contextBuilder = EvaluationContext.Builder();
            configure(contextBuilder, provider);
            return contextBuilder.Build();
        });

        return builder;
    }

    /// <summary>
    /// Adds a new feature provider with specified options and configuration builder.
    /// </summary>
    /// <typeparam name="TOptions">The <see cref="OpenFeatureOptions"/> type for configuring the feature provider.</typeparam>
    /// <typeparam name="TProviderFactory">The type of the provider factory implementing <see cref="IFeatureProviderFactory"/>.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="configureFactory">An optional action to configure the provider factory of type <typeparamref name="TProviderFactory"/>.</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="builder"/> is null.</exception>
    public static OpenFeatureBuilder AddProvider<TOptions, TProviderFactory>(this OpenFeatureBuilder builder, Action<TProviderFactory>? configureFactory = null)
        where TOptions : OpenFeatureOptions
        where TProviderFactory : class, IFeatureProviderFactory
    {
        Guard.ThrowIfNull(builder);

        builder.HasDefaultProvider = true;

        builder.Services.Configure<TOptions>(options =>
        {
            options.AddDefaultProviderName();
        });

        if (configureFactory != null)
        {
            builder.Services.AddOptions<TProviderFactory>()
                .Validate(options => options != null, $"{typeof(TProviderFactory).Name} configuration is invalid.")
                .Configure(configureFactory);
        }
        else
        {
            builder.Services.AddOptions<TProviderFactory>()
                .Configure(options => { });
        }

        builder.Services.TryAddSingleton(static provider =>
        {
            var providerFactory = provider.GetRequiredService<IOptions<TProviderFactory>>().Value;
            return providerFactory.Create();
        });

        builder.AddClient();

        return builder;
    }

    /// <summary>
    /// Adds a new feature provider with the default <see cref="OpenFeatureOptions"/> type and a specified configuration builder.
    /// </summary>
    /// <typeparam name="TProviderFactory">The type of the provider factory implementing <see cref="IFeatureProviderFactory"/>.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="configureFactory">An optional action to configure the provider factory of type <typeparamref name="TProviderFactory"/>.</param>
    /// <returns>The configured <see cref="OpenFeatureBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="builder"/> is null.</exception>
    public static OpenFeatureBuilder AddProvider<TProviderFactory>(this OpenFeatureBuilder builder, Action<TProviderFactory>? configureFactory = null)
        where TProviderFactory : class, IFeatureProviderFactory
        => AddProvider<OpenFeatureOptions, TProviderFactory>(builder, configureFactory);

    /// <summary>
    /// Adds a feature provider with specified options and configuration builder for the specified domain.
    /// </summary>
    /// <typeparam name="TOptions">The <see cref="OpenFeatureOptions"/> type for configuring the feature provider.</typeparam>
    /// <typeparam name="TProviderFactory">The type of the provider factory implementing <see cref="IFeatureProviderFactory"/>.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="domain">The unique name of the provider.</param>
    /// <param name="configureFactory">An optional action to configure the provider factory of type <typeparamref name="TProviderFactory"/>.</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="builder"/> or <paramref name="domain"/> is null or empty.</exception>
    public static OpenFeatureBuilder AddProvider<TOptions, TProviderFactory>(this OpenFeatureBuilder builder, string domain, Action<TProviderFactory>? configureFactory = null)
        where TOptions : OpenFeatureOptions
        where TProviderFactory : class, IFeatureProviderFactory
    {
        Guard.ThrowIfNull(builder);
        Guard.ThrowIfNullOrWhiteSpace(domain, nameof(domain));

        builder.DomainBoundProviderRegistrationCount++;

        builder.Services.Configure<TOptions>(options =>
        {
            options.AddProviderName(domain);
        });

        if (configureFactory != null)
        {
            builder.Services.AddOptions<TProviderFactory>(domain)
                .Validate(options => options != null, $"{typeof(TProviderFactory).Name} configuration is invalid.")
                .Configure(configureFactory);
        }
        else
        {
            builder.Services.AddOptions<TProviderFactory>(domain)
                .Configure(options => { });
        }

        builder.Services.TryAddKeyedSingleton(domain, static (provider, key) =>
        {
            var options = provider.GetRequiredService<IOptionsMonitor<TProviderFactory>>();
            var providerFactory = options.Get(key!.ToString());
            return providerFactory.Create();
        });

        builder.AddClient(domain);

        return builder;
    }

    /// <summary>
    /// Adds a feature provider with a specified configuration builder for the specified domain, using default <see cref="OpenFeatureOptions"/>.
    /// </summary>
    /// <typeparam name="TProviderFactory">The type of the provider factory implementing <see cref="IFeatureProviderFactory"/>.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="domain">The unique domain of the provider.</param>
    /// <param name="configureFactory">An optional action to configure the provider factory of type <typeparamref name="TProviderFactory"/>.</param>
    /// <returns>The configured <see cref="OpenFeatureBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="builder"/> or <paramref name="domain"/> is null or empty.</exception>
    public static OpenFeatureBuilder AddProvider<TProviderFactory>(this OpenFeatureBuilder builder, string domain, Action<TProviderFactory>? configureFactory = null)
        where TProviderFactory : class, IFeatureProviderFactory
        => AddProvider<OpenFeatureOptions, TProviderFactory>(builder, domain, configureFactory);

    /// <summary>
    /// Adds a feature client to the service collection, configuring it to work with a specific context if provided.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="name">Optional: The name for the feature client instance.</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    internal static OpenFeatureBuilder AddClient(this OpenFeatureBuilder builder, string? name = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            if (builder.IsContextConfigured)
            {
                builder.Services.TryAddScoped<IFeatureClient>(static provider =>
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
                builder.Services.TryAddScoped<IFeatureClient>(static provider =>
                {
                    var api = provider.GetRequiredService<Api>();
                    return api.GetClient();
                });
            }
        }
        else
        {
            if (builder.IsContextConfigured)
            {
                builder.Services.TryAddKeyedScoped<IFeatureClient>(name, static (provider, key) =>
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
                builder.Services.TryAddKeyedScoped<IFeatureClient>(name, static (provider, key) =>
                {
                    var api = provider.GetRequiredService<Api>();
                    return api.GetClient(key!.ToString());
                });
            }
        }

        return builder;
    }

    /// <summary>
    /// Configures a default client for OpenFeature using the provided factory function.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="clientFactory">
    /// A factory function that creates an <see cref="IFeatureClient"/> based on the service provider and <see cref="PolicyNameOptions"/>.
    /// </param>
    /// <returns>The configured <see cref="OpenFeatureBuilder"/> instance.</returns>
    internal static OpenFeatureBuilder AddDefaultClient(this OpenFeatureBuilder builder, Func<IServiceProvider, PolicyNameOptions, IFeatureClient> clientFactory)
    {
        builder.Services.AddScoped(provider =>
        {
            var policy = provider.GetRequiredService<IOptions<PolicyNameOptions>>().Value;
            return clientFactory(provider, policy);
        });

        return builder;
    }

    /// <summary>
    /// Configures policy name options for OpenFeature using the specified options type.
    /// </summary>
    /// <typeparam name="TOptions">The type of options used to configure <see cref="PolicyNameOptions"/>.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="configureOptions">A delegate to configure <typeparamref name="TOptions"/>.</param>
    /// <returns>The configured <see cref="OpenFeatureBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="builder"/> or <paramref name="configureOptions"/> is null.</exception>
    public static OpenFeatureBuilder AddPolicyName<TOptions>(this OpenFeatureBuilder builder, Action<TOptions> configureOptions)
        where TOptions : PolicyNameOptions
    {
        Guard.ThrowIfNull(builder);
        Guard.ThrowIfNull(configureOptions);

        builder.IsPolicyConfigured = true;

        builder.Services.Configure(configureOptions);
        return builder;
    }

    /// <summary>
    /// Configures the default policy name options for OpenFeature.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="configureOptions">A delegate to configure <see cref="PolicyNameOptions"/>.</param>
    /// <returns>The configured <see cref="OpenFeatureBuilder"/> instance.</returns>
    public static OpenFeatureBuilder AddPolicyName(this OpenFeatureBuilder builder, Action<PolicyNameOptions> configureOptions)
        => AddPolicyName<PolicyNameOptions>(builder, configureOptions);
}
