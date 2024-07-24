using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OpenFeature.Internal;
using OpenFeature.Model;

namespace OpenFeature;

/// <summary>
/// Contains extension methods for the <see cref="OpenFeatureBuilder"/> class.
/// </summary>
public static class OpenFeatureBuilderExtensions
{
    /// <summary>
    /// This method is used to add a new context to the service collection.
    /// </summary>
    /// <param name="builder"><see cref="OpenFeatureBuilder"/></param>
    /// <param name="configure">the desired configuration</param>
    /// <returns>
    /// the <see cref="OpenFeatureBuilder"/> instance
    /// </returns>
    public static OpenFeatureBuilder AddContext(
        this OpenFeatureBuilder builder,
        Action<EvaluationContextBuilder> configure)
    {
        Check.NotNull(builder);
        Check.NotNull(configure);

        AddContext(builder, null, (b, _, _) => configure(b));

        return builder;
    }

    /// <summary>
    /// This method is used to add a new context to the service collection.
    /// </summary>
    /// <param name="builder"><see cref="OpenFeatureBuilder"/></param>
    /// <param name="configure">the desired configuration</param>
    /// <returns>
    /// the <see cref="OpenFeatureBuilder"/> instance
    /// </returns>
    public static OpenFeatureBuilder AddContext(
        this OpenFeatureBuilder builder,
        Action<EvaluationContextBuilder, IServiceProvider> configure)
    {
        Check.NotNull(builder);
        Check.NotNull(configure);

        AddContext(builder, null, (b, _, s) => configure(b, s));

        return builder;
    }

    /// <summary>
    /// This method is used to add a new context to the service collection.
    /// </summary>
    /// <param name="builder"><see cref="OpenFeatureBuilder"/></param>
    /// <param name="providerName">the name of the provider</param>
    /// <param name="configure">the desired configuration</param>
    /// <returns>
    /// the <see cref="OpenFeatureBuilder"/> instance
    /// </returns>
    public static OpenFeatureBuilder AddContext(
        this OpenFeatureBuilder builder,
        string? providerName,
        Action<EvaluationContextBuilder, string?, IServiceProvider> configure)
    {
        Check.NotNull(builder);
        Check.NotNull(configure);

        builder.ServiceCollection.AddKeyedSingleton(providerName, (services, key) =>
        {
            var b = EvaluationContext.Builder();

            configure(b, key as string, services);

            return b.Build();
        });

        return builder;
    }

    /// <summary>
    /// This method is used to add a new feature client to the service collection.
    /// </summary>
    /// <param name="builder"><see cref="OpenFeatureBuilder"/></param>
    /// <param name="providerName">the name of the provider</param>
    public static void TryAddOpenFeatureClient(this OpenFeatureBuilder builder, string? providerName = null)
    {
        Check.NotNull(builder);

        builder.ServiceCollection.AddHostedService<OpenFeatureHostedService>();

        builder.ServiceCollection.TryAddKeyedSingleton(providerName, static (services, providerName) =>
        {
            var api = providerName switch
            {
                null => Api.Instance,
                not null => services.GetRequiredKeyedService<Api>(null)
            };

            api.AddHooks(services.GetKeyedServices<Hook>(providerName));
            api.SetContext(services.GetRequiredKeyedService<EvaluationContextBuilder>(providerName).Build());

            return api;
        });

        builder.ServiceCollection.TryAddKeyedSingleton(providerName, static (services, providerName) => providerName switch
        {
            null => services.GetRequiredService<ILogger<FeatureClient>>(),
            not null => services.GetRequiredService<ILoggerFactory>().CreateLogger($"OpenFeature.FeatureClient.{providerName}")
        });

        builder.ServiceCollection.TryAddKeyedTransient(providerName, static (services, providerName) =>
        {
            var builder = providerName switch
            {
                null => EvaluationContext.Builder(),
                not null => services.GetRequiredKeyedService<EvaluationContextBuilder>(null)
            };

            foreach (var c in services.GetKeyedServices<EvaluationContext>(providerName))
            {
                builder.Merge(c);
            }

            return builder;
        });

        builder.ServiceCollection.TryAddKeyedTransient<IFeatureClient>(providerName, static (services, providerName) =>
        {
            var api = services.GetRequiredService<Api>();

            return api.GetClient(
                api.GetProviderMetadata(providerName as string ?? string.Empty)?.Name,
                null,
                services.GetRequiredKeyedService<ILogger>(providerName),
                services.GetRequiredKeyedService<EvaluationContextBuilder>(providerName).Build());
        });

        if (providerName is not null)
            builder.ServiceCollection.Replace(ServiceDescriptor.Transient(services => services.GetRequiredKeyedService<IFeatureClient>(providerName)));
    }
}
