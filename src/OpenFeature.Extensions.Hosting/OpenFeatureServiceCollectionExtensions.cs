using System;
using OpenFeature;

#pragma warning disable IDE0130 // Namespace does not match folder structure
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///
/// </summary>
public static class OpenFeatureServiceCollectionExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection AddOpenFeature(this IServiceCollection services, Action<OpenFeatureBuilder> configure)
    {
        Check.NotNull(services);
        Check.NotNull(configure);

        configure(AddOpenFeature(services));

        return services;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static OpenFeatureBuilder AddOpenFeature(this IServiceCollection services)
    {
        Check.NotNull(services);

        var builder = new OpenFeatureBuilder(services);

        builder.TryAddOpenFeatureClient();

        return builder;
    }
}
