using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenFeature.ServiceCollection.Feature;
using OpenFeatureSDK;
namespace OpenFeature.ServiceCollection;

public static class AddOpenFeatureExtention
{
    public static IServiceCollection AddOpenFeautre(this IServiceCollection serviceCollection,Action<OpenFeatureOption> options)
    {
        serviceCollection.Configure(options);
        serviceCollection.AddTransient(CreateOpenFeature);
        serviceCollection.AddTransient(typeof(IFeatureCollection<>), typeof(FeatureCollection<>));
        return serviceCollection;
    }

    private static FeatureClient CreateOpenFeature(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService <IOptions<OpenFeatureOption>>().Value;
        if (options.FeatureProvider != null)
        {
            OpenFeatureSDK.OpenFeature.Instance.SetProvider((FeatureProvider)serviceProvider.GetRequiredService(options.FeatureProvider));
        }

        var featureClient = OpenFeatureSDK.OpenFeature.Instance.GetClient(
            options.Name,
            options.Version,
            serviceProvider.GetService<ILogger<FeatureClient>>());

        featureClient.AddHooks(options.Hooks.Select(type=>(Hook)serviceProvider.GetRequiredService(type)));
        return featureClient;
    }
}

