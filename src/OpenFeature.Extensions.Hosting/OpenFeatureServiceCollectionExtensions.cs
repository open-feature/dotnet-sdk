using System;
using OpenFeature;

#pragma warning disable IDE0130 // Namespace does not match folder structure
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contains extension methods for the <see cref="IServiceCollection"/> class.
/// </summary>
public static class OpenFeatureServiceCollectionExtensions
{
    /// <summary>
    /// This method is used to add OpenFeature to the service collection.
    /// OpenFeature will be registered as a singleton.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="configure">the desired configuration</param>
    /// <returns>the current <see cref="IServiceCollection"/> instance</returns>
    public static IServiceCollection AddOpenFeature(this IServiceCollection services, Action<OpenFeatureBuilder> configure)
    {
        Check.NotNull(services);
        Check.NotNull(configure);

        configure(AddOpenFeature(services));

        return services;
    }

    /// <summary>
    /// This method is used to add OpenFeature to the service collection.
    /// OpenFeature will be registered as a singleton.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <returns>the current <see cref="IServiceCollection"/> instance</returns>
    public static OpenFeatureBuilder AddOpenFeature(this IServiceCollection services)
    {
        Check.NotNull(services);

        var builder = new OpenFeatureBuilder(services);

        builder.TryAddOpenFeatureClient();

        return builder;
    }
}
