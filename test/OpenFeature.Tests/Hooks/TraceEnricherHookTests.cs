using System.Diagnostics;
using OpenFeature.Hooks;
using OpenFeature.Model;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OpenFeature.Tests.Hooks;

[CollectionDefinition(nameof(TraceEnricherHookTests), DisableParallelization = true)]
public class TraceEnricherHookTests : IDisposable
{
    private readonly List<Activity> _exportedItems;
    private readonly TracerProvider _tracerProvider;
    private readonly Tracer _tracer;

    public TraceEnricherHookTests()
    {
        // List that will be populated with the traces by InMemoryExporter
        this._exportedItems = [];

        // Create a new in-memory exporter
        this._tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("my-tracer")
            .ConfigureResource(r => r.AddService("in-memory-test"))
            .AddInMemoryExporter(this._exportedItems)
            .Build();

        this._tracer = this._tracerProvider.GetTracer("my-tracer");
    }

#pragma warning disable CA1816
    public void Dispose()
    {
        this._tracerProvider.Shutdown();
    }
#pragma warning restore CA1816

    [Fact]
    public async Task TestFinally()
    {
        // Arrange
        var traceEnricherHook = new TraceEnricherHook();
        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        var span = this._tracer.StartActiveSpan("my-span");
        await traceEnricherHook.FinallyAsync(ctx,
            new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default"),
            new Dictionary<string, object>()).ConfigureAwait(true);
        span.End();

        this._tracerProvider.ForceFlush();

        // Assert
        Assert.Single(this._exportedItems);
        var rootSpan = this._exportedItems.First();

        Assert.Single(rootSpan.Events);
        ActivityEvent ev = rootSpan.Events.First();
        Assert.Equal("feature_flag.evaluation", ev.Name);

        Assert.Contains(new KeyValuePair<string, object?>("feature_flag.key", "my-flag"), ev.Tags);
        Assert.Contains(new KeyValuePair<string, object?>("feature_flag.result.variant", "default"), ev.Tags);
        Assert.Contains(new KeyValuePair<string, object?>("feature_flag.provider.name", "my-provider"), ev.Tags);
        Assert.Contains(new KeyValuePair<string, object?>("feature_flag.result.reason", "static"), ev.Tags);
        Assert.Contains(new KeyValuePair<string, object?>("feature_flag.result.value", "foo"), ev.Tags);
    }

    [Fact]
    public async Task TestFinally_WithCustomDimension()
    {
        // Arrange
        var traceHookOptions = TraceEnricherHookOptions.CreateBuilder()
            .WithCustomDimension("custom_dimension_key", "custom_dimension_value")
            .Build();
        var traceEnricherHook = new TraceEnricherHook(traceHookOptions);
        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        var span = this._tracer.StartActiveSpan("my-span");
        await traceEnricherHook.FinallyAsync(ctx,
            new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default"),
            new Dictionary<string, object>()).ConfigureAwait(true);
        span.End();

        this._tracerProvider.ForceFlush();

        // Assert
        Assert.Single(this._exportedItems);
        var rootSpan = this._exportedItems.First();

        Assert.Single(rootSpan.Events);
        ActivityEvent ev = rootSpan.Events.First();
        Assert.Equal("feature_flag.evaluation", ev.Name);

        Assert.Contains(new KeyValuePair<string, object?>("custom_dimension_key", "custom_dimension_value"), ev.Tags);
    }

    [Fact]
    public async Task TestFinally_WithFlagEvaluationMetadata()
    {
        // Arrange
        var traceHookOptions = TraceEnricherHookOptions.CreateBuilder()
            .WithFlagEvaluationMetadata("double", metadata => metadata.GetDouble("double"))
            .WithFlagEvaluationMetadata("int", metadata => metadata.GetInt("int"))
            .WithFlagEvaluationMetadata("bool", metadata => metadata.GetBool("bool"))
            .WithFlagEvaluationMetadata("string", metadata => metadata.GetString("string"))
            .Build();
        var traceEnricherHook = new TraceEnricherHook(traceHookOptions);
        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        var flagMetadata = new ImmutableMetadata(new Dictionary<string, object>
        {
            { "double", 1.0 },
            { "int", 2025 },
            { "bool", true },
            { "string", "foo" }
        });

        // Act
        var span = this._tracer.StartActiveSpan("my-span");
        await traceEnricherHook.FinallyAsync(ctx,
            new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default", flagMetadata: flagMetadata),
            new Dictionary<string, object>()).ConfigureAwait(true);
        span.End();

        this._tracerProvider.ForceFlush();

        // Assert
        Assert.Single(this._exportedItems);
        var rootSpan = this._exportedItems.First();

        Assert.Single(rootSpan.Events);
        ActivityEvent ev = rootSpan.Events.First();
        Assert.Equal("feature_flag.evaluation", ev.Name);

        Assert.Contains(new KeyValuePair<string, object?>("double", 1.0), ev.Tags);
        Assert.Contains(new KeyValuePair<string, object?>("int", 2025), ev.Tags);
        Assert.Contains(new KeyValuePair<string, object?>("bool", true), ev.Tags);
        Assert.Contains(new KeyValuePair<string, object?>("string", "foo"), ev.Tags);
    }

    [Fact]
    public async Task TestFinally_NoSpan()
    {
        // Arrange
        var traceEnricherHook = new TraceEnricherHook();
        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        await traceEnricherHook.FinallyAsync(ctx,
            new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default"),
            new Dictionary<string, object>()).ConfigureAwait(true);

        this._tracerProvider.ForceFlush();

        // Assert
        Assert.Empty(this._exportedItems);
    }
}
