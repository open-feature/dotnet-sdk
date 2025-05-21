using Microsoft.AspNetCore.Mvc;
using OpenFeature;
using OpenFeature.DependencyInjection.Providers.Memory;
using OpenFeature.Hooks;
using OpenFeature.Providers.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddOpenFeature(builder =>
{
    builder.AddHostedFeatureLifecycle()
        .AddHook(sp => new LoggingHook(sp.GetRequiredService<ILogger<LoggingHook>>()))
        .AddInMemoryProvider("InMemory", provider => new Dictionary<string, Flag>()
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
