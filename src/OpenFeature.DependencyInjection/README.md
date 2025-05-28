# OpenFeature.DependencyInjection

[![NuGet](https://img.shields.io/nuget/vpre/OpenFeature.DependencyInjection)](https://www.nuget.org/packages/OpenFeature.DependencyInjection)
[![Specification](https://img.shields.io/static/v1?label=specification&message=v0.8.0&color=yellow&style=for-the-badge)](https://github.com/open-feature/spec/releases/tag/v0.8.0)
[
![Release](https://img.shields.io/static/v1?label=release&message=v2.6.0&color=blue&style=for-the-badge) <!-- x-release-please-version -->
](https://github.com/open-feature/dotnet-sdk/releases/tag/v2.6.0) <!-- x-release-please-version -->

OpenFeature.DependencyInjection provides seamless integration of OpenFeature with .NET's built-in dependency injection container. This package simplifies the configuration and lifecycle management of feature flag providers, evaluation contexts, hooks, and handlers in modern .NET applications.

> [!NOTE]
> This package is currently experimental. It streamlines the integration of OpenFeature within .NET applications, allowing for seamless configuration and lifecycle management of feature flag providers using dependency injection and hosting services.

## 🚀 Quick start

### Requirements

-   .NET 8+
-   .NET Framework 4.6.2+

### Installation

Install both the DependencyInjection and Hosting packages for complete functionality:

```sh
dotnet add package OpenFeature.DependencyInjection
dotnet add package OpenFeature.Hosting
```

### Basic Usage

Configure OpenFeature in your application's service collection:

```csharp
using OpenFeature;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenFeature(featureBuilder =>
{
    featureBuilder
        .AddHostedFeatureLifecycle() // From Hosting package
        .AddInMemoryProvider();
});

var app = builder.Build();
```

Use the injected `IFeatureClient` in your services:

```csharp
app.MapGet("/api/features", async ([FromServices] IFeatureClient featureClient) =>
{
    var isEnabled = await featureClient.GetBooleanValueAsync("my-feature", false);
    return new { FeatureEnabled = isEnabled };
});
```

## 📖 Core Concepts

### OpenFeatureBuilder

The `OpenFeatureBuilder` is the main configuration interface that allows you to:

-   Register feature flag providers (default and domain-scoped)
-   Configure evaluation contexts
-   Add hooks and event handlers
-   Set up provider selection policies

### Provider Registration

#### Default Provider

Register a single provider for the entire application:

```csharp
builder.Services.AddOpenFeature(featureBuilder =>
{
    featureBuilder.AddInMemoryProvider();
});
```

#### Domain-Scoped Providers

Register multiple providers with specific names for different domains:

```csharp
builder.Services.AddOpenFeature(featureBuilder =>
{
    featureBuilder
        .AddInMemoryProvider("memory-provider")
        .AddProvider("external-provider", provider => new ExternalProvider())
        .AddPolicyName(options =>
        {
            options.DefaultNameSelector = provider =>
            {
                // Logic to select provider based on context
                return "memory-provider";
            };
        });
});
```

#### Provider with Options

Register providers with configuration options:

```csharp
public class MyProviderOptions : OpenFeatureOptions
{
    public string ApiKey { get; set; }
    public string Environment { get; set; }
}

builder.Services.AddOpenFeature(featureBuilder =>
{
    featureBuilder.AddProvider<MyProviderOptions>(
        provider => new MyProvider(provider.GetRequiredService<IOptions<MyProviderOptions>>().Value),
        options =>
        {
            options.ApiKey = "your-api-key";
            options.Environment = "production";
        });
});
```

### Evaluation Context

Configure global evaluation context that will be merged with request-specific contexts:

```csharp
builder.Services.AddOpenFeature(featureBuilder =>
{
    featureBuilder
        .AddContext(contextBuilder =>
        {
            contextBuilder
                .Set("environment", "production")
                .Set("version", "1.0.0");
        })
        .AddInMemoryProvider();
});
```

#### Dynamic Context with Service Provider

Access other services when building the evaluation context:

```csharp
builder.Services.AddOpenFeature(featureBuilder =>
{
    featureBuilder
        .AddContext((contextBuilder, serviceProvider) =>
        {
            var httpContext = serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext;
            if (httpContext != null)
            {
                contextBuilder
                    .Set("userId", httpContext.User.Identity?.Name ?? "anonymous")
                    .Set("userAgent", httpContext.Request.Headers.UserAgent.ToString());
            }
        })
        .AddInMemoryProvider();
});
```

### Hooks

Add global hooks that will be executed for all feature flag evaluations:

```csharp
public class LoggingHook : Hook
{
    private readonly ILogger<LoggingHook> _logger;

    public LoggingHook(ILogger<LoggingHook> logger)
    {
        _logger = logger;
    }

    public override ValueTask<EvaluationContext> BeforeAsync<T>(
        HookContext<T> context,
        IReadOnlyDictionary<string, object>? hints = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Evaluating flag: {FlagKey}", context.FlagKey);
        return base.BeforeAsync(context, hints, cancellationToken);
    }
}

// Register the hook
builder.Services.AddOpenFeature(featureBuilder =>
{
    featureBuilder
        .AddHook<LoggingHook>()
        .AddInMemoryProvider();
});
```

#### Custom Hook Registration

Register hooks with custom names or factory methods:

```csharp
builder.Services.AddOpenFeature(featureBuilder =>
{
    featureBuilder
        .AddHook("custom-logging", serviceProvider =>
            new LoggingHook(serviceProvider.GetRequiredService<ILogger<LoggingHook>>()))
        .AddInMemoryProvider();
});
```

### Event Handlers

Register handlers for provider lifecycle events:

```csharp
using OpenFeature.Constant;

builder.Services.AddOpenFeature(featureBuilder =>
{
    featureBuilder
        .AddHandler(ProviderEventTypes.ProviderReady, eventDetails =>
        {
            Console.WriteLine($"Provider {eventDetails.ProviderName} is ready");
        })
        .AddHandler(ProviderEventTypes.ProviderError, eventDetails =>
        {
            Console.WriteLine($"Provider {eventDetails.ProviderName} error: {eventDetails.Message}");
        })
        .AddInMemoryProvider();
});
```

## 🔧 Advanced Configuration

### Multiple Providers with Policy

When using multiple providers, you must configure a policy to determine which provider to use:

```csharp
builder.Services.AddOpenFeature(featureBuilder =>
{
    featureBuilder
        .AddInMemoryProvider("local")
        .AddProvider("remote", provider => new RemoteProvider())
        .AddPolicyName(options =>
        {
            options.DefaultNameSelector = serviceProvider =>
            {
                var environment = serviceProvider.GetService<IHostEnvironment>();
                return environment?.IsDevelopment() == true ? "local" : "remote";
            };
        });
});
```

### Validation

The OpenFeature DI system includes built-in validation:

-   **Multiple providers without policy**: Throws `InvalidOperationException`
-   **Mixed default and named providers without policy**: Throws `InvalidOperationException`

### Service Registration

The package automatically registers these services:

-   `Api.Instance` as singleton
-   `IFeatureLifecycleManager` as singleton
-   `EvaluationContext` as transient (when configured)
-   Feature providers with appropriate lifetimes
-   Hooks as keyed singletons

## 📋 Examples

### ASP.NET Core Web API

```csharp
using OpenFeature;
using OpenFeature.DependencyInjection.Providers.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenFeature(featureBuilder =>
{
    featureBuilder
        .AddHostedFeatureLifecycle()
        .AddContext((contextBuilder, serviceProvider) =>
        {
            var httpContext = serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext;
            contextBuilder.Set("requestId", httpContext?.TraceIdentifier ?? "unknown");
        })
        .AddHook<RequestLoggingHook>()
        .AddInMemoryProvider("memory", provider => new Dictionary<string, Flag>
        {
            { "new-ui", new Flag<bool>(true) },
            { "beta-features", new Flag<bool>(false) }
        });
});

var app = builder.Build();

app.MapGet("/api/user-interface", async ([FromServices] IFeatureClient client) =>
{
    var useNewUI = await client.GetBooleanValueAsync("new-ui", false);
    return new { UseNewUI = useNewUI };
});

app.Run();
```

### Background Service

```csharp
public class FeatureFlagBackgroundService : BackgroundService
{
    private readonly IFeatureClient _featureClient;
    private readonly ILogger<FeatureFlagBackgroundService> _logger;

    public FeatureFlagBackgroundService(
        IFeatureClient featureClient,
        ILogger<FeatureFlagBackgroundService> logger)
    {
        _featureClient = featureClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var enableBatchProcessing = await _featureClient.GetBooleanValueAsync(
                "batch-processing", false);

            if (enableBatchProcessing)
            {
                _logger.LogInformation("Batch processing is enabled");
                // Perform batch processing
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}

// Registration
builder.Services.AddOpenFeature(featureBuilder =>
{
    featureBuilder
        .AddHostedFeatureLifecycle()
        .AddInMemoryProvider();
});

builder.Services.AddHostedService<FeatureFlagBackgroundService>();
```

## 🛠️ Framework Support

-   **Target Frameworks**: `.NET Standard 2.0`, `.NET 8.0`, `.NET 9.0`, `.NET Framework 4.6.2`
-   **Dependencies**:
    -   `Microsoft.Extensions.DependencyInjection.Abstractions`
    -   `Microsoft.Extensions.Options`
    -   `OpenFeature` (main SDK)

## 📚 Related Packages

-   **[OpenFeature](https://www.nuget.org/packages/OpenFeature)**: Core OpenFeature SDK
-   **[OpenFeature.Hosting](https://www.nuget.org/packages/OpenFeature.Hosting)**: Hosting lifecycle management
-   **[Provider packages](https://github.com/open-feature/dotnet-sdk-contrib)**: Various feature flag provider implementations

## 🤝 Contributing

See the [main repository CONTRIBUTING guide](../../CONTRIBUTING.md) for details on how to contribute to this project.

## 📄 License

This project is licensed under the same terms as the main OpenFeature .NET SDK. See the [LICENSE](../../LICENSE) file for details.
