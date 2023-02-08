
using System.Collections.Immutable;
using System.Threading.Tasks;
using AutoFixture;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using OpenFeature.Model;

namespace OpenFeature.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net60, baseline: true)]
    [RPlotExporter]
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
        }

        [Benchmark]
        public async Task OpenFeatureClient_GetBooleanValue_WithoutEvaluationContext_WithoutFlagEvaluationOptions()
        {
            Api.Instance.SetProvider(new NoOpFeatureProvider());
            var client = Api.Instance.GetClient(_clientName, _clientVersion);

            await client.GetBooleanValue(_flagName, _defaultBoolValue);
        }

        [Benchmark]
        public async Task OpenFeatureClient_GetBooleanValue_WithEmptyEvaluationContext_WithoutFlagEvaluationOptions()
        {
            Api.Instance.SetProvider(new NoOpFeatureProvider());
            var client = Api.Instance.GetClient(_clientName, _clientVersion);

            await client.GetBooleanValue(_flagName, _defaultBoolValue, EvaluationContext.Empty);
        }

        [Benchmark]
        public async Task OpenFeatureClient_GetBooleanValue_WithEmptyEvaluationContext_WithEmptyFlagEvaluationOptions()
        {
            Api.Instance.SetProvider(new NoOpFeatureProvider());
            var client = Api.Instance.GetClient(_clientName, _clientVersion);

            await client.GetBooleanValue(_flagName, _defaultBoolValue, EvaluationContext.Empty, _emptyFlagOptions);
        }

        [Benchmark]
        public async Task OpenFeatureClient_GetStringValue_WithoutEvaluationContext_WithoutFlagEvaluationOptions()
        {
            Api.Instance.SetProvider(new NoOpFeatureProvider());
            var client = Api.Instance.GetClient(_clientName, _clientVersion);

            await client.GetStringValue(_flagName, _defaultStringValue);
        }

        [Benchmark]
        public async Task OpenFeatureClient_GetStringValue_WithEmptyEvaluationContext_WithoutFlagEvaluationOptions()
        {
            Api.Instance.SetProvider(new NoOpFeatureProvider());
            var client = Api.Instance.GetClient(_clientName, _clientVersion);

            await client.GetStringValue(_flagName, _defaultStringValue, EvaluationContext.Empty);
        }

        [Benchmark]
        public async Task OpenFeatureClient_GetStringValue_WithoutEvaluationContext_WithEmptyFlagEvaluationOptions()
        {
            Api.Instance.SetProvider(new NoOpFeatureProvider());
            var client = Api.Instance.GetClient(_clientName, _clientVersion);

            await client.GetStringValue(_flagName, _defaultStringValue, EvaluationContext.Empty, _emptyFlagOptions);
        }

        [Benchmark]
        public async Task OpenFeatureClient_GetIntegerValue_WithoutEvaluationContext_WithoutFlagEvaluationOptions()
        {
            Api.Instance.SetProvider(new NoOpFeatureProvider());
            var client = Api.Instance.GetClient(_clientName, _clientVersion);

            await client.GetIntegerValue(_flagName, _defaultIntegerValue);
        }

        [Benchmark]
        public async Task OpenFeatureClient_GetIntegerValue_WithEmptyEvaluationContext_WithoutFlagEvaluationOptions()
        {
            Api.Instance.SetProvider(new NoOpFeatureProvider());
            var client = Api.Instance.GetClient(_clientName, _clientVersion);

            await client.GetIntegerValue(_flagName, _defaultIntegerValue, EvaluationContext.Empty);
        }

        [Benchmark]
        public async Task OpenFeatureClient_GetIntegerValue_WithEmptyEvaluationContext_WithEmptyFlagEvaluationOptions()
        {
            Api.Instance.SetProvider(new NoOpFeatureProvider());
            var client = Api.Instance.GetClient(_clientName, _clientVersion);

            await client.GetIntegerValue(_flagName, _defaultIntegerValue, EvaluationContext.Empty, _emptyFlagOptions);
        }

        [Benchmark]
        public async Task OpenFeatureClient_GetDoubleValue_WithoutEvaluationContext_WithoutFlagEvaluationOptions()
        {
            Api.Instance.SetProvider(new NoOpFeatureProvider());
            var client = Api.Instance.GetClient(_clientName, _clientVersion);

            await client.GetDoubleValue(_flagName, _defaultDoubleValue);
        }

        [Benchmark]
        public async Task OpenFeatureClient_GetDoubleValue_WithEmptyEvaluationContext_WithoutFlagEvaluationOptions()
        {
            Api.Instance.SetProvider(new NoOpFeatureProvider());
            var client = Api.Instance.GetClient(_clientName, _clientVersion);

            await client.GetDoubleValue(_flagName, _defaultDoubleValue, EvaluationContext.Empty);
        }

        [Benchmark]
        public async Task OpenFeatureClient_GetDoubleValue_WithEmptyEvaluationContext_WithEmptyFlagEvaluationOptions()
        {
            Api.Instance.SetProvider(new NoOpFeatureProvider());
            var client = Api.Instance.GetClient(_clientName, _clientVersion);

            await client.GetDoubleValue(_flagName, _defaultDoubleValue, EvaluationContext.Empty, _emptyFlagOptions);
        }

        [Benchmark]
        public async Task OpenFeatureClient_GetObjectValue_WithoutEvaluationContext_WithoutFlagEvaluationOptions()
        {
            Api.Instance.SetProvider(new NoOpFeatureProvider());
            var client = Api.Instance.GetClient(_clientName, _clientVersion);

            await client.GetObjectValue(_flagName, _defaultStructureValue);
        }

        [Benchmark]
        public async Task OpenFeatureClient_GetObjectValue_WithEmptyEvaluationContext_WithoutFlagEvaluationOptions()
        {
            Api.Instance.SetProvider(new NoOpFeatureProvider());
            var client = Api.Instance.GetClient(_clientName, _clientVersion);

            await client.GetObjectValue(_flagName, _defaultStructureValue, EvaluationContext.Empty);
        }

        [Benchmark]
        public async Task OpenFeatureClient_GetObjectValue_WithEmptyEvaluationContext_WithEmptyFlagEvaluationOptions()
        {
            Api.Instance.SetProvider(new NoOpFeatureProvider());
            var client = Api.Instance.GetClient(_clientName, _clientVersion);

            await client.GetObjectValue(_flagName, _defaultStructureValue, EvaluationContext.Empty, _emptyFlagOptions);
        }
    }
}
