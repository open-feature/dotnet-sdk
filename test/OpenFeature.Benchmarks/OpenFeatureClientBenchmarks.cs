
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoFixture;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using OpenFeature.Model;

namespace OpenFeature.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net60, baseline: true)]
    [JsonExporterAttribute.Full]
    [JsonExporterAttribute.FullCompressed]
    [SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
    public class OpenFeatureClientBenchmarks
    {
        private readonly string _clientName;
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
            _clientName = fixture.Create<string>();
            _clientVersion = fixture.Create<string>();
            _flagName = fixture.Create<string>();
            _defaultBoolValue = fixture.Create<bool>();
            _defaultStringValue = fixture.Create<string>();
            _defaultIntegerValue = fixture.Create<int>();
            _defaultDoubleValue = fixture.Create<double>();
            _defaultStructureValue = fixture.Create<Value>();
            _emptyFlagOptions = new FlagEvaluationOptions(ImmutableList<Hook>.Empty, ImmutableDictionary<string, object>.Empty);

            _client = Api.Instance.GetClient(_clientName, _clientVersion);
        }

        [Benchmark]
        public async Task OpenFeatureClient_GetBooleanValue_WithoutEvaluationContext_WithoutFlagEvaluationOptions() =>
            await _client.GetBooleanValue(_flagName, _defaultBoolValue);

        [Benchmark]
        public async Task OpenFeatureClient_GetBooleanValue_WithEmptyEvaluationContext_WithoutFlagEvaluationOptions() =>
            await _client.GetBooleanValue(_flagName, _defaultBoolValue, EvaluationContext.Empty);

        [Benchmark]
        public async Task OpenFeatureClient_GetBooleanValue_WithEmptyEvaluationContext_WithEmptyFlagEvaluationOptions() =>
            await _client.GetBooleanValue(_flagName, _defaultBoolValue, EvaluationContext.Empty, _emptyFlagOptions);

        [Benchmark]
        public async Task OpenFeatureClient_GetStringValue_WithoutEvaluationContext_WithoutFlagEvaluationOptions() =>
            await _client.GetStringValue(_flagName, _defaultStringValue);

        [Benchmark]
        public async Task OpenFeatureClient_GetStringValue_WithEmptyEvaluationContext_WithoutFlagEvaluationOptions() =>
            await _client.GetStringValue(_flagName, _defaultStringValue, EvaluationContext.Empty);

        [Benchmark]
        public async Task OpenFeatureClient_GetStringValue_WithoutEvaluationContext_WithEmptyFlagEvaluationOptions() =>
            await _client.GetStringValue(_flagName, _defaultStringValue, EvaluationContext.Empty, _emptyFlagOptions);

        [Benchmark]
        public async Task OpenFeatureClient_GetIntegerValue_WithoutEvaluationContext_WithoutFlagEvaluationOptions() =>
            await _client.GetIntegerValue(_flagName, _defaultIntegerValue);

        [Benchmark]
        public async Task OpenFeatureClient_GetIntegerValue_WithEmptyEvaluationContext_WithoutFlagEvaluationOptions() =>
            await _client.GetIntegerValue(_flagName, _defaultIntegerValue, EvaluationContext.Empty);

        [Benchmark]
        public async Task OpenFeatureClient_GetIntegerValue_WithEmptyEvaluationContext_WithEmptyFlagEvaluationOptions() =>
            await _client.GetIntegerValue(_flagName, _defaultIntegerValue, EvaluationContext.Empty, _emptyFlagOptions);

        [Benchmark]
        public async Task OpenFeatureClient_GetDoubleValue_WithoutEvaluationContext_WithoutFlagEvaluationOptions() =>
            await _client.GetDoubleValue(_flagName, _defaultDoubleValue);

        [Benchmark]
        public async Task OpenFeatureClient_GetDoubleValue_WithEmptyEvaluationContext_WithoutFlagEvaluationOptions() =>
            await _client.GetDoubleValue(_flagName, _defaultDoubleValue, EvaluationContext.Empty);

        [Benchmark]
        public async Task OpenFeatureClient_GetDoubleValue_WithEmptyEvaluationContext_WithEmptyFlagEvaluationOptions() =>
            await _client.GetDoubleValue(_flagName, _defaultDoubleValue, EvaluationContext.Empty, _emptyFlagOptions);

        [Benchmark]
        public async Task OpenFeatureClient_GetObjectValue_WithoutEvaluationContext_WithoutFlagEvaluationOptions() =>
            await _client.GetObjectValue(_flagName, _defaultStructureValue);

        [Benchmark]
        public async Task OpenFeatureClient_GetObjectValue_WithEmptyEvaluationContext_WithoutFlagEvaluationOptions() =>
            await _client.GetObjectValue(_flagName, _defaultStructureValue, EvaluationContext.Empty);

        [Benchmark]
        public async Task OpenFeatureClient_GetObjectValue_WithEmptyEvaluationContext_WithEmptyFlagEvaluationOptions() =>
            await _client.GetObjectValue(_flagName, _defaultStructureValue, EvaluationContext.Empty, _emptyFlagOptions);
    }
}
