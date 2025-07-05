using Microsoft.AspNetCore.Mvc;
using OpenFeature;
using OpenFeature.DependencyInjection.Providers.Memory;
using OpenFeature.Hooks;
using OpenFeature.Providers.Memory;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

app.Run();
