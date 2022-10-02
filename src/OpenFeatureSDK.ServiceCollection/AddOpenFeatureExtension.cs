using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenFeature.ServiceCollection.Feature;
using OpenFeatureSDK;
using OpenFeatureSDK.Model;

namespace OpenFeature.ServiceCollection;
/// <summary>
///  OpenFeature ServiceCollection Extensions
/// </summary>
public static class AddOpenFeatureExtension
{
    /// <summary>
    /// Add OpenFeature to ServiceCollection
    /// </summary>
    /// <param name="serviceCollection">service collection</param>
    /// <param name="options">open features options</param>
    /// <returns></returns>
    public static IServiceCollection AddOpenFeature(this IServiceCollection serviceCollection,Action<OpenFeatureOption> options)
    {
        serviceCollection.Configure(options);
        serviceCollection.AddTransient(CreateOpenFeature);
        serviceCollection.AddTransient(typeof(IFeatures<>), typeof(Features<>));
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

        if (options.EvaluationContext != null)
        {
            featureClient.SetContext((EvaluationContext) serviceProvider.GetRequiredService(options.EvaluationContext));
        }

        featureClient.AddHooks(options.Hooks.Select(type=>(Hook)serviceProvider.GetRequiredService(type)));
        return featureClient;
    }
}

