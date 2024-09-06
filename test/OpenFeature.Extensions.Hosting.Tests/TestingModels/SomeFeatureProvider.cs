using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenFeature.Model;

namespace OpenFeature.Extensions.Hosting.Tests.TestingModels;

sealed class SomeFeatureProvider : FeatureProvider
{
    public const string Name = "some_feature_provider";

    public override Metadata GetMetadata() => new(Name);

    public override Task<ResolutionDetails<bool>> ResolveBooleanValueAsync(string flagKey, bool defaultValue, EvaluationContext? context = null,
        CancellationToken cancellationToken = default) => Task.FromResult(new ResolutionDetails<bool>(flagKey, defaultValue));

    public override Task<ResolutionDetails<string>> ResolveStringValueAsync(string flagKey, string defaultValue, EvaluationContext? context = null,
        CancellationToken cancellationToken = default) => Task.FromResult(new ResolutionDetails<string>(flagKey, defaultValue));

    public override Task<ResolutionDetails<int>> ResolveIntegerValueAsync(string flagKey, int defaultValue, EvaluationContext? context = null,
        CancellationToken cancellationToken = default) => Task.FromResult(new ResolutionDetails<int>(flagKey, defaultValue));

    public override Task<ResolutionDetails<double>> ResolveDoubleValueAsync(string flagKey, double defaultValue, EvaluationContext? context = null,
        CancellationToken cancellationToken = default) => Task.FromResult(new ResolutionDetails<double>(flagKey, defaultValue));

    public override Task<ResolutionDetails<Value>> ResolveStructureValueAsync(string flagKey, Value defaultValue, EvaluationContext? context = null,
        CancellationToken cancellationToken = default) => Task.FromResult(new ResolutionDetails<Value>(flagKey, defaultValue));
}

public static class SomeFeatureProviderExtensions
{
    public static OpenFeatureBuilder AddSomeFeatureProvider(this OpenFeatureBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<FeatureProvider, SomeFeatureProvider>());
        builder.TryAddOpenFeatureClient(SomeFeatureProvider.Name);

        return builder;
    }

    public static void AddSomeFeatureProvider(this OpenFeatureBuilder builder, Action<OpenFeatureBuilder> configure)
    {
        throw new NotImplementedException();
    }
}