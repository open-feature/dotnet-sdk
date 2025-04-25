
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoFixture;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using OpenFeature.Model;

namespace OpenFeature.Benchmark;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60, baseline: true)]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
public class OpenFeatureClientBenchmarks
{
    private readonly string _domain;
    private readonly string _clientVersion;
    private readonly string _flagName;
    private readonly bool _defaultBoolValue;
    private readonly string _defaultStringValue;
    private readonly int _defaultIntegerValue;
    private readonly double _defaultDoubleValue;
    private readonly Value _defaultStructureValue;
    private readonly FlagEvaluationOptions _emptyFlagOptions;
    private readonly FeatureClient _client;

    public OpenFeatureClientBenchmarks()
    {
        var fixture = new Fixture();
        this._domain = fixture.Create<string>();
        this._clientVersion = fixture.Create<string>();
        this._flagName = fixture.Create<string>();
        this._defaultBoolValue = fixture.Create<bool>();
        this._defaultStringValue = fixture.Create<string>();
        this._defaultIntegerValue = fixture.Create<int>();
        this._defaultDoubleValue = fixture.Create<double>();
        this._defaultStructureValue = fixture.Create<Value>();
        this._emptyFlagOptions = new FlagEvaluationOptions(ImmutableList<Hook>.Empty, ImmutableDictionary<string, object>.Empty);

        this._client = Api.Instance.GetClient(this._domain, this._clientVersion);
    }

    [Benchmark]
    public async Task OpenFeatureClient_GetBooleanValue_WithoutEvaluationContext_WithoutFlagEvaluationOptions() =>
        await this._client.GetBooleanValueAsync(this._flagName, this._defaultBoolValue);

    [Benchmark]
    public async Task OpenFeatureClient_GetBooleanValue_WithEmptyEvaluationContext_WithoutFlagEvaluationOptions() =>
        await this._client.GetBooleanValueAsync(this._flagName, this._defaultBoolValue, EvaluationContext.Empty);

    [Benchmark]
    public async Task OpenFeatureClient_GetBooleanValue_WithEmptyEvaluationContext_WithEmptyFlagEvaluationOptions() =>
        await this._client.GetBooleanValueAsync(this._flagName, this._defaultBoolValue, EvaluationContext.Empty, this._emptyFlagOptions);

    [Benchmark]
    public async Task OpenFeatureClient_GetStringValue_WithoutEvaluationContext_WithoutFlagEvaluationOptions() =>
        await this._client.GetStringValueAsync(this._flagName, this._defaultStringValue);

    [Benchmark]
    public async Task OpenFeatureClient_GetStringValue_WithEmptyEvaluationContext_WithoutFlagEvaluationOptions() =>
        await this._client.GetStringValueAsync(this._flagName, this._defaultStringValue, EvaluationContext.Empty);

    [Benchmark]
    public async Task OpenFeatureClient_GetStringValue_WithoutEvaluationContext_WithEmptyFlagEvaluationOptions() =>
        await this._client.GetStringValueAsync(this._flagName, this._defaultStringValue, EvaluationContext.Empty, this._emptyFlagOptions);

    [Benchmark]
    public async Task OpenFeatureClient_GetIntegerValue_WithoutEvaluationContext_WithoutFlagEvaluationOptions() =>
        await this._client.GetIntegerValueAsync(this._flagName, this._defaultIntegerValue);

    [Benchmark]
    public async Task OpenFeatureClient_GetIntegerValue_WithEmptyEvaluationContext_WithoutFlagEvaluationOptions() =>
        await this._client.GetIntegerValueAsync(this._flagName, this._defaultIntegerValue, EvaluationContext.Empty);

    [Benchmark]
    public async Task OpenFeatureClient_GetIntegerValue_WithEmptyEvaluationContext_WithEmptyFlagEvaluationOptions() =>
        await this._client.GetIntegerValueAsync(this._flagName, this._defaultIntegerValue, EvaluationContext.Empty, this._emptyFlagOptions);

    [Benchmark]
    public async Task OpenFeatureClient_GetDoubleValue_WithoutEvaluationContext_WithoutFlagEvaluationOptions() =>
        await this._client.GetDoubleValueAsync(this._flagName, this._defaultDoubleValue);

    [Benchmark]
    public async Task OpenFeatureClient_GetDoubleValue_WithEmptyEvaluationContext_WithoutFlagEvaluationOptions() =>
        await this._client.GetDoubleValueAsync(this._flagName, this._defaultDoubleValue, EvaluationContext.Empty);

    [Benchmark]
    public async Task OpenFeatureClient_GetDoubleValue_WithEmptyEvaluationContext_WithEmptyFlagEvaluationOptions() =>
        await this._client.GetDoubleValueAsync(this._flagName, this._defaultDoubleValue, EvaluationContext.Empty, this._emptyFlagOptions);

    [Benchmark]
    public async Task OpenFeatureClient_GetObjectValue_WithoutEvaluationContext_WithoutFlagEvaluationOptions() =>
        await this._client.GetObjectValueAsync(this._flagName, this._defaultStructureValue);

    [Benchmark]
    public async Task OpenFeatureClient_GetObjectValue_WithEmptyEvaluationContext_WithoutFlagEvaluationOptions() =>
        await this._client.GetObjectValueAsync(this._flagName, this._defaultStructureValue, EvaluationContext.Empty);

    [Benchmark]
    public async Task OpenFeatureClient_GetObjectValue_WithEmptyEvaluationContext_WithEmptyFlagEvaluationOptions() =>
        await this._client.GetObjectValueAsync(this._flagName, this._defaultStructureValue, EvaluationContext.Empty, this._emptyFlagOptions);
}
