# OpenFeature .NET MultiProvider

[![NuGet](https://img.shields.io/nuget/vpre/OpenFeature.Providers.MultiProvider)](https://www.nuget.org/packages/OpenFeature.Providers.MultiProvider)

The MultiProvider is a feature provider that enables the use of multiple underlying providers, allowing different providers to be used for different flag keys or based on specific routing logic. This enables scenarios where different feature flags may be served by different sources or providers within the same application.

## Overview

The MultiProvider acts as a composite provider that can delegate flag resolution to different underlying providers based on configuration or routing rules. It supports various evaluation strategies to determine how multiple providers should be evaluated and how their results should be combined.

For more information about the MultiProvider specification, see the [OpenFeature Multi Provider specification](https://openfeature.dev/specification/appendix-a/#multi-provider).

## Installation

```shell
dotnet add package OpenFeature.Providers.MultiProvider
```

## Usage

### Basic Setup

```csharp
using OpenFeature;
using OpenFeature.Providers.MultiProvider;

// Create your individual providers
var primaryProvider = new YourPrimaryProvider();
var fallbackProvider = new YourFallbackProvider();

// Create provider entries
var providerEntries = new[]
{
    new ProviderEntry(primaryProvider, "primary"),
    new ProviderEntry(fallbackProvider, "fallback")
};

// Create and set the MultiProvider
var multiProvider = new MultiProvider(providerEntries);
await Api.Instance.SetProviderAsync(multiProvider);

// Use the client as normal
var client = Api.Instance.GetClient();
var result = await client.GetBooleanValueAsync("my-flag", false);
```

### Evaluation Strategies

The MultiProvider supports several evaluation strategies to determine how providers are evaluated:

#### 1. FirstMatchStrategy (Default)

Returns the first result that does not indicate "flag not found". Providers are evaluated sequentially in the order they were configured.

```csharp
using OpenFeature.Providers.MultiProvider.Strategies;

var strategy = new FirstMatchStrategy();
var multiProvider = new MultiProvider(providerEntries, strategy);
```

#### 2. FirstSuccessfulStrategy

Returns the first result that does not result in an error. If any provider returns an error, it's ignored as long as there is a successful result.

```csharp
using OpenFeature.Providers.MultiProvider.Strategies;

var strategy = new FirstSuccessfulStrategy();
var multiProvider = new MultiProvider(providerEntries, strategy);
```

#### 3. ComparisonStrategy

Evaluates all providers and compares their results. Useful for testing or validation scenarios where you want to ensure providers return consistent values.

```csharp
using OpenFeature.Providers.MultiProvider.Strategies;

var strategy = new ComparisonStrategy();
var multiProvider = new MultiProvider(providerEntries, strategy);
```

### Advanced Configuration

#### Named Providers

You can assign names to providers for better identification and debugging:

```csharp
var providerEntries = new[]
{
    new ProviderEntry(new ProviderA(), "provider-a"),
    new ProviderEntry(new ProviderB(), "provider-b"),
    new ProviderEntry(new ProviderC(), "provider-c")
};
```

#### Custom Evaluation Context

The MultiProvider respects evaluation context and passes it to underlying providers:

```csharp
var context = EvaluationContext.Builder()
    .Set("userId", "user123")
    .Set("environment", "production")
    .Build();

var result = await client.GetBooleanValueAsync("feature-flag", false, context);
```

## Use Cases

### Primary/Fallback Configuration

Use multiple providers with fallback capabilities:

```csharp
var providerEntries = new[]
{
    new ProviderEntry(new RemoteProvider(), "remote"),
    new ProviderEntry(new LocalCacheProvider(), "cache"),
    new ProviderEntry(new StaticProvider(), "static")
};

var multiProvider = new MultiProvider(providerEntries, new FirstSuccessfulStrategy());
```

### A/B Testing Provider Comparison

Compare results from different providers for testing purposes:

```csharp
var providerEntries = new[]
{
    new ProviderEntry(new ProviderA(), "provider-a"),
    new ProviderEntry(new ProviderB(), "provider-b")
};

var multiProvider = new MultiProvider(providerEntries, new ComparisonStrategy());
```

### Migration Scenarios

Gradually migrate from one provider to another:

```csharp
var providerEntries = new[]
{
    new ProviderEntry(new NewProvider(), "new-provider"),
    new ProviderEntry(new LegacyProvider(), "legacy-provider")
};

var multiProvider = new MultiProvider(providerEntries, new FirstMatchStrategy());
```

## Error Handling

The MultiProvider handles errors from underlying providers according to the chosen evaluation strategy:

- **FirstMatchStrategy**: Throws errors immediately when encountered
- **FirstSuccessfulStrategy**: Ignores errors if there's a successful result, throws all errors if all providers fail
- **ComparisonStrategy**: Collects and reports all errors for analysis

## Thread Safety

The MultiProvider is thread-safe and can be used concurrently across multiple threads. It properly handles initialization and shutdown of underlying providers.

## Lifecycle Management

The MultiProvider manages the lifecycle of all registered providers:

```csharp
// Initialize all providers
await multiProvider.InitializeAsync(context);

// Shutdown all providers
await multiProvider.ShutdownAsync();

// Dispose (implements IAsyncDisposable)
await multiProvider.DisposeAsync();
```

## Events

The MultiProvider supports OpenFeature events and provides specification-compliant event handling. It follows the [OpenFeature Multi-Provider specification](https://openfeature.dev/specification/appendix-a#status-and-event-handling) for event handling behavior.

### Event Handling Example

```csharp
using OpenFeature;
using OpenFeature.Providers.MultiProvider;

// Create the MultiProvider with multiple providers
var providerEntries = new[]
{
    new ProviderEntry(new ProviderA(), "provider-a"),
    new ProviderEntry(new ProviderB(), "provider-b")
};
var multiProvider = new MultiProvider(providerEntries);

// Subscribe to MultiProvider events
Api.Instance.AddHandler(ProviderEventTypes.ProviderReady, (eventDetails) =>
{
    Console.WriteLine($"MultiProvider is ready: {eventDetails?.ProviderName}");
});

Api.Instance.AddHandler(ProviderEventTypes.ProviderStale, (eventDetails) =>
{
    Console.WriteLine($"MultiProvider became stale: {eventDetails?.Message}");
});

Api.Instance.AddHandler(ProviderEventTypes.ProviderConfigurationChanged, (eventDetails) =>
{
    Console.WriteLine($"Configuration changed - Flags: {string.Join(", ", eventDetails?.FlagsChanged ?? [])}");
});

Api.Instance.AddHandler(ProviderEventTypes.ProviderError, (eventDetails) =>
{
    Console.WriteLine($"MultiProvider error: {eventDetails?.Message}");
});

// Set the provider - this will initialize all underlying providers
// and emit PROVIDER_READY when all are successfully initialized
await Api.Instance.SetProviderAsync(multiProvider);

// Later, if an underlying provider becomes stale and changes MultiProvider status:
// Only then will a PROVIDER_STALE event be emitted from MultiProvider
```

### Event Lifecycle

1. **During Initialization**:

    - MultiProvider emits `PROVIDER_READY` when all underlying providers initialize successfully
    - MultiProvider emits `PROVIDER_ERROR` if any providers fail to initialize (causing aggregate status to become ERROR/FATAL)

2. **Runtime Status Changes**:

    - Status-changing events from underlying providers are captured internally
    - MultiProvider only emits events when its aggregate status changes due to these internal events
    - Example: If MultiProvider is READY and one provider becomes STALE, MultiProvider emits `PROVIDER_STALE`

3. **Configuration Changes**:
    - `PROVIDER_CONFIGURATION_CHANGED` events from underlying providers are always re-emitted

## Requirements

- .NET 8+
- .NET Framework 4.6.2+
- .NET Standard 2.0+

## Contributing

See the [OpenFeature .NET SDK contributing guide](../../CONTRIBUTING.md) for details on how to contribute to this project.
