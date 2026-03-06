using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenFeature.Constant;
using OpenFeature.Hosting;
using OpenFeature.Hosting.Internal;
using OpenFeature.Model;
using OpenFeature.Providers.DependencyInjection;

namespace OpenFeature;

/// <summary>
/// Extension methods for configuring the hosted feature lifecycle in the <see cref="OpenFeatureBuilder"/>.
/// </summary>
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
    /// Adds a feature client to the service collection, configuring it to work with a specific context if provided.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="name">Optional: The name for the feature client instance.</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    internal static OpenFeatureBuilder AddClient(this OpenFeatureBuilder builder, string? name = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            builder.Services.TryAddScoped<IFeatureClient>(static provider =>
            {
                var api = provider.GetRequiredService<Api>();
                var client = api.GetClient();

                var context = provider.GetService<EvaluationContext>();
                if (context is not null)
                {
                    client.SetContext(context);
                }

                return client;
            });
        }
        else
        {
            builder.Services.TryAddKeyedScoped<IFeatureClient>(name, static (provider, key) =>
            {
                var api = provider.GetRequiredService<Api>();
                var client = api.GetClient(key!.ToString());

                var context = provider.GetService<EvaluationContext>();
                if (context is not null)
                {
                    client.SetContext(context);
                }

                return client;
            });
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
            return ResolveFeatureClient(provider, name);
        });

        return builder;
    }

    private static IFeatureClient ResolveFeatureClient(IServiceProvider provider, string? name = null)
    {
        var api = provider.GetRequiredService<Api>();
        var client = api.GetClient(name);
        var context = provider.GetService<EvaluationContext>();
        if (context != null)
        {
            client.SetContext(context);
        }

        return client;
    }

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

    /// <summary>
    /// Adds the <see cref="HostedFeatureLifecycleService"/> to the OpenFeatureBuilder, 
    /// which manages the lifecycle of features within the application. It also allows 
    /// configuration of the <see cref="FeatureLifecycleStateOptions"/>.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="configureOptions">An optional action to configure <see cref="FeatureLifecycleStateOptions"/>.</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    [Obsolete("Calling AddHostedFeatureLifecycle() is no longer necessary. OpenFeature will inject this automatically when you call AddOpenFeature().")]
    public static OpenFeatureBuilder AddHostedFeatureLifecycle(this OpenFeatureBuilder builder, Action<FeatureLifecycleStateOptions>? configureOptions = null)
    {
        if (configureOptions is not null)
        {
            builder.Services.Configure(configureOptions);
        }

        builder.Services.AddHostedService<HostedFeatureLifecycleService>();
        return builder;
    }
}
