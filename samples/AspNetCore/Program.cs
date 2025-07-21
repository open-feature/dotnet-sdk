using Microsoft.AspNetCore.Mvc;
using OpenFeature;
using OpenFeature.DependencyInjection.Providers.Memory;
using OpenFeature.Hooks;
using OpenFeature.Providers.Memory;
using OpenFeature.Providers.MultiProvider;
using OpenFeature.Providers.MultiProvider.Models;
using OpenFeature.Providers.MultiProvider.Strategies;
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

app.MapGet("/multi-provider", async context =>
{
    // Create first in-memory provider with some flags
    var provider1Flags = new Dictionary<string, Flag>
    {
        { "providername", new Flag<string>(new Dictionary<string, string> { { "enabled", "enabled-provider1" }, { "disabled", "disabled-provider1" } }, "enabled") },
        { "max-items", new Flag<int>(new Dictionary<string, int> { { "low", 10 }, { "high", 100 } }, "high") },
    };
    var provider1 = new InMemoryProvider(provider1Flags);

    // Create second in-memory provider with different flags
    var provider2Flags = new Dictionary<string, Flag>
    {
        { "providername", new Flag<string>(new Dictionary<string, string> { { "enabled", "enabled-provider2" }, { "disabled", "disabled-provider2" } }, "enabled") },
    };
    var provider2 = new InMemoryProvider(provider2Flags);

    // Create provider entries
    var providerEntries = new List<ProviderEntry>
    {
        new(provider1, "Provider1"),
        new(provider2, "Provider2")
    };

    // Create multi-provider with FirstMatchStrategy (default)
    var multiProvider = new MultiProvider(providerEntries, new FirstMatchStrategy());

    // Set the multi-provider as the default provider using OpenFeature API
    await Api.Instance.SetProviderAsync(multiProvider);

    // Create a client directly using the API
    var client = Api.Instance.GetClient();

    try
    {
        // Test flag evaluation from different providers
        var maxItemsFlag = await client.GetIntegerDetailsAsync("max-items", 0);
        var providerNameFlag = await client.GetStringDetailsAsync("providername", "default");

        // Test a flag that doesn't exist in any provider
        var unknownFlag = await client.GetBooleanDetailsAsync("unknown-flag", false);

        var result = new
        {
            message = "Multi-provider evaluation results",
            results = new
            {
                maxItemsFlag = new { value = maxItemsFlag },
                providerNameFlag = new { value = providerNameFlag },
            }
        };

        await context.Response.WriteAsJsonAsync(result);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new
        {
            title = "Multi-provider evaluation failed",
            detail = ex.Message,
            status = 500
        });
    }
});

app.Run();
