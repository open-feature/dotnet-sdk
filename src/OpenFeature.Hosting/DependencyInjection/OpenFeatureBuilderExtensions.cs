using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenFeature.Constant;
using OpenFeature.DependencyInjection;
using OpenFeature.DependencyInjection.Internal;
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
    public static OpenFeatureBuilder AddContext(this OpenFeatureBuilder builder, Action<EvaluationContextBuilder> configure)
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
    public static OpenFeatureBuilder AddContext(this OpenFeatureBuilder builder, Action<EvaluationContextBuilder, IServiceProvider> configure)
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
    /// Adds a feature provider using a factory method without additional configuration options.
    /// This method adds the feature provider as a transient service and sets it as the default provider within the application.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> used to configure feature flags.</param>
    /// <param name="implementationFactory">
    /// A factory method that creates and returns a <see cref="FeatureProvider"/>
    /// instance based on the provided service provider.
    /// </param>
    /// <returns>The updated <see cref="OpenFeatureBuilder"/> instance with the default feature provider set and configured.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="builder"/> is null, as a valid builder is required to add and configure providers.</exception>
    public static OpenFeatureBuilder AddProvider(this OpenFeatureBuilder builder, Func<IServiceProvider, FeatureProvider> implementationFactory)
        => AddProvider<OpenFeatureOptions>(builder, implementationFactory, null);

    /// <summary>
    /// Adds a feature provider using a factory method to create the provider instance and optionally configures its settings.
    /// This method adds the feature provider as a transient service and sets it as the default provider within the application.
    /// </summary>
    /// <typeparam name="TOptions"> Type derived from <see cref="OpenFeatureOptions"/> used to configure the feature provider.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> used to configure feature flags.</param>
    /// <param name="implementationFactory">
    /// A factory method that creates and returns a <see cref="FeatureProvider"/>
    /// instance based on the provided service provider.
    /// </param>
    /// <param name="configureOptions">An optional delegate to configure the provider-specific options.</param>
    /// <returns>The updated <see cref="OpenFeatureBuilder"/> instance with the default feature provider set and configured.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="builder"/> is null, as a valid builder is required to add and configure providers.</exception>
    public static OpenFeatureBuilder AddProvider<TOptions>(this OpenFeatureBuilder builder, Func<IServiceProvider, FeatureProvider> implementationFactory, Action<TOptions>? configureOptions)
        where TOptions : OpenFeatureOptions
    {
        Guard.ThrowIfNull(builder);

        builder.HasDefaultProvider = true;
        builder.Services.PostConfigure<TOptions>(options => options.AddDefaultProviderName());
        if (configureOptions != null)
        {
            builder.Services.Configure(configureOptions);
        }

        builder.Services.TryAddTransient(implementationFactory);
        builder.AddClient();
        return builder;
    }

    /// <summary>
    /// Adds a feature provider for a specific domain using provided options and a configuration builder.
    /// </summary>
    /// <typeparam name="TOptions"> Type derived from <see cref="OpenFeatureOptions"/> used to configure the feature provider.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> used to configure feature flags.</param>
    /// <param name="domain">The unique name of the provider.</param>
    /// <param name="implementationFactory">
    /// A factory method that creates a feature provider instance.
    /// It adds the provider as a transient service unless it is already added.
    /// </param>
    /// <param name="configureOptions">An optional delegate to configure the provider-specific options.</param>
    /// <returns>The updated <see cref="OpenFeatureBuilder"/> instance with the new feature provider configured.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if either <paramref name="builder"/> or <paramref name="domain"/> is null or if the <paramref name="domain"/> is empty.
    /// </exception>
    public static OpenFeatureBuilder AddProvider<TOptions>(this OpenFeatureBuilder builder, string domain, Func<IServiceProvider, string, FeatureProvider> implementationFactory, Action<TOptions>? configureOptions)
        where TOptions : OpenFeatureOptions
    {
        Guard.ThrowIfNull(builder);

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

        builder.AddClient(domain);
        return builder;
    }

    /// <summary>
    /// Adds a feature provider for a specified domain using the default options.
    /// This method configures a feature provider without custom options, delegating to the more generic AddProvider method.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> used to configure feature flags.</param>
    /// <param name="domain">The unique name of the provider.</param>
    /// <param name="implementationFactory">
    /// A factory method that creates a feature provider instance.
    /// It adds the provider as a transient service unless it is already added.
    /// </param>
    /// <returns>The updated <see cref="OpenFeatureBuilder"/> instance with the new feature provider configured.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if either <paramref name="builder"/> or <paramref name="domain"/> is null or if the <paramref name="domain"/> is empty.
    /// </exception>
    public static OpenFeatureBuilder AddProvider(this OpenFeatureBuilder builder, string domain, Func<IServiceProvider, string, FeatureProvider> implementationFactory)
        => AddProvider<OpenFeatureOptions>(builder, domain, implementationFactory, configureOptions: null);

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
    /// Adds a default <see cref="IFeatureClient"/> to the <see cref="OpenFeatureBuilder"/> based on the policy name options.
    /// This method configures the dependency injection container to resolve the appropriate <see cref="IFeatureClient"/>
    /// depending on the policy name selected.
    /// If no name is selected (i.e., null), it retrieves the default client.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <returns>The configured <see cref="OpenFeatureBuilder"/> instance.</returns>
    internal static OpenFeatureBuilder AddPolicyBasedClient(this OpenFeatureBuilder builder)
    {
        builder.Services.AddScoped(provider =>
        {
            var policy = provider.GetRequiredService<IOptions<PolicyNameOptions>>().Value;
            var name = policy.DefaultNameSelector(provider);
            if (name == null)
            {
                return provider.GetRequiredService<IFeatureClient>();
            }
            return provider.GetRequiredKeyedService<IFeatureClient>(name);
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

    /// <summary>
    /// Adds a feature hook to the service collection using a factory method. Hooks added here are not domain-bound.
    /// </summary>
    /// <typeparam name="THook">The type of<see cref="Hook"/> to be added.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="implementationFactory">Optional factory for controlling how <typeparamref name="THook"/> will be created in the DI container.</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    public static OpenFeatureBuilder AddHook<
#if NET
        [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
    THook>(this OpenFeatureBuilder builder, Func<IServiceProvider, THook>? implementationFactory = null)
        where THook : Hook
    {
        return builder.AddHook(typeof(THook).Name, implementationFactory);
    }

    /// <summary>
    /// Adds a feature hook to the service collection. Hooks added here are not domain-bound.
    /// </summary>
    /// <typeparam name="THook">The type of<see cref="Hook"/> to be added.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="hook">Instance of Hook to inject into the OpenFeature context.</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    public static OpenFeatureBuilder AddHook<
#if NET
        [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
    THook>(this OpenFeatureBuilder builder, THook hook)
        where THook : Hook
    {
        return builder.AddHook(typeof(THook).Name, hook);
    }

    /// <summary>
    /// Adds a feature hook to the service collection with a specified name. Hooks added here are not domain-bound.
    /// </summary>
    /// <typeparam name="THook">The type of<see cref="Hook"/> to be added.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="hookName">The name of the <see cref="Hook"/> that is being added.</param>
    /// <param name="hook">Instance of Hook to inject into the OpenFeature context.</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    public static OpenFeatureBuilder AddHook<
#if NET
        [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
    THook>(this OpenFeatureBuilder builder, string hookName, THook hook)
        where THook : Hook
    {
        return builder.AddHook(hookName, _ => hook);
    }

    /// <summary>
    /// Adds a feature hook to the service collection using a factory method and specified name. Hooks added here are not domain-bound.
    /// </summary>
    /// <typeparam name="THook">The type of<see cref="Hook"/> to be added.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="hookName">The name of the <see cref="Hook"/> that is being added.</param>
    /// <param name="implementationFactory">Optional factory for controlling how <typeparamref name="THook"/> will be created in the DI container.</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    public static OpenFeatureBuilder AddHook<
#if NET
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
    THook>
        (this OpenFeatureBuilder builder, string hookName, Func<IServiceProvider, THook>? implementationFactory = null)
        where THook : Hook
    {
        builder.Services.PostConfigure<OpenFeatureOptions>(options => options.AddHookName(hookName));

        if (implementationFactory is not null)
        {
            builder.Services.TryAddKeyedSingleton<Hook>(hookName, (serviceProvider, key) =>
            {
                return implementationFactory(serviceProvider);
            });
        }
        else
        {
            builder.Services.TryAddKeyedSingleton<Hook, THook>(hookName);
        }

        return builder;
    }

    /// <summary>
    /// Add a <see cref="EventHandlerDelegate"/> to allow you to react to state changes in the provider or underlying flag management system, such as flag definition changes, provider readiness, or error conditions
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="type">The type <see cref="ProviderEventTypes"/> to handle.</param>
    /// <param name="eventHandlerDelegate">The handler which reacts to <see cref="ProviderEventTypes"/>.</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    public static OpenFeatureBuilder AddHandler(this OpenFeatureBuilder builder, ProviderEventTypes type, EventHandlerDelegate eventHandlerDelegate)
    {
        return AddHandler(builder, type, _ => eventHandlerDelegate);
    }

    /// <summary>
    /// Add a <see cref="EventHandlerDelegate"/> to allow you to react to state changes in the provider or underlying flag management system, such as flag definition changes, provider readiness, or error conditions
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="type">The type <see cref="ProviderEventTypes"/> to handle.</param>
    /// <param name="implementationFactory">The handler factory for creating a handler which reacts to <see cref="ProviderEventTypes"/>.</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    public static OpenFeatureBuilder AddHandler(this OpenFeatureBuilder builder, ProviderEventTypes type, Func<IServiceProvider, EventHandlerDelegate> implementationFactory)
    {
        builder.Services.AddSingleton((serviceProvider) =>
        {
            var handler = implementationFactory(serviceProvider);
            return new EventHandlerDelegateWrapper(type, handler);
        });

        return builder;
    }
}
