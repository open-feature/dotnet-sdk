# OpenFeature.Hosting

> **⚠️ DEPRECATED**: This library is now deprecated. The OpenTelemetry Hosting integration has been moved to the OpenFeature DependencyInjection library version 2.8.0.

OpenFeature is an open standard for feature flag management, created to support a robust feature flag ecosystem using cloud native technologies. OpenFeature will provide a unified API and SDK, and a developer-first, cloud-native implementation, with extensibility for open source and commercial offerings.

## Migration Guide

If you are using `OpenFeature.Hosting`, you should migrate to the `OpenFeature.DependencyInjection` package. The dependency injection package provides the same functionality but in one package.

### 1. Update dependencies

Remove this package:

```xml
<PackageReference Include="OpenFeature.Hosting" Version="..." />
```

Update or install the latest `OpenFeature.DependencyInjection` package:

```xml
<PackageReference Include="OpenFeature.DependencyInjection" Version="2.8.0" />
```

### 2. Update your `Program.cs`

Remove the `AddHostedFeatureLifecycle` method call.

#### Before

```csharp
builder.Services.AddOpenFeature(featureBuilder =>
{
    featureBuilder
        .AddHostedFeatureLifecycle();

    // Omit for code brevity
});
```

#### After

```csharp
builder.Services.AddOpenFeature(featureBuilder =>
{
    // Omit for code brevity
});
```
