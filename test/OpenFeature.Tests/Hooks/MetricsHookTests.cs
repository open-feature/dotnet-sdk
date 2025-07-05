using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using OpenFeature.Hooks;
using OpenFeature.Model;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace OpenFeature.Tests.Hooks;

[CollectionDefinition(nameof(MetricsHookTest), DisableParallelization = true)]
public class MetricsHookTest : IDisposable
{
    private readonly List<Metric> _exportedItems;
    private readonly MeterProvider _meterProvider;

    public MetricsHookTest()
    {
        // Arrange metrics collector
        this._exportedItems = [];
        this._meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter("OpenFeature")
            .ConfigureResource(r => r.AddService("open-feature"))
            .AddInMemoryExporter(this._exportedItems,
                option => option.PeriodicExportingMetricReaderOptions =
                    new PeriodicExportingMetricReaderOptions { ExportIntervalMilliseconds = 100 })
            .Build();
    }

#pragma warning disable CA1816
    public void Dispose()
    {
        this._meterProvider.Shutdown();
    }
#pragma warning restore CA1816

    [Fact]
    public async Task After_Test()
    {
        // Arrange
        const string metricName = "feature_flag.evaluation_success_total";
        var metricsHook = new MetricsHook();
        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        await metricsHook.AfterAsync(ctx,
            new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default"),
            new Dictionary<string, object>()).ConfigureAwait(true);
        this._meterProvider.ForceFlush();

        // Assert metrics
        Assert.NotEmpty(this._exportedItems);

        // check if the metric is present in the exported items
        var metric = this._exportedItems.FirstOrDefault(m => m.Name == metricName);
        Assert.NotNull(metric);

        var noOtherMetric = this._exportedItems.All(m => m.Name == metricName);
        Assert.True(noOtherMetric);
    }

    [Fact]
    public async Task With_CustomDimensions_After_Test()
    {
        // Arrange
        var metricHookOptions = MetricsHookOptions.CreateBuilder()
            .WithCustomDimension("custom_dimension_key", "custom_dimension_value")
            .Build();

        var metricsHook = new MetricsHook(metricHookOptions);

        using var collector = new MetricCollector<long>(metricsHook._evaluationSuccessCounter);

        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        await metricsHook.AfterAsync(ctx,
            new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default"),
            new Dictionary<string, object>()).ConfigureAwait(true);

        var measurements = collector.LastMeasurement;

        // Assert
        Assert.NotNull(measurements);

        Assert.Equal("my-flag", measurements.Tags["feature_flag.key"]);
        Assert.Equal("my-provider", measurements.Tags["feature_flag.provider.name"]);
        Assert.Equal("STATIC", measurements.Tags["feature_flag.result.reason"]);
        Assert.Equal("custom_dimension_value", measurements.Tags["custom_dimension_key"]);
    }

    [Fact]
    public async Task With_FlagMetadataCallback_After_Test()
    {
        // Arrange
        var metricHookOptions = MetricsHookOptions.CreateBuilder()
            .WithFlagEvaluationMetadata("bool", m => m.GetBool("bool"))
            .Build();

        var metricsHook = new MetricsHook(metricHookOptions);

        using var collector = new MetricCollector<long>(metricsHook._evaluationSuccessCounter);

        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        var flagMetadata = new ImmutableMetadata(new Dictionary<string, object>
        {
            { "bool", true }
        });

        // Act
        await metricsHook.AfterAsync(ctx,
            new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default", errorMessage: null, flagMetadata),
            new Dictionary<string, object>()).ConfigureAwait(true);

        var measurements = collector.LastMeasurement;

        // Assert
        Assert.NotNull(measurements);

        Assert.Equal("my-flag", measurements.Tags["feature_flag.key"]);
        Assert.Equal("my-provider", measurements.Tags["feature_flag.provider.name"]);
        Assert.Equal("STATIC", measurements.Tags["feature_flag.result.reason"]);
        Assert.Equal(true, measurements.Tags["bool"]);
    }

    [Fact]
    public async Task Error_Test()
    {
        // Arrange
        const string metricName = "feature_flag.evaluation_error_total";
        var metricsHook = new MetricsHook();
        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        await metricsHook.ErrorAsync(ctx, new Exception(), new Dictionary<string, object>()).ConfigureAwait(true);
        this._meterProvider.ForceFlush();

        // Assert metrics
        Assert.NotEmpty(this._exportedItems);

        // check if the metric is present in the exported items
        var metric = this._exportedItems.FirstOrDefault(m => m.Name == metricName);
        Assert.NotNull(metric);

        var noOtherMetric = this._exportedItems.All(m => m.Name == metricName);
        Assert.True(noOtherMetric);
    }

    [Fact]
    public async Task Finally_Test()
    {
        // Arrange
        const string metricName = "feature_flag.evaluation_active_count";
        var metricsHook = new MetricsHook();
        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);
        var evaluationDetails = new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default");

        // Act
        await metricsHook.FinallyAsync(ctx, evaluationDetails, new Dictionary<string, object>()).ConfigureAwait(true);
        this._meterProvider.ForceFlush();

        // Assert metrics
        Assert.NotEmpty(this._exportedItems);

        // check if the metric feature_flag.evaluation_success_total is present in the exported items
        var metric = this._exportedItems.FirstOrDefault(m => m.Name == metricName);
        Assert.NotNull(metric);

        var noOtherMetric = this._exportedItems.All(m => m.Name == metricName);
        Assert.True(noOtherMetric);
    }

    [Fact]
    public async Task Before_Test()
    {
        // Arrange
        const string metricName1 = "feature_flag.evaluation_active_count";
        const string metricName2 = "feature_flag.evaluation_requests_total";
        var metricsHook = new MetricsHook();
        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        await metricsHook.BeforeAsync(ctx, new Dictionary<string, object>()).ConfigureAwait(true);
        this._meterProvider.ForceFlush();

        // Assert metrics
        Assert.NotEmpty(this._exportedItems);

        // check if the metric is present in the exported items
        var metric1 = this._exportedItems.FirstOrDefault(m => m.Name == metricName1);
        Assert.NotNull(metric1);

        var metric2 = this._exportedItems.FirstOrDefault(m => m.Name == metricName2);
        Assert.NotNull(metric2);

        var noOtherMetric = this._exportedItems.All(m => m.Name == metricName1 || m.Name == metricName2);
        Assert.True(noOtherMetric);
    }
}
