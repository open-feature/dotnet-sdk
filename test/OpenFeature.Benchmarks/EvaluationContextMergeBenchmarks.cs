using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature.Benchmark;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
public class EvaluationContextMergeBenchmarks
{
    private const string FlagKey = "flag";

    private FeatureClient _emptyClient = null!;
    private FeatureClient _populatedClient = null!;
    private EvaluationContext _invocationContext = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        var emptyApi = new Api();
        await emptyApi.SetProviderAsync(new NoOpFeatureProvider());
        this._emptyClient = emptyApi.GetClient("empty");

        var populatedApi = new Api();
        await populatedApi.SetProviderAsync(new NoOpFeatureProvider());
        populatedApi.SetContext(CreateContext("api"));
        this._populatedClient = populatedApi.GetClient("populated", context: CreateContext("client"));

        this._invocationContext = CreateContext("invocation");

        var details = await this._populatedClient.GetBooleanDetailsAsync(FlagKey, false, this._invocationContext);
        if (details.ErrorType != ErrorType.None)
        {
            throw new InvalidOperationException($"Evaluation failed during setup: {details.ErrorType}");
        }
    }

    [Benchmark(Baseline = true)]
    public Task<FlagEvaluationDetails<bool>> AllContextsEmpty() =>
        this._emptyClient.GetBooleanDetailsAsync(FlagKey, false);

    [Benchmark]
    public Task<FlagEvaluationDetails<bool>> OnlyInvocationContext() =>
        this._emptyClient.GetBooleanDetailsAsync(FlagKey, false, this._invocationContext);

    [Benchmark]
    public Task<FlagEvaluationDetails<bool>> ApiClientAndInvocationContexts() =>
        this._populatedClient.GetBooleanDetailsAsync(FlagKey, false, this._invocationContext);

    private static EvaluationContext CreateContext(string prefix)
    {
        var builder = EvaluationContext.Builder();
        for (var i = 0; i < 5; i++)
        {
            builder.Set($"{prefix}-{i}", i);
        }

        return builder.Build();
    }
}
