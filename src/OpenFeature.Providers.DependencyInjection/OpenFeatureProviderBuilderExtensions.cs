using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenFeature.Providers.DependencyInjection;

namespace OpenFeature;

/// <summary>
/// Contains extension methods for the <see cref="OpenFeatureProviderBuilder"/> class.
/// </summary>
#if NET8_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.Experimental(Providers.DependencyInjection.Diagnostics.FeatureCodes.NewDi)]
#endif
public static partial class OpenFeatureProviderBuilderExtensions
{
    /// <summary>
    /// Adds a feature provider using a factory method without additional configuration options.
    /// This method adds the feature provider as a transient service and sets it as the default provider within the application.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureProviderBuilder"/> used to configure feature flags.</param>
    /// <param name="implementationFactory">
    /// A factory method that creates and returns a <see cref="FeatureProvider"/>
    /// instance based on the provided service provider.
    /// </param>
    /// <returns>The updated <see cref="OpenFeatureProviderBuilder"/> instance with the default feature provider set and configured.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="builder"/> is null, as a valid builder is required to add and configure providers.</exception>
    public static OpenFeatureProviderBuilder AddProvider(this OpenFeatureProviderBuilder builder, Func<IServiceProvider, FeatureProvider> implementationFactory)
        => AddProvider<OpenFeatureProviderOptions>(builder, implementationFactory, null);

    /// <summary>
    /// Adds a feature provider using a factory method to create the provider instance and optionally configures its settings.
    /// This method adds the feature provider as a transient service and sets it as the default provider within the application.
    /// </summary>
    /// <typeparam name="TOptions"> Type derived from <see cref="OpenFeatureProviderBuilder"/> used to configure the feature provider.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureProviderBuilder"/> used to configure feature flags.</param>
    /// <param name="implementationFactory">
    /// A factory method that creates and returns a <see cref="FeatureProvider"/>
    /// instance based on the provided service provider.
    /// </param>
    /// <param name="configureOptions">An optional delegate to configure the provider-specific options.</param>
    /// <returns>The updated <see cref="OpenFeatureProviderBuilder"/> instance with the default feature provider set and configured.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="builder"/> is null, as a valid builder is required to add and configure providers.</exception>
    public static OpenFeatureProviderBuilder AddProvider<TOptions>(this OpenFeatureProviderBuilder builder, Func<IServiceProvider, FeatureProvider> implementationFactory, Action<TOptions>? configureOptions)
        where TOptions : OpenFeatureProviderOptions
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        builder.HasDefaultProvider = true;
        builder.Services.PostConfigure<TOptions>(options => options.AddDefaultProviderName());
        if (configureOptions != null)
        {
            builder.Services.Configure(configureOptions);
        }

        builder.Services.TryAddTransient(implementationFactory);
        builder.TryAddClient();
        return builder;
    }

    /// <summary>
    /// Adds a feature provider for a specific domain using provided options and a configuration builder.
    /// </summary>
    /// <typeparam name="TOptions"> Type derived from <see cref="OpenFeatureProviderBuilder"/> used to configure the feature provider.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureProviderBuilder"/> used to configure feature flags.</param>
    /// <param name="domain">The unique name of the provider.</param>
    /// <param name="implementationFactory">
    /// A factory method that creates a feature provider instance.
    /// It adds the provider as a transient service unless it is already added.
    /// </param>
    /// <param name="configureOptions">An optional delegate to configure the provider-specific options.</param>
    /// <returns>The updated <see cref="OpenFeatureProviderBuilder"/> instance with the new feature provider configured.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if either <paramref name="builder"/> or <paramref name="domain"/> is null or if the <paramref name="domain"/> is empty.
    /// </exception>
    public static OpenFeatureProviderBuilder AddProvider<TOptions>(this OpenFeatureProviderBuilder builder, string domain, Func<IServiceProvider, string, FeatureProvider> implementationFactory, Action<TOptions>? configureOptions)
        where TOptions : OpenFeatureProviderOptions
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        builder.DomainBoundProviderRegistrationCount++;

        builder.Services.PostConfigure<TOptions>(options => options.AddProviderName(domain));
        if (configureOptions != null)
        {
            builder.Services.Configure(domain, configureOptions);
        }

        builder.Services.TryAddKeyedTransient(domain, (provider, key) =>
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return implementationFactory(provider, key.ToString()!);
        });

        builder.TryAddClient(domain);
        return builder;
    }

    /// <summary>
    /// Adds a feature provider for a specified domain using the default options.
    /// This method configures a feature provider without custom options, delegating to the more generic AddProvider method.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureProviderBuilder"/> used to configure feature flags.</param>
    /// <param name="domain">The unique name of the provider.</param>
    /// <param name="implementationFactory">
    /// A factory method that creates a feature provider instance.
    /// It adds the provider as a transient service unless it is already added.
    /// </param>
    /// <returns>The updated <see cref="OpenFeatureProviderBuilder"/> instance with the new feature provider configured.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if either <paramref name="builder"/> or <paramref name="domain"/> is null or if the <paramref name="domain"/> is empty.
    /// </exception>
    public static OpenFeatureProviderBuilder AddProvider(this OpenFeatureProviderBuilder builder, string domain, Func<IServiceProvider, string, FeatureProvider> implementationFactory)
        => AddProvider<OpenFeatureProviderOptions>(builder, domain, implementationFactory, configureOptions: null);

    /// <summary>
    /// Configures policy name options for OpenFeature using the specified options type.
    /// </summary>
    /// <typeparam name="TOptions">The type of options used to configure <see cref="OpenFeatureProviderOptions"/>.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureProviderBuilder"/> instance.</param>
    /// <param name="configureOptions">A delegate to configure <typeparamref name="TOptions"/>.</param>
    /// <returns>The configured <see cref="OpenFeatureProviderBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="builder"/> or <paramref name="configureOptions"/> is null.</exception>
    public static OpenFeatureProviderBuilder AddPolicyName<TOptions>(this OpenFeatureProviderBuilder builder, Action<TOptions> configureOptions)
        where TOptions : PolicyNameOptions
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

        builder.IsPolicyConfigured = true;

        builder.Services.Configure(configureOptions);
        return builder;
    }

    /// <summary>
    /// Configures the default policy name options for OpenFeature.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureProviderBuilder"/> instance.</param>
    /// <param name="configureOptions">A delegate to configure <see cref="OpenFeatureProviderBuilder"/>.</param>
    /// <returns>The configured <see cref="OpenFeatureProviderBuilder"/> instance.</returns>
    public static OpenFeatureProviderBuilder AddPolicyName(this OpenFeatureProviderBuilder builder, Action<PolicyNameOptions> configureOptions)
        => AddPolicyName<PolicyNameOptions>(builder, configureOptions);
}
