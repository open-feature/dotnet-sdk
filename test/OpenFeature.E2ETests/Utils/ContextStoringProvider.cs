using System.Threading;
using System.Threading.Tasks;
using OpenFeature.Model;

namespace OpenFeature.E2ETests.Utils;

public class ContextStoringProvider : FeatureProvider
{
    private EvaluationContext? evaluationContext;
    public EvaluationContext? EvaluationContext { get => this.evaluationContext; }

    public override Metadata? GetMetadata()
    {
        return new Metadata("ContextStoringProvider");
    }

    public override Task<ResolutionDetails<bool>> ResolveBooleanValueAsync(string flagKey, bool defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
    {
        this.evaluationContext = context;
        return Task.FromResult(new ResolutionDetails<bool>(flagKey, defaultValue));
    }

    public override Task<ResolutionDetails<double>> ResolveDoubleValueAsync(string flagKey, double defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
    {
        this.evaluationContext = context;
        return Task.FromResult(new ResolutionDetails<double>(flagKey, defaultValue));
    }

    public override Task<ResolutionDetails<int>> ResolveIntegerValueAsync(string flagKey, int defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
    {
        this.evaluationContext = context;
        return Task.FromResult(new ResolutionDetails<int>(flagKey, defaultValue));
    }

    public override Task<ResolutionDetails<string>> ResolveStringValueAsync(string flagKey, string defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
    {
        this.evaluationContext = context;
        return Task.FromResult(new ResolutionDetails<string>(flagKey, defaultValue));
    }

    public override Task<ResolutionDetails<Value>> ResolveStructureValueAsync(string flagKey, Value defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
    {
        this.evaluationContext = context;
        return Task.FromResult(new ResolutionDetails<Value>(flagKey, defaultValue));
    }
}
