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

public class TracingHookTest
{
    [Fact]
    public async Task TestAfter()
    {
        // Arrange
        // List that will be populated with the traces by InMemoryExporter
        var exportedItems = new List<Activity>();

        // Create a new in-memory exporter
        var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("my-tracer")
            .ConfigureResource(r => r.AddService("in-memory-test"))
            .AddInMemoryExporter(exportedItems)
            .Build();

        var tracer = tracerProvider.GetTracer("my-tracer");

        var tracingHook = new TracingHook();
        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        var span = tracer.StartActiveSpan("my-span");
        await tracingHook.AfterAsync(ctx,
            new FlagEvaluationDetails<string>("my-flag", "foo", Constant.ErrorType.None, "STATIC", "default"),
            new Dictionary<string, object>());
        span.End();

        tracerProvider.ForceFlush();

        // Assert
        Assert.Single(exportedItems);
        var rootSpan = exportedItems.First();

        Assert.Single(rootSpan.Events);
        ActivityEvent ev = rootSpan.Events.First();
        Assert.Equal("feature_flag", ev.Name);

        Assert.Contains(new KeyValuePair<string, object?>("feature_flag.key", "my-flag"), ev.Tags);
        Assert.Contains(new KeyValuePair<string, object?>("feature_flag.variant", "default"), ev.Tags);
        Assert.Contains(new KeyValuePair<string, object?>("feature_flag.provider_name", "my-provider"), ev.Tags);
    }

    [Fact]
    public async Task TestError()
    {
        // Arrange
        // List that will be populated with the traces by InMemoryExporter
        var exportedItems = new List<Activity>();

        // Create a new in-memory exporter
        var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("my-tracer")
            .ConfigureResource(r => r.AddService("in-memory-test"))
            .AddInMemoryExporter(exportedItems)
            .Build();

        var tracer = tracerProvider.GetTracer("my-tracer");

        var tracingHook = new TracingHook();
        var evaluationContext = EvaluationContext.Empty;
        var ctx = new HookContext<string>("my-flag", "foo", Constant.FlagValueType.String,
            new ClientMetadata("my-client", "1.0"), new Metadata("my-provider"), evaluationContext);

        // Act
        var span = tracer.StartActiveSpan("my-span");
        await tracingHook.ErrorAsync(ctx, new System.Exception("unexpected error"),
            new Dictionary<string, object>());
        span.End();

        tracerProvider.ForceFlush();

        // Assert
        Assert.Single(exportedItems);
        var rootSpan = exportedItems.First();

        Assert.Single(rootSpan.Events);
        var ev = rootSpan.Events.First();

        Assert.Equal("exception", ev.Name);

        Assert.Contains(new KeyValuePair<string, object?>("exception.message", "unexpected error"), ev.Tags);
    }
}
