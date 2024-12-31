using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenFeature.Hooks;
using OpenFeature.Model;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Xunit;

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
            .AddMeter("*")
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

        // Act
        await metricsHook.FinallyAsync(ctx, new Dictionary<string, object>()).ConfigureAwait(true);
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
