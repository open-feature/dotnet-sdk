using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OpenFeature.Hooks;
using OpenFeature.Model;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Xunit;

namespace OpenFeature.Tests.Hooks;

[CollectionDefinition(nameof(TracingHookTest), DisableParallelization = true)]
public class TracingHookTest : IDisposable
{
    private readonly List<Activity> _exportedItems;
    private readonly TracerProvider _tracerProvider;
    private readonly Tracer _tracer;

    public TracingHookTest()
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
    public async Task TestAfter()
    {
        // Arrange
        var tracingHook = new TracingHook();
        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        var span = this._tracer.StartActiveSpan("my-span");
        await tracingHook.AfterAsync(ctx,
            new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default"),
            new Dictionary<string, object>()).ConfigureAwait(true); ;
        span.End();

        this._tracerProvider.ForceFlush();

        // Assert
        Assert.Single(this._exportedItems);
        var rootSpan = this._exportedItems.First();

        Assert.Single(rootSpan.Events);
        ActivityEvent ev = rootSpan.Events.First();
        Assert.Equal("feature_flag", ev.Name);

        Assert.Contains(new KeyValuePair<string, object?>("feature_flag.key", "my-flag"), ev.Tags);
        Assert.Contains(new KeyValuePair<string, object?>("feature_flag.variant", "default"), ev.Tags);
        Assert.Contains(new KeyValuePair<string, object?>("feature_flag.provider_name", "my-provider"), ev.Tags);
    }

    [Fact]
    public async Task TestAfter_NoSpan()
    {
        // Arrange
        var tracingHook = new TracingHook();
        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        await tracingHook.AfterAsync(ctx,
            new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default"),
            new Dictionary<string, object>()).ConfigureAwait(true); ;

        this._tracerProvider.ForceFlush();

        // Assert
        Assert.Empty(this._exportedItems);
    }

    [Fact]
    public async Task TestError()
    {
        // Arrange
        var tracingHook = new TracingHook();
        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        var span = this._tracer.StartActiveSpan("my-span");
        await tracingHook.ErrorAsync(ctx, new Exception("unexpected error"),
            new Dictionary<string, object>()).ConfigureAwait(true); ;
        span.End();

        this._tracerProvider.ForceFlush();

        // Assert
        Assert.Single(this._exportedItems);
        var rootSpan = this._exportedItems.First();

        Assert.Single(rootSpan.Events);
        var ev = rootSpan.Events.First();

        Assert.Equal("exception", ev.Name);

        Assert.Contains(new KeyValuePair<string, object?>("exception.message", "unexpected error"), ev.Tags);
    }

    [Fact]
    public async Task TestError_NoSpan()
    {
        // Arrange
        var tracingHook = new TracingHook();
        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        await tracingHook.ErrorAsync(ctx, new Exception("unexpected error"),
            new Dictionary<string, object>()).ConfigureAwait(true); ;

        this._tracerProvider.ForceFlush();

        // Assert
        Assert.Empty(this._exportedItems);
    }
}
