using System;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OpenFeature.Extensions.Hosting.Tests.TestingModels;

public class SomeHook : Hook;

public static class SomeHookExtensions
{
    public static OpenFeatureBuilder AddSomeHook(this OpenFeatureBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.TryAddSingleton<Hook, SomeHook>();

        return builder;
    }
}
