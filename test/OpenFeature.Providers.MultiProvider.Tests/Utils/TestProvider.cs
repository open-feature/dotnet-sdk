using OpenFeature.Model;

namespace OpenFeature.Providers.MultiProvider.Tests.Utils;

public class TestProvider : FeatureProvider
{
    private const string DefaultName = "test-provider";

    private readonly string _name;
    private readonly Exception? _initException;

    /// <summary>
    /// A provider used for testing.
    /// </summary>
    /// <param name="name">the name of the provider.</param>
    /// <param name="initException">Optional exception to throw during init.</param>
    public TestProvider(string? name, Exception? initException = null)
    {
        this._name = string.IsNullOrWhiteSpace(name) ? DefaultName : name;
        this._initException = initException;
    }

    public override Metadata GetMetadata()
    {
        return new Metadata(this._name);
    }

    public override Task<ResolutionDetails<bool>> ResolveBooleanValueAsync(string flagKey, bool defaultValue,
        EvaluationContext? context = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(new ResolutionDetails<bool>(flagKey, !defaultValue));

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

    public override Task InitializeAsync(EvaluationContext context, CancellationToken cancellationToken = default)
    {
        return this._initException != null ? throw this._initException : Task.CompletedTask;
    }
}
