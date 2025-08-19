# OpenFeature.DependencyInjection

> **⚠️ DEPRECATED**: This library is now deprecated. The OpenTelemetry Dependency Injection library has been moved to the OpenFeature Hosting integration in version 2.9.0.

OpenFeature is an open standard for feature flag management, created to support a robust feature flag ecosystem using cloud native technologies. OpenFeature will provide a unified API and SDK, and a developer-first, cloud-native implementation, with extensibility for open source and commercial offerings.

## Migration Guide

If you are using `OpenFeature.DependencyInjection`, you should migrate to the `OpenFeature.Hosting` package. The hosting package provides the same functionality but in one package.

### 1. Update dependencies

Remove this package:

```xml
<PackageReference Include="OpenFeature.DependencyInjection" Version="..." />
```

Update or install the latest `OpenFeature.Hosting` package:

```xml
<PackageReference Include="OpenFeature.Hosting" Version="2.9.0" />
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
