using Microsoft.Extensions.DependencyInjection;
using OpenFeature.DependencyInjection;
using OpenFeature.Hosting;

namespace OpenFeature;

/// <summary>
/// Extension methods for configuring the hosted feature lifecycle in the <see cref="OpenFeatureBuilder"/>.
/// </summary>
public static partial class OpenFeatureBuilderExtensions
{
    /// <summary>
    /// Adds the <see cref="HostedFeatureLifecycleService"/> to the OpenFeatureBuilder, 
    /// which manages the lifecycle of features within the application. It also allows 
    /// configuration of the <see cref="FeatureLifecycleStateOptions"/>.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="configureOptions">An optional action to configure <see cref="FeatureLifecycleStateOptions"/>.</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    public static OpenFeatureBuilder AddHostedFeatureLifecycle(this OpenFeatureBuilder builder, Action<FeatureLifecycleStateOptions>? configureOptions = null)
    {
        if (configureOptions == null)
        {
            builder.Services.Configure<FeatureLifecycleStateOptions>(cfg =>
            {
                cfg.StartState = FeatureStartState.Starting;
                cfg.StopState = FeatureStopState.Stopping;
            });
        }
        else
        {
            builder.Services.Configure(configureOptions);
        }

        builder.Services.AddHostedService<HostedFeatureLifecycleService>();
        return builder;
    }
}
