# OpenFeature.Hosting

[![NuGet](https://img.shields.io/nuget/vpre/OpenFeature.Hosting)](https://www.nuget.org/packages/OpenFeature.Hosting)
[![Specification](https://img.shields.io/static/v1?label=specification&message=v0.8.0&color=yellow&style=for-the-badge)](https://github.com/open-feature/spec/releases/tag/v0.8.0)
[
![Release](https://img.shields.io/static/v1?label=release&message=v2.6.0&color=blue&style=for-the-badge) <!-- x-release-please-version -->
](https://github.com/open-feature/dotnet-sdk/releases/tag/v2.6.0) <!-- x-release-please-version -->

OpenFeature.Hosting provides hosting extensions for the OpenFeature .NET SDK, enabling seamless integration with .NET's generic host and dependency injection container. This package simplifies the setup and lifecycle management of feature flags in hosted .NET applications.

## Features

-   🚀 **Easy Integration**: Simple extension methods for `IHostBuilder` and `IServiceCollection`
-   🔄 **Lifecycle Management**: Automatic provider initialization and shutdown handling
-   🏗️ **Builder Pattern**: Fluent API for configuring OpenFeature with the hosting extensions
-   🎯 **Provider Lifecycle**: Proper startup and shutdown coordination with the host application
-   📦 **Minimal Dependencies**: Lightweight package with focus on hosting scenarios

## Quick Start

### Installation

```bash
dotnet add package OpenFeature.Hosting
```

### Basic Usage with Generic Host

```csharp
using Microsoft.Extensions.Hosting;
using OpenFeature.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureOpenFeature(builder =>
    {
        builder.AddProvider(new MyFeatureFlagProvider());
    })
    .Build();

await host.RunAsync();
```

### ASP.NET Core Integration

```csharp
using OpenFeature.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add OpenFeature with hosting support
builder.Host.ConfigureOpenFeature(openFeature =>
{
    openFeature.AddProvider(new MyFeatureFlagProvider())
               .SetEvaluationContext(new EvaluationContextBuilder()
                   .Set("environment", builder.Environment.EnvironmentName)
                   .Build());
});

var app = builder.Build();
app.Run();
```

## Core Concepts

### Host Configuration

The `ConfigureOpenFeature` extension method provides a convenient way to set up OpenFeature within the .NET hosting model:

```csharp
Host.CreateDefaultBuilder()
    .ConfigureOpenFeature(builder =>
    {
        // Configure providers, hooks, and evaluation context
        builder.AddProvider(new InMemoryProvider())
               .AddHook(new LoggingHook())
               .SetEvaluationContext(new EvaluationContextBuilder()
                   .Set("userId", "user-123")
                   .Build());
    })
    .Build();
```

### Hosted Feature Lifecycle Service

The package includes a `HostedFeatureLifecycleService` that ensures proper initialization and shutdown of OpenFeature providers:

-   **Startup**: Providers are initialized when the host starts
-   **Shutdown**: Providers are properly shut down when the host stops
-   **Error Handling**: Graceful handling of provider lifecycle errors

### Provider Registration

Multiple providers can be registered with different configurations:

```csharp
builder.Host.ConfigureOpenFeature(openFeature =>
{
    // Add multiple providers
    openFeature.AddProvider("provider1", new Provider1())
               .AddProvider("provider2", new Provider2());

    // Set default provider
    openFeature.SetProvider(new DefaultProvider());
});
```

## Advanced Configuration

### Environment-Specific Setup

```csharp
builder.Host.ConfigureOpenFeature(openFeature =>
{
    if (builder.Environment.IsDevelopment())
    {
        openFeature.AddProvider(new LocalFileProvider("dev-flags.json"));
    }
    else
    {
        openFeature.AddProvider(new RemoteProvider(configuration.GetConnectionString("FeatureFlags")));
    }
});
```

### Custom Hooks and Event Handlers

```csharp
builder.Host.ConfigureOpenFeature(openFeature =>
{
    openFeature.AddHook(new MetricsHook())
               .AddHook(new AuditHook());

    // Add event handlers
    Api.Instance.AddHandler(ProviderEventTypes.ProviderReady, args =>
    {
        logger.LogInformation("Provider {ProviderName} is ready", args.ProviderName);
    });
});
```

### Integration with Configuration

```csharp
builder.Host.ConfigureOpenFeature(openFeature =>
{
    var flagConfig = builder.Configuration.GetSection("FeatureFlags");

    openFeature.AddProvider(new ConfigurationProvider(flagConfig))
               .SetEvaluationContext(new EvaluationContextBuilder()
                   .Set("application", builder.Environment.ApplicationName)
                   .Set("version", Assembly.GetExecutingAssembly().GetName().Version?.ToString())
                   .Build());
});
```

## Working with Feature Flags

Once configured, you can inject and use the OpenFeature client throughout your application:

```csharp
public class MyService
{
    private readonly FeatureClient _featureClient;

    public MyService(FeatureClient featureClient)
    {
        _featureClient = featureClient;
    }

    public async Task<string> GetWelcomeMessageAsync()
    {
        var useNewWelcome = await _featureClient.GetBooleanValueAsync(
            "new-welcome-message",
            false);

        return useNewWelcome
            ? "Welcome to our amazing new experience!"
            : "Welcome!";
    }
}
```

## Background Services and Workers

OpenFeature.Hosting works seamlessly with background services:

```csharp
public class FlagAwareWorker : BackgroundService
{
    private readonly FeatureClient _featureClient;
    private readonly ILogger<FlagAwareWorker> _logger;

    public FlagAwareWorker(FeatureClient featureClient, ILogger<FlagAwareWorker> logger)
    {
        _featureClient = featureClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var processingEnabled = await _featureClient.GetBooleanValueAsync(
                "background-processing-enabled",
                true);

            if (processingEnabled)
            {
                // Perform background work
                _logger.LogInformation("Processing background task");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}

// Registration
builder.Services.AddHostedService<FlagAwareWorker>();
```

## Health Checks Integration

Monitor the health of your feature flag providers:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<OpenFeatureHealthCheck>("openfeature");

builder.Host.ConfigureOpenFeature(openFeature =>
{
    openFeature.AddProvider(new MyProvider());
});
```

## Best Practices

### 1. Provider Initialization Order

Configure providers in order of preference, with the most reliable provider last:

```csharp
openFeature.AddProvider("cache", new CacheProvider())
           .AddProvider("remote", new RemoteProvider())
           .SetProvider(new FallbackProvider()); // Default fallback
```

### 2. Graceful Degradation

Always provide sensible defaults for feature flags:

```csharp
var featureEnabled = await _featureClient.GetBooleanValueAsync(
    "experimental-feature",
    defaultValue: false, // Safe default
    context: evaluationContext);
```

### 3. Context Management

Use evaluation context to provide relevant information for flag evaluation:

```csharp
var context = new EvaluationContextBuilder()
    .Set("userId", currentUser.Id)
    .Set("plan", currentUser.SubscriptionPlan)
    .Set("region", currentUser.Region)
    .Build();

var result = await _featureClient.GetBooleanValueAsync(
    "premium-feature",
    false,
    context);
```

## Error Handling

The hosting extensions provide robust error handling:

```csharp
builder.Host.ConfigureOpenFeature(openFeature =>
{
    openFeature.AddProvider(new ResilientProvider())
               .AddHook(new ErrorHandlingHook());

    // Providers that fail to initialize won't prevent host startup
});
```

## Migration Guide

### From Manual Configuration

**Before:**

```csharp
// Manual setup in Program.cs
var provider = new MyProvider();
await Api.Instance.SetProviderAsync(provider);
```

**After:**

```csharp
// Hosting integration
builder.Host.ConfigureOpenFeature(openFeature =>
{
    openFeature.AddProvider(new MyProvider());
});
```

## Troubleshooting

### Common Issues

1. **Provider Not Initialized**: Ensure providers are added before the host starts
2. **Service Not Found**: Make sure OpenFeature.DependencyInjection is also installed for service registration
3. **Lifecycle Issues**: Check that HostedFeatureLifecycleService is properly registered

### Debug Logging

Enable debug logging to troubleshoot issues:

```csharp
builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Debug);

builder.Host.ConfigureOpenFeature(openFeature =>
{
    openFeature.AddHook(new DebugLoggingHook());
});
```

## Requirements

-   .NET 6.0 or later
-   Microsoft.Extensions.Hosting 6.0.0 or later
-   OpenFeature SDK 1.0.0 or later

## Related Packages

-   **[OpenFeature](https://www.nuget.org/packages/OpenFeature/)**: Core OpenFeature SDK
-   **[OpenFeature.DependencyInjection](https://www.nuget.org/packages/OpenFeature.DependencyInjection/)**: Dependency injection extensions

## Contributing

We welcome contributions! Please see our [contributing guide](https://github.com/open-feature/dotnet-sdk/blob/main/CONTRIBUTING.md) for details.

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](https://github.com/open-feature/dotnet-sdk/blob/main/LICENSE) file for details.
