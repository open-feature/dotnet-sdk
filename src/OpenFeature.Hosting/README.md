# OpenFeature.Hosting

[![NuGet](https://img.shields.io/nuget/vpre/OpenFeature.Hosting?label=OpenFeature.Hosting&style=for-the-badge)](https://www.nuget.org/packages/OpenFeature.Hosting)
[![Specification](https://img.shields.io/static/v1?label=specification&message=v0.8.0&color=yellow&style=for-the-badge)](https://github.com/open-feature/spec/releases/tag/v0.8.0)

OpenFeature.Hosting is an extension for the [OpenFeature .NET SDK](https://github.com/open-feature/dotnet-sdk) that streamlines integration with .NET applications using dependency injection and hosting. It enables seamless configuration and lifecycle management of feature flag providers, hooks, and evaluation context using idiomatic .NET patterns.

**🧪 The OpenFeature.Hosting package is still considered experimental and may undergo significant changes. Feedback and contributions are welcome!**

## 🚀 Quick Start

### Requirements

- .NET 8+
- .NET Framework 4.6.2+

### Installation

Add the package to your project:

```sh
dotnet add package OpenFeature.Hosting
```

### Basic Usage

Register OpenFeature in your application's dependency injection container (e.g., in `Program.cs` for ASP.NET Core):

```csharp
builder.Services.AddOpenFeature(featureBuilder => {
    featureBuilder
        .AddInMemoryProvider();
});
```

You can add global evaluation context, hooks, and event handlers as needed:

```csharp
builder.Services.AddOpenFeature(featureBuilder => {
    featureBuilder
        .AddContext((contextBuilder, serviceProvider) => {
            // Custom context configuration
        })
        .AddHook<LoggingHook>()
        .AddHandler(ProviderEventTypes.ProviderReady, (eventDetails) => {
            // Handle provider ready event
        });
});
```

### Domain-Scoped Providers

To register multiple providers and select a default provider by domain:

```csharp
builder.Services.AddOpenFeature(featureBuilder => {
    featureBuilder
        .AddInMemoryProvider("default")
        .AddInMemoryProvider("beta")
        .AddPolicyName(options => {
            options.DefaultNameSelector = serviceProvider => "default";
        });
});
```

### Registering a Custom Provider

You can register a custom provider using a factory:

```csharp
builder.Services.AddOpenFeature(featureBuilder => {
    featureBuilder.AddProvider(provider => {
        // Resolve services or configuration as needed
        return new MyCustomProvider();
    });
});
```

## 🧩 Features

- **Dependency Injection**: Register providers, hooks, and context using the .NET DI container.
- **Domain Support**: Assign providers to logical domains for multi-tenancy or environment separation.
- **Event Handlers**: React to provider lifecycle events (e.g., readiness).
- **Extensibility**: Add custom hooks, context, and providers.

## 🛠️ Example: ASP.NET Core Integration

Below is a simple example of integrating OpenFeature with an ASP.NET Core application using an in-memory provider and a logging hook.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenFeature(featureBuilder => {
    featureBuilder
        .AddInMemoryProvider()
        .AddHook<LoggingHook>();
});

var app = builder.Build();

app.MapGet("/", async (IFeatureClient client) => {
    bool enabled = await client.GetBooleanValueAsync("my-flag", false);
    return enabled ? "Feature enabled!" : "Feature disabled.";
});

app.Run();
```

If you have multiple providers registered, you can specify which client and provider to resolve by using the `FromKeyedServices` attribute:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenFeature(featureBuilder => {
    featureBuilder
        .AddInMemoryProvider("default")
        .AddInMemoryProvider("beta")
        .AddPolicyName(options => {
            options.DefaultNameSelector = serviceProvider => "default";
        });
});

var app = builder.Build();

app.MapGet("/", async ([FromKeyedServices("beta")] IFeatureClient client) => {
    bool enabled = await client.GetBooleanValueAsync("my-flag", false);
    return enabled ? "Feature enabled!" : "Feature disabled.";
});

app.Run();
```

## 📚 Further Reading

- [OpenFeature .NET SDK Documentation](https://github.com/open-feature/dotnet-sdk)
- [OpenFeature Specification](https://openfeature.dev)
- [Samples](https://github.com/open-feature/dotnet-sdk/blob/main/samples/AspNetCore/README.md)

## 🤝 Contributing

Contributions are welcome! See the [CONTRIBUTING](https://github.com/open-feature/dotnet-sdk/blob/main/CONTRIBUTING.md) guide for details.
