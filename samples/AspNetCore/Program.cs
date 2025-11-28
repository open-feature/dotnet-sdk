using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using OpenFeature;
using OpenFeature.Hooks;
using OpenFeature.Hosting.Providers.Memory;
using OpenFeature.Model;
using OpenFeature.Providers.Memory;
using OpenFeature.Providers.MultiProvider;
using OpenFeature.Providers.MultiProvider.DependencyInjection;
using OpenFeature.Providers.MultiProvider.Models;
using OpenFeature.Providers.MultiProvider.Strategies;
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

    featureBuilder
        .AddHook(sp => new LoggingHook(sp.GetRequiredService<ILogger<LoggingHook>>()))
        .AddHook(_ => new MetricsHook(metricsHookOptions))
        .AddHook<TraceEnricherHook>()
        .AddMultiProvider("multi-provider", multiProviderBuilder =>
        {
            // Create provider flags
            var provider1Flags = new Dictionary<string, Flag>
            {
                { "providername", new Flag<string>(new Dictionary<string, string> { { "enabled", "enabled-provider1" }, { "disabled", "disabled-provider1" } }, "enabled") },
                { "max-items", new Flag<int>(new Dictionary<string, int> { { "low", 10 }, { "high", 100 } }, "high") },
            };

            var provider2Flags = new Dictionary<string, Flag>
            {
                { "providername", new Flag<string>(new Dictionary<string, string> { { "enabled", "enabled-provider2" }, { "disabled", "disabled-provider2" } }, "enabled") },
            };

            // Use the factory pattern to create providers - they will be properly initialized
            multiProviderBuilder
                .AddProvider("p1", sp => new InMemoryProvider(provider1Flags))
                .AddProvider("p2", sp => new InMemoryProvider(provider2Flags))
                .UseStrategy<FirstMatchStrategy>();
        })
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
        })
        .AddPolicyName(policy => policy.DefaultNameSelector = provider => "InMemory");
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

app.MapGet("/multi-provider", async () =>
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

        return Results.Ok();
    }
    catch (Exception)
    {
        return Results.InternalServerError();
    }
});

app.MapGet("/multi-provider-di", async ([FromKeyedServices("multi-provider")] IFeatureClient featureClient) =>
{
    try
    {
        // Test flag evaluation from different providers
        var maxItemsFlag = await featureClient.GetIntegerDetailsAsync("max-items", 0);
        var providerNameFlag = await featureClient.GetStringDetailsAsync("providername", "default");

        // Test a flag that doesn't exist in any provider
        var unknownFlag = await featureClient.GetBooleanDetailsAsync("unknown-flag", false);

        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error: {ex.Message}\n\nStack: {ex.StackTrace}");
    }
});

app.Run();


public class TestConfig
{
    public int Threshold { get; set; } = 10;
}

[JsonSerializable(typeof(TestConfig))]
[JsonSerializable(typeof(Value))]
public partial class AppJsonSerializerContext : JsonSerializerContext;
