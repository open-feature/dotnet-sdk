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
        var tracingHook = new TraceEnricherHook();
        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        var span = this._tracer.StartActiveSpan("my-span");
        await tracingHook.FinallyAsync(ctx,
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
    public async Task TestFinally_NoSpan()
    {
        // Arrange
        var tracingHook = new TraceEnricherHook();
        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        await tracingHook.FinallyAsync(ctx,
            new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default"),
            new Dictionary<string, object>()).ConfigureAwait(true);

        this._tracerProvider.ForceFlush();

        // Assert
        Assert.Empty(this._exportedItems);
    }
}
