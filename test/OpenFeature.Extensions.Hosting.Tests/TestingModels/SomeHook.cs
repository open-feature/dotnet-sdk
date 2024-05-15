using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OpenFeature.Extensions.Hosting.Tests.TestingModels;

public class SomeHook : Hook
{

}

public static class SomeHookExtensions
{
    public static OpenFeatureBuilder AddSomeHook(this OpenFeatureBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.ServiceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<FeatureProvider, SomeFeatureProvider>());
        builder.TryAddOpenFeatureClient(SomeFeatureProvider.Name);

        return builder;
    }

    public static void AddSomeFeatureProvider(this OpenFeatureBuilder builder, Action<OpenFeatureBuilder> configure)
    {
        throw new NotImplementedException();
    }
}
