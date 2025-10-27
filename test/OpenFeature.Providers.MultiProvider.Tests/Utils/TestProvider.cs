using OpenFeature.Model;

namespace OpenFeature.Providers.MultiProvider.Tests.Utils;

/// <summary>
/// A test implementation of FeatureProvider for MultiProvider testing.
/// </summary>
public class TestProvider : FeatureProvider
{
    private readonly string _name;
    private readonly Exception? _initException;
    private readonly Exception? _shutdownException;

    public TestProvider(string name, Exception? initException = null, Exception? shutdownException = null)
    {
        this._name = name;
        this._initException = initException;
        this._shutdownException = shutdownException;
    }

    public override Metadata GetMetadata() => new(this._name);

    public override async Task InitializeAsync(EvaluationContext context, CancellationToken cancellationToken = default)
    {
        if (this._initException != null)
        {
            throw this._initException;
        }

        await Task.CompletedTask;
    }

    public override async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        if (this._shutdownException != null)
        {
            throw this._shutdownException;
        }

        await Task.CompletedTask;
    }

    public override Task<ResolutionDetails<bool>> ResolveBooleanValueAsync(string flagKey, bool defaultValue,
        EvaluationContext? context = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(new ResolutionDetails<bool>(flagKey, defaultValue));

    public override Task<ResolutionDetails<string>> ResolveStringValueAsync(string flagKey, string defaultValue,
        EvaluationContext? context = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(new ResolutionDetails<string>(flagKey, defaultValue));

    public override Task<ResolutionDetails<int>> ResolveIntegerValueAsync(string flagKey, int defaultValue,
        EvaluationContext? context = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(new ResolutionDetails<int>(flagKey, defaultValue));

    public override Task<ResolutionDetails<double>> ResolveDoubleValueAsync(string flagKey, double defaultValue,
        EvaluationContext? context = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(new ResolutionDetails<double>(flagKey, defaultValue));

    public override Task<ResolutionDetails<Value>> ResolveStructureValueAsync(string flagKey, Value defaultValue,
        EvaluationContext? context = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(new ResolutionDetails<Value>(flagKey, defaultValue));
}
