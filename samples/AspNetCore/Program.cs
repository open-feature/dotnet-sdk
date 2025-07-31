using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using OpenFeature;
using OpenFeature.DependencyInjection.Providers.Memory;
using OpenFeature.Hooks;
using OpenFeature.Model;
using OpenFeature.Providers.Memory;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateSlimBuilder(args);

// Add services to the container.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddProblemDetails();

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("openfeature-aspnetcore-sample"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddMeter("OpenFeature")
        .AddOtlpExporter());

builder.Services.AddOpenFeature(featureBuilder =>
{
    var metricsHookOptions = MetricsHookOptions.CreateBuilder()
        .WithCustomDimension("custom_dimension_key", "custom_dimension_value")
        .WithFlagEvaluationMetadata("boolean", s => s.GetBool("boolean"))
        .Build();

    featureBuilder.AddHostedFeatureLifecycle()
        .AddHook(sp => new LoggingHook(sp.GetRequiredService<ILogger<LoggingHook>>()))
        .AddHook(_ => new MetricsHook(metricsHookOptions))
        .AddHook<TraceEnricherHook>()
        .AddInMemoryProvider("InMemory", _ => new Dictionary<string, Flag>()
        {
            {
                "welcome-message", new Flag<bool>(
                    new Dictionary<string, bool> { { "show", true }, { "hide", false } }, "show")
            },
            {
                "test-config", new Flag<Value>(new Dictionary<string, Value>()
                {
                    { "enable", new Value(Structure.Builder().Set(nameof(TestConfig.Threshold), 100).Build()) },
                    { "half", new Value(Structure.Builder().Set(nameof(TestConfig.Threshold), 50).Build()) },
                    { "disable", new Value(Structure.Builder().Set(nameof(TestConfig.Threshold), 0).Build()) }
                }, "disable")
            }
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapGet("/welcome", async ([FromServices] IFeatureClient featureClient) =>
{
    var welcomeMessageEnabled = await featureClient.GetBooleanValueAsync("welcome-message", false);

    if (welcomeMessageEnabled)
    {
        return TypedResults.Ok("Hello world! The welcome-message feature flag was enabled!");
    }

    return TypedResults.Ok("Hello world!");
});
app.MapGet("/test-config", async ([FromServices] IFeatureClient featureClient) =>
{
    var testConfigValue = await featureClient.GetObjectValueAsync("test-config",
            new Value(Structure.Builder().Set("Threshold", 50).Build())
        );
    var json = JsonSerializer.Serialize(testConfigValue, AppJsonSerializerContext.Default.Value);
    var config = JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.TestConfig);
    return Results.Ok(config);
});

app.Run();


public class TestConfig
{
    public int Threshold { get; set; } = 10;
}

[JsonSerializable(typeof(TestConfig))]
[JsonSerializable(typeof(Value))]
public partial class AppJsonSerializerContext : JsonSerializerContext;
