using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using OpenFeature.Hooks;
using OpenFeature.Model;

namespace OpenFeature.Tests.Hooks;

[CollectionDefinition(nameof(MetricsHookTest), DisableParallelization = true)]
public class MetricsHookTest
{
    [Fact]
    public async Task After_Test()
    {
        // Arrange
        var metricsHook = new MetricsHook();

        using var collector = new MetricCollector<long>(metricsHook._evaluationSuccessCounter);

        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        await metricsHook.AfterAsync(ctx, new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default"), new Dictionary<string, object>(), TestContext.Current.CancellationToken).ConfigureAwait(true);

        var measurements = collector.LastMeasurement;

        // Assert
        Assert.NotNull(measurements);

        Assert.Equal("my-flag", measurements.Tags["feature_flag.key"]);
        Assert.Equal("my-provider", measurements.Tags["feature_flag.provider.name"]);
        Assert.Equal("STATIC", measurements.Tags["feature_flag.result.reason"]);
    }

    [Fact]
    public async Task Without_Reason_After_Test_Defaults_To_Unknown()
    {
        // Arrange
        var metricsHook = new MetricsHook();

        using var collector = new MetricCollector<long>(metricsHook._evaluationSuccessCounter);

        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        await metricsHook.AfterAsync(ctx, new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, reason: null, "default"), new Dictionary<string, object>(), TestContext.Current.CancellationToken).ConfigureAwait(true);

        var measurements = collector.LastMeasurement;

        // Assert
        Assert.NotNull(measurements);

        Assert.Equal("my-flag", measurements.Tags["feature_flag.key"]);
        Assert.Equal("my-provider", measurements.Tags["feature_flag.provider.name"]);
        Assert.Equal("UNKNOWN", measurements.Tags["feature_flag.result.reason"]);
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
        await metricsHook.AfterAsync(ctx, new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default"), new Dictionary<string, object>(), TestContext.Current.CancellationToken).ConfigureAwait(true);

        var measurements = collector.LastMeasurement;

        // Assert
        Assert.NotNull(measurements);

        Assert.Equal(1, measurements.Value);
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
        await metricsHook.AfterAsync(ctx, new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default", errorMessage: null, flagMetadata), new Dictionary<string, object>(), TestContext.Current.CancellationToken).ConfigureAwait(true);

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
        var metricsHook = new MetricsHook();

        using var collector = new MetricCollector<long>(metricsHook._evaluationErrorCounter);

        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        var errorMessage = "An error occurred during evaluation";

        // Act
        await metricsHook.ErrorAsync(ctx, new Exception(errorMessage), new Dictionary<string, object>(), TestContext.Current.CancellationToken).ConfigureAwait(true);

        var measurements = collector.LastMeasurement;

        // Assert
        Assert.NotNull(measurements);

        Assert.Equal("my-flag", measurements.Tags["feature_flag.key"]);
        Assert.Equal("my-provider", measurements.Tags["feature_flag.provider.name"]);
        Assert.Equal(errorMessage, measurements.Tags["exception"]);
    }

    [Fact]
    public async Task With_CustomDimensions_Error_Test()
    {
        // Arrange
        var metricHookOptions = MetricsHookOptions.CreateBuilder()
            .WithCustomDimension("custom_dimension_key", "custom_dimension_value")
            .Build();

        var metricsHook = new MetricsHook(metricHookOptions);

        using var collector = new MetricCollector<long>(metricsHook._evaluationErrorCounter);

        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        var errorMessage = "An error occurred during evaluation";

        // Act
        await metricsHook.ErrorAsync(ctx, new Exception(errorMessage), new Dictionary<string, object>(), TestContext.Current.CancellationToken).ConfigureAwait(true);

        var measurements = collector.LastMeasurement;

        // Assert
        Assert.NotNull(measurements);

        Assert.Equal("my-flag", measurements.Tags["feature_flag.key"]);
        Assert.Equal("my-provider", measurements.Tags["feature_flag.provider.name"]);
        Assert.Equal(errorMessage, measurements.Tags["exception"]);
        Assert.Equal("custom_dimension_value", measurements.Tags["custom_dimension_key"]);
    }

    [Fact]
    public async Task Finally_Test()
    {
        // Arrange
        var metricsHook = new MetricsHook();

        using var collector = new MetricCollector<long>(metricsHook._evaluationActiveUpDownCounter);

        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);
        var evaluationDetails = new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default");

        // Act
        await metricsHook.FinallyAsync(ctx, evaluationDetails, new Dictionary<string, object>(), TestContext.Current.CancellationToken).ConfigureAwait(true);

        var measurements = collector.LastMeasurement;

        // Assert
        Assert.NotNull(measurements);

        Assert.Equal(-1, measurements.Value);
        Assert.Equal("my-flag", measurements.Tags["feature_flag.key"]);
        Assert.Equal("my-provider", measurements.Tags["feature_flag.provider.name"]);
    }

    [Fact]
    public async Task With_CustomDimensions_Finally_Test()
    {
        // Arrange
        var metricHookOptions = MetricsHookOptions.CreateBuilder()
            .WithCustomDimension("custom_dimension_key", "custom_dimension_value")
            .Build();

        var metricsHook = new MetricsHook(metricHookOptions);

        using var collector = new MetricCollector<long>(metricsHook._evaluationActiveUpDownCounter);

        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);
        var evaluationDetails = new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default");

        // Act
        await metricsHook.FinallyAsync(ctx, evaluationDetails, new Dictionary<string, object>(), TestContext.Current.CancellationToken).ConfigureAwait(true);

        var measurements = collector.LastMeasurement;

        // Assert
        Assert.NotNull(measurements);

        Assert.Equal(-1, measurements.Value);
        Assert.Equal("my-flag", measurements.Tags["feature_flag.key"]);
        Assert.Equal("my-provider", measurements.Tags["feature_flag.provider.name"]);
        Assert.Equal("custom_dimension_value", measurements.Tags["custom_dimension_key"]);
    }

    [Fact]
    public async Task With_FlagMetadataCallback_Finally_Test()
    {
        // Arrange
        var metricHookOptions = MetricsHookOptions.CreateBuilder()
            .WithFlagEvaluationMetadata("status_code", m => m.GetInt("status_code"))
            .Build();

        var metricsHook = new MetricsHook(metricHookOptions);

        using var collector = new MetricCollector<long>(metricsHook._evaluationActiveUpDownCounter);

        var flagMetadata = new ImmutableMetadata(new Dictionary<string, object>
        {
            { "status_code", 1521 }
        });

        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);
        var evaluationDetails = new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default", flagMetadata: flagMetadata);

        // Act
        await metricsHook.FinallyAsync(ctx, evaluationDetails, new Dictionary<string, object>(), TestContext.Current.CancellationToken).ConfigureAwait(true);

        var measurements = collector.LastMeasurement;

        // Assert
        Assert.NotNull(measurements);

        Assert.Equal(-1, measurements.Value);
        Assert.Equal("my-flag", measurements.Tags["feature_flag.key"]);
        Assert.Equal("my-provider", measurements.Tags["feature_flag.provider.name"]);
        Assert.Equal(1521, measurements.Tags["status_code"]);
    }

    [Fact]
    public async Task Before_Test()
    {
        // Arrange
        var metricsHook = new MetricsHook();

        using var collector1 = new MetricCollector<long>(metricsHook._evaluationActiveUpDownCounter);
        using var collector2 = new MetricCollector<long>(metricsHook._evaluationRequestCounter);

        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        await metricsHook.BeforeAsync(ctx, new Dictionary<string, object>(), TestContext.Current.CancellationToken).ConfigureAwait(true);

        var measurements = collector1.LastMeasurement;

        // Assert
        Assert.NotNull(measurements);

        Assert.Equal(1, measurements.Value);
        Assert.Equal("my-flag", measurements.Tags["feature_flag.key"]);
        Assert.Equal("my-provider", measurements.Tags["feature_flag.provider.name"]);
    }

    [Fact]
    public async Task With_CustomDimensions_Before_Test()
    {
        // Arrange
        var metricHookOptions = MetricsHookOptions.CreateBuilder()
            .WithCustomDimension("custom_dimension_key", "custom_dimension_value")
            .Build();

        var metricsHook = new MetricsHook(metricHookOptions);

        using var collector1 = new MetricCollector<long>(metricsHook._evaluationActiveUpDownCounter);
        using var collector2 = new MetricCollector<long>(metricsHook._evaluationRequestCounter);

        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        await metricsHook.BeforeAsync(ctx, new Dictionary<string, object>(), TestContext.Current.CancellationToken).ConfigureAwait(true);

        var measurements = collector1.LastMeasurement;

        // Assert
        Assert.NotNull(measurements);

        Assert.Equal(1, measurements.Value);
        Assert.Equal("my-flag", measurements.Tags["feature_flag.key"]);
        Assert.Equal("my-provider", measurements.Tags["feature_flag.provider.name"]);
        Assert.Equal("custom_dimension_value", measurements.Tags["custom_dimension_key"]);
    }

    [Fact]
    public async Task ActiveCounter_Before_And_Finally_Have_Same_Tags_Test()
    {
        // Arrange
        var metricsHookOptions = MetricsHookOptions.CreateBuilder()
            .WithCustomDimension("custom_dimension_key", "custom_dimension_value")
            .WithFlagEvaluationMetadata("boolean", m => m.GetBool("boolean"))
            .Build();

        var metricsHook = new MetricsHook(metricsHookOptions);

        using var collector = new MetricCollector<long>(metricsHook._evaluationActiveUpDownCounter);

        var flagMetadata = new ImmutableMetadata(new Dictionary<string, object> { { "boolean", true } });

        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);
        var evaluationDetails = new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default", flagMetadata: flagMetadata);

        // Act
        await metricsHook.BeforeAsync(ctx, new Dictionary<string, object>()).ConfigureAwait(true);
        await metricsHook.FinallyAsync(ctx, evaluationDetails, new Dictionary<string, object>()).ConfigureAwait(true);

        var measurements = collector.GetMeasurementSnapshot();

        // Assert
        Assert.NotNull(measurements);
        Assert.Equal(2, measurements.Count);
        Assert.Equal(0, measurements.Sum(m => m.Value));

        var beforeTagKeys = measurements[0].Tags.Keys.OrderBy(k => k).ToList();
        var finallyTagKeys = measurements[1].Tags.Keys.OrderBy(k => k).ToList();

        Assert.Equal(beforeTagKeys, finallyTagKeys);
    }

    [Fact]
    public async Task ActiveCounter_With_FlagMetadata_Before_Has_Null_Metadata_Test()
    {
        // Arrange
        var metricsHookOptions = MetricsHookOptions.CreateBuilder()
            .WithFlagEvaluationMetadata("boolean", m => m.GetBool("boolean"))
            .Build();

        var metricsHook = new MetricsHook(metricsHookOptions);

        using var collector = new MetricCollector<long>(metricsHook._evaluationActiveUpDownCounter);

        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act - call BeforeAsync (no flag metadata available yet)
        await metricsHook.BeforeAsync(ctx, new Dictionary<string, object>()).ConfigureAwait(true);

        var measurements = collector.LastMeasurement;

        // Assert - should handle null metadata gracefully
        Assert.NotNull(measurements);
        Assert.Equal(1, measurements.Value);
        Assert.Equal("my-flag", measurements.Tags["feature_flag.key"]);
        Assert.Equal("my-provider", measurements.Tags["feature_flag.provider.name"]);
        Assert.Contains("boolean", measurements.Tags.Keys);
        Assert.Null(measurements.Tags["boolean"]);
    }
}
