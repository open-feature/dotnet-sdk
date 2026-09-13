# OpenFeature.Providers.DependencyInjection

[![NuGet](https://img.shields.io/nuget/vpre/OpenFeature.Providers.DependencyInjection?label=OpenFeature.Providers.DependencyInjection&style=for-the-badge)](https://www.nuget.org/packages/OpenFeature.Providers.DependencyInjection)
[![Specification](https://img.shields.io/static/v1?label=specification&message=v0.8.0&color=yellow&style=for-the-badge)](https://github.com/open-feature/spec/releases/tag/v0.8.0)

OpenFeature.Providers.DependencyInjection is a thin, hosting-free package for the [OpenFeature .NET SDK](https://github.com/open-feature/dotnet-sdk). It exposes the `OpenFeatureBuilder` and the builder extension methods needed to register providers, hooks, evaluation context, and clients with the .NET dependency injection container.

It is intended for **provider and community library authors** who need a small, stable integration surface without taking a dependency on `Microsoft.Extensions.Hosting`. **Application developers** should reference `OpenFeature.Hosting` instead, which builds on this package and adds the hosted lifecycle (`AddOpenFeature`, provider initialization and shutdown).

**🧪 This package is still considered experimental and may undergo significant changes. Feedback and contributions are welcome!**

## 🤔 Which package do I use?

| You are… | Use |
| --- | --- |
| An application developer wiring up OpenFeature | `OpenFeature.Hosting` (call `AddOpenFeature(...)`) |
| A provider / community library author | `OpenFeature.Providers.DependencyInjection` (extend `OpenFeatureBuilder`) |

## 🚀 Quick Start

### Requirements

- .NET 8+
- .NET Framework 4.6.2+

### Installation

```sh
dotnet add package OpenFeature.Providers.DependencyInjection
```

## 🧩 Authoring a provider integration

Provider libraries integrate by adding extension methods on `OpenFeatureBuilder`. Reference only this package — no hosting dependency is required, and no central registration is needed:

```csharp
using Microsoft.Extensions.DependencyInjection;
using OpenFeature.Hosting;

namespace OpenFeature.Contrib.Providers.MyProvider;

public static class MyProviderBuilderExtensions
{
    public static OpenFeatureBuilder AddMyProvider(
        this OpenFeatureBuilder builder,
        Action<MyProviderOptions>? configureOptions = null)
        => builder.AddProvider<MyProviderOptions>(
            provider => new MyProvider(/* resolve services / options */),
            configureOptions);
}
```

Consumers then compose it through the standard OpenFeature setup:

```csharp
builder.Services.AddOpenFeature(featureBuilder =>
{
    featureBuilder.AddMyProvider(options => { /* ... */ });
});
```

The same pattern applies to hooks (`AddHook`), event handlers (`AddHandler`), and evaluation context (`AddContext`).
