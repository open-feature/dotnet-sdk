using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenFeature.Model;

namespace OpenFeature;

/// <summary>
/// Contains extension methods for the <see cref="OpenFeatureBuilder"/> class.
/// </summary>
public static partial class OpenFeatureBuilderExtensions
{
    /// <summary>
    /// This method is used to add a new context to the service collection.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="configure">the desired configuration</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
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
    /// Adds a feature provider to the service collection.
    /// </summary>
    /// <typeparam name="T">The type of the feature provider, which must inherit from <see cref="FeatureProvider"/>.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="providerFactory">A factory method to create the feature provider, using the service provider.</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    public static OpenFeatureBuilder AddProvider<T>(this OpenFeatureBuilder builder, Func<IServiceProvider, T> providerFactory)
        where T : FeatureProvider
    {
        Guard.ThrowIfNull(builder);
        builder.Services.TryAddSingleton<FeatureProvider>(providerFactory);
        return builder;
    }
}
