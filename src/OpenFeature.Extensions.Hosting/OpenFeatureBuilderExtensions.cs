using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OpenFeature.Internal;
using OpenFeature.Model;

namespace OpenFeature;

/// <summary>
///
/// </summary>
public static class OpenFeatureBuilderExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure"></param>
    /// <returns>
    ///
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
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure"></param>
    /// <returns>
    ///
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
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="providerName"></param>
    /// <param name="configure"></param>
    /// <returns>
    ///
    /// </returns>
    public static OpenFeatureBuilder AddContext(
        this OpenFeatureBuilder builder,
        string? providerName,
        Action<EvaluationContextBuilder, string?, IServiceProvider> configure)
    {
        Check.NotNull(builder);
        Check.NotNull(configure);

        builder.Services.AddKeyedSingleton(providerName, (services, key) =>
        {
            var b = EvaluationContext.Builder();

            configure(b, key as string, services);

            return b.Build();
        });

        return builder;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="providerName"></param>
    public static void TryAddOpenFeatureClient(this OpenFeatureBuilder builder, string? providerName = null)
    {
        Check.NotNull(builder);

        builder.Services.AddHostedService<OpenFeatureHostedService>();

        builder.Services.TryAddKeyedSingleton(providerName, static (services, providerName) =>
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

        builder.Services.TryAddKeyedSingleton(providerName, static (services, providerName) => providerName switch
        {
            null => services.GetRequiredService<ILogger<FeatureClient>>(),
            not null => services.GetRequiredService<ILoggerFactory>().CreateLogger($"OpenFeature.FeatureClient.{providerName}")
        });

        builder.Services.TryAddKeyedTransient(providerName, static (services, providerName) =>
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

        builder.Services.TryAddKeyedTransient<IFeatureClient>(providerName, static (services, providerName) =>
        {
            var api = services.GetRequiredService<Api>();

            return api.GetClient(
                api.GetProviderMetadata(providerName as string ?? string.Empty).Name,
                null,
                services.GetRequiredKeyedService<ILogger>(providerName),
                services.GetRequiredKeyedService<EvaluationContextBuilder>(providerName).Build());
        });

        if (providerName is not null)
            builder.Services.Replace(ServiceDescriptor.Transient(services => services.GetRequiredKeyedService<IFeatureClient>(providerName)));
    }
}
