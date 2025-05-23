<!-- markdownlint-disable MD033 MD039 -->
<!-- x-hide-in-docs-start -->
<!-- NuGet doesn't support most HTML tags. Disabling dark mode support until https://github.com/NuGet/NuGetGallery/issues/8644 is resolved. -->

![OpenFeature Dark Logo](https://raw.githubusercontent.com/open-feature/community/0e23508c163a6a1ac8c0ced3e4bd78faafe627c7/assets/logo/horizontal/black/openfeature-horizontal-black.svg)

## .NET SDK

<!-- x-hide-in-docs-end -->

[![Specification](https://img.shields.io/static/v1?label=specification&message=v0.8.0&color=yellow&style=for-the-badge)](https://github.com/open-feature/spec/releases/tag/v0.8.0)
[
![Release](https://img.shields.io/static/v1?label=release&message=v2.6.0&color=blue&style=for-the-badge) <!-- x-release-please-version -->
](https://github.com/open-feature/dotnet-sdk/releases/tag/v2.6.0) <!-- x-release-please-version -->

[![Slack](https://img.shields.io/badge/slack-%40cncf%2Fopenfeature-brightgreen?style=flat&logo=slack)](https://cloud-native.slack.com/archives/C0344AANLA1)
[![Codecov](https://codecov.io/gh/open-feature/dotnet-sdk/branch/main/graph/badge.svg?token=MONAVJBXUJ)](https://codecov.io/gh/open-feature/dotnet-sdk)
[![NuGet](https://img.shields.io/nuget/vpre/OpenFeature)](https://www.nuget.org/packages/OpenFeature)
[![CII Best Practices](https://bestpractices.coreinfrastructure.org/projects/6250/badge)](https://www.bestpractices.dev/en/projects/6250)

<!-- x-hide-in-docs-start -->

[OpenFeature](https://openfeature.dev) is an open specification that provides a vendor-agnostic, community-driven API for feature flagging that works with your favorite feature flag management tool or in-house solution.

<!-- x-hide-in-docs-end -->

## 🚀 Quick start

### Requirements

-   .NET 8+
-   .NET Framework 4.6.2+

Note that the packages will aim to support all current .NET versions. Refer to the currently supported versions [.NET](https://dotnet.microsoft.com/download/dotnet) and [.NET Framework](https://dotnet.microsoft.com/download/dotnet-framework) excluding .NET Framework 3.5

### Install

Use the following to initialize your project:

```sh
dotnet new console
```

and install OpenFeature:

```sh
dotnet add package OpenFeature
```

### Usage

```csharp
public async Task Example()
{
    // Register your feature flag provider
    try
    {
        await Api.Instance.SetProviderAsync(new InMemoryProvider());
    }
    catch (Exception ex)
    {
        // Log error
    }

    // Create a new client
    FeatureClient client = Api.Instance.GetClient();

    // Evaluate your feature flag
    bool v2Enabled = await client.GetBooleanValueAsync("v2_enabled", false);

    if ( v2Enabled )
    {
        // Do some work
    }
}
```

### Samples

The [`samples/`](./samples) folder contains example applications demonstrating how to use OpenFeature in different .NET scenarios.

| Sample Name                                       | Description                                                    |
|---------------------------------------------------|----------------------------------------------------------------|
| [AspNetCore](/samples/AspNetCore/README.md)       | Feature flags in an ASP.NET Core Web API.                      |

**Getting Started with a Sample:**

1. Navigate to the sample directory

   ```shell
   cd samples/AspNetCore
   ```

2. Restore dependencies and run

   ```shell
   dotnet run
   ```

Want to contribute a new sample? See our [CONTRIBUTING](#-contributing) guide!

## 🌟 Features

| Status | Features                                                            | Description                                                                                                                                                   |
| ------ | ------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| ✅     | [Providers](#providers)                                             | Integrate with a commercial, open source, or in-house feature management tool.                                                                                |
| ✅     | [Targeting](#targeting)                                             | Contextually-aware flag evaluation using [evaluation context](https://openfeature.dev/docs/reference/concepts/evaluation-context).                            |
| ✅     | [Hooks](#hooks)                                                     | Add functionality to various stages of the flag evaluation life-cycle.                                                                                        |
| ✅     | [Tracking](#tracking)                                               | Associate user actions with feature flag evaluations.                                                                                                         |
| ✅     | [Logging](#logging)                                                 | Integrate with popular logging packages.                                                                                                                      |
| ✅     | [Domains](#domains)                                                 | Logically bind clients with providers.                                                                                                                        |
| ✅     | [Eventing](#eventing)                                               | React to state changes in the provider or flag management system.                                                                                             |
| ✅     | [Shutdown](#shutdown)                                               | Gracefully clean up a provider during application shutdown.                                                                                                   |
| ✅     | [Transaction Context Propagation](#transaction-context-propagation) | Set a specific [evaluation context](https://openfeature.dev/docs/reference/concepts/evaluation-context) for a transaction (e.g. an HTTP request or a thread). |
| ✅     | [Extending](#extending)                                             | Extend OpenFeature with custom providers and hooks.                                                                                                           |
| 🔬     | [DependencyInjection](#DependencyInjection)                         | Integrate OpenFeature with .NET's dependency injection for streamlined provider setup.                                                                        |

> Implemented: ✅ | In-progress: ⚠️ | Not implemented yet: ❌ | Experimental: 🔬

### Providers

[Providers](https://openfeature.dev/docs/reference/concepts/provider) are an abstraction between a flag management system and the OpenFeature SDK.
Here is [a complete list of available providers](https://openfeature.dev/ecosystem?instant_search%5BrefinementList%5D%5Btype%5D%5B0%5D=Provider&instant_search%5BrefinementList%5D%5Btechnology%5D%5B0%5D=.NET).

If the provider you're looking for hasn't been created yet, see the [develop a provider](#develop-a-provider) section to learn how to build it yourself.

Once you've added a provider as a dependency, it can be registered with OpenFeature like this:

```csharp
try
{
    await Api.Instance.SetProviderAsync(new MyProvider());
}
catch (Exception ex)
{
    // Log error
}
```

When calling `SetProviderAsync` an exception may be thrown if the provider cannot be initialized. This may occur if the provider has not been configured correctly. See the documentation for the provider you are using for more information on how to configure the provider correctly.

In some situations, it may be beneficial to register multiple providers in the same application.
This is possible using [domains](#domains), which is covered in more detail below.

### Targeting

Sometimes, the value of a flag must consider some dynamic criteria about the application or user such as the user's location, IP, email address, or the server's location.
In OpenFeature, we refer to this as [targeting](https://openfeature.dev/specification/glossary#targeting).
If the flag management system you're using supports targeting, you can provide the input data using the [evaluation context](https://openfeature.dev/docs/reference/concepts/evaluation-context).

```csharp
// set a value to the global context
EvaluationContextBuilder builder = EvaluationContext.Builder();
builder.Set("region", "us-east-1");
EvaluationContext apiCtx = builder.Build();
Api.Instance.SetContext(apiCtx);

// set a value to the client context
builder = EvaluationContext.Builder();
builder.Set("region", "us-east-1");
EvaluationContext clientCtx = builder.Build();
var client = Api.Instance.GetClient();
client.SetContext(clientCtx);

// set a value to the invocation context
builder = EvaluationContext.Builder();
builder.Set("region", "us-east-1");
EvaluationContext reqCtx = builder.Build();

bool flagValue = await client.GetBooleanValuAsync("some-flag", false, reqCtx);

```

### Hooks

[Hooks](https://openfeature.dev/docs/reference/concepts/hooks) allow for custom logic to be added at well-defined points of the flag evaluation life-cycle.
Look [here](https://openfeature.dev/ecosystem/?instant_search%5BrefinementList%5D%5Btype%5D%5B0%5D=Hook&instant_search%5BrefinementList%5D%5Bcategory%5D%5B0%5D=Server-side&instant_search%5BrefinementList%5D%5Btechnology%5D%5B0%5D=.NET) for a complete list of available hooks.
If the hook you're looking for hasn't been created yet, see the [develop a hook](#develop-a-hook) section to learn how to build it yourself.

Once you've added a hook as a dependency, it can be registered at the global, client, or flag invocation level.

```csharp
// add a hook globally, to run on all evaluations
Api.Instance.AddHooks(new ExampleGlobalHook());

// add a hook on this client, to run on all evaluations made by this client
var client = Api.Instance.GetClient();
client.AddHooks(new ExampleClientHook());

// add a hook for this evaluation only
var value = await client.GetBooleanValueAsync("boolFlag", false, context, new FlagEvaluationOptions(new ExampleInvocationHook()));
```

### Logging

The .NET SDK uses Microsoft.Extensions.Logging. See the [manual](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line) for complete documentation.
Note that in accordance with the OpenFeature specification, the SDK doesn't generally log messages during flag evaluation. If you need further troubleshooting, please look into the `Logging Hook` section.

#### Logging Hook

The .NET SDK includes a LoggingHook, which logs detailed information at key points during flag evaluation, using Microsoft.Extensions.Logging structured logging API. This hook can be particularly helpful for troubleshooting and debugging; simply attach it at the global, client or invocation level and ensure your log level is set to "debug".

```csharp
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger("Program");

var client = Api.Instance.GetClient();
client.AddHooks(new LoggingHook(logger));
```

See [hooks](#hooks) for more information on configuring hooks.

### Domains

Clients can be assigned to a domain.
A domain is a logical identifier which can be used to associate clients with a particular provider.
If a domain has no associated provider, the default provider is used.

```csharp
try
{
    // registering the default provider
    await Api.Instance.SetProviderAsync(new LocalProvider());

    // registering a provider to a domain
    await Api.Instance.SetProviderAsync("clientForCache", new CachedProvider());
}
catch (Exception ex)
{
    // Log error
}

// a client backed by default provider
FeatureClient clientDefault = Api.Instance.GetClient();

// a client backed by CachedProvider
FeatureClient scopedClient = Api.Instance.GetClient("clientForCache");
```

Domains can be defined on a provider during registration.
For more details, please refer to the [providers](#providers) section.

### Eventing

Events allow you to react to state changes in the provider or underlying flag management system, such as flag definition changes,
provider readiness, or error conditions.
Initialization events (`PROVIDER_READY` on success, `PROVIDER_ERROR` on failure) are dispatched for every provider.
Some providers support additional events, such as `PROVIDER_CONFIGURATION_CHANGED`.

Please refer to the documentation of the provider you're using to see what events are supported.

Example usage of an Event handler:

```csharp
public static void EventHandler(ProviderEventPayload eventDetails)
{
    Console.WriteLine(eventDetails.Type);
}
```

```csharp
EventHandlerDelegate callback = EventHandler;
// add an implementation of the EventHandlerDelegate for the PROVIDER_READY event
Api.Instance.AddHandler(ProviderEventTypes.ProviderReady, callback);
```

It is also possible to register an event handler for a specific client, as in the following example:

```csharp
EventHandlerDelegate callback = EventHandler;

var myClient = Api.Instance.GetClient("my-client");

try
{
    var provider = new ExampleProvider();
    await Api.Instance.SetProviderAsync(myClient.GetMetadata().Name, provider);
}
catch (Exception ex)
{
    // Log error
}

myClient.AddHandler(ProviderEventTypes.ProviderReady, callback);
```

### Tracking

The [tracking API](https://openfeature.dev/specification/sections/tracking) allows you to use OpenFeature abstractions and objects to associate user actions with feature flag evaluations.
This is essential for robust experimentation powered by feature flags.
For example, a flag enhancing the appearance of a UI component might drive user engagement to a new feature; to test this hypothesis, telemetry collected by a hook(#hooks) or provider(#providers) can be associated with telemetry reported in the client's `track` function.

```csharp
var client = Api.Instance.GetClient();
client.Track("visited-promo-page", trackingEventDetails: new TrackingEventDetailsBuilder().SetValue(99.77).Set("currency", "USD").Build());
```

Note that some providers may not support tracking; check the documentation for your provider for more information.

### Shutdown

The OpenFeature API provides a close function to perform a cleanup of all registered providers. This should only be called when your application is in the process of shutting down.

```csharp
// Shut down all providers
await Api.Instance.ShutdownAsync();
```

### Transaction Context Propagation

Transaction context is a container for transaction-specific evaluation context (e.g. user id, user agent, IP).
Transaction context can be set where specific data is available (e.g. an auth service or request handler) and by using the transaction context propagator it will automatically be applied to all flag evaluations within a transaction (e.g. a request or thread).
By default, the `NoOpTransactionContextPropagator` is used, which doesn't store anything.
To register a [AsyncLocal](https://learn.microsoft.com/en-us/dotnet/api/system.threading.asynclocal-1) context propagator, you can use the `SetTransactionContextPropagator` method as shown below.

```csharp
// registering the AsyncLocalTransactionContextPropagator
Api.Instance.SetTransactionContextPropagator(new AsyncLocalTransactionContextPropagator());
```

Once you've registered a transaction context propagator, you can propagate the data into request-scoped transaction context.

```csharp
// adding userId to transaction context
EvaluationContext transactionContext = EvaluationContext.Builder()
    .Set("userId", userId)
    .Build();
Api.Instance.SetTransactionContext(transactionContext);
```

Additionally, you can develop a custom transaction context propagator by implementing the `TransactionContextPropagator` interface and registering it as shown above.

## Extending

### Develop a provider

To develop a provider, you need to create a new project and include the OpenFeature SDK as a dependency.
This can be a new repository or included in [the existing contrib repository](https://github.com/open-feature/dotnet-sdk-contrib) available under the OpenFeature organization.
You’ll then need to write the provider by implementing the `FeatureProvider` interface exported by the OpenFeature SDK.

```csharp
public class MyProvider : FeatureProvider
{
    public override Metadata GetMetadata()
    {
        return new Metadata("My Provider");
    }

    public override Task<ResolutionDetails<bool>> ResolveBooleanValueAsync(string flagKey, bool defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
    {
        // resolve a boolean flag value
    }

    public override Task<ResolutionDetails<string>> ResolveStringValueAsync(string flagKey, string defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
    {
        // resolve a string flag value
    }

    public override Task<ResolutionDetails<int>> ResolveIntegerValueAsync(string flagKey, int defaultValue, EvaluationContext context = null)
    {
        // resolve an int flag value
    }

    public override Task<ResolutionDetails<double>> ResolveDoubleValueAsync(string flagKey, double defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
    {
        // resolve a double flag value
    }

    public override Task<ResolutionDetails<Value>> ResolveStructureValueAsync(string flagKey, Value defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
    {
        // resolve an object flag value
    }
}
```

### Develop a hook

To develop a hook, you need to create a new project and include the OpenFeature SDK as a dependency.
This can be a new repository or included in [the existing contrib repository](https://github.com/open-feature/dotnet-sdk-contrib) available under the OpenFeature organization.
Implement your own hook by conforming to the `Hook interface`.
To satisfy the interface, all methods (`Before`/`After`/`Finally`/`Error`) need to be defined.

```csharp
public class MyHook : Hook
{
  public ValueTask<EvaluationContext> BeforeAsync<T>(HookContext<T> context,
      IReadOnlyDictionary<string, object> hints = null)
  {
    // code to run before flag evaluation
  }

  public ValueTask AfterAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> details,
      IReadOnlyDictionary<string, object>? hints = null)
  {
    // code to run after successful flag evaluation
  }

  public ValueTask ErrorAsync<T>(HookContext<T> context, Exception error,
      IReadOnlyDictionary<string, object>? hints = null)
  {
    // code to run if there's an error during before hooks or during flag evaluation
  }

  public ValueTask FinallyAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> evaluationDetails, IReadOnlyDictionary<string, object> hints = null)
  {
    // code to run after all other stages, regardless of success/failure
  }
}
```

Hooks support passing per-evaluation data between that stages using `hook data`. The below example hook uses `hook data` to measure the duration between the execution of the `before` and `after` stage.

```csharp
    class TimingHook : Hook
    {
        public ValueTask<EvaluationContext> BeforeAsync<T>(HookContext<T> context,
            IReadOnlyDictionary<string, object>? hints = null)
        {
            context.Data.Set("beforeTime", DateTime.Now);
            return ValueTask.FromResult(context.EvaluationContext);
        }

        public ValueTask AfterAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> details,
            IReadOnlyDictionary<string, object>? hints = null)
        {
            var beforeTime = context.Data.Get("beforeTime") as DateTime?;
            var duration = DateTime.Now - beforeTime;
            Console.WriteLine($"Duration: {duration}");
            return ValueTask.CompletedTask;
        }
    }
```

Built a new hook? [Let us know](https://github.com/open-feature/openfeature.dev/issues/new?assignees=&labels=hook&projects=&template=document-hook.yaml&title=%5BHook%5D%3A+) so we can add it to the docs!

### DependencyInjection

> [!NOTE]
> The OpenFeature.DependencyInjection and OpenFeature.Hosting packages are currently experimental. They streamline the integration of OpenFeature within .NET applications, allowing for seamless configuration and lifecycle management of feature flag providers using dependency injection and hosting services.

#### Installation

To set up dependency injection and hosting capabilities for OpenFeature, install the following packages:

```sh
dotnet add package OpenFeature.DependencyInjection
dotnet add package OpenFeature.Hosting
```

#### Usage Examples

For a basic configuration, you can use the InMemoryProvider. This provider is simple and well-suited for development and testing purposes.

**Basic Configuration:**

```csharp
builder.Services.AddOpenFeature(featureBuilder => {
    featureBuilder
        .AddHostedFeatureLifecycle() // From Hosting package
        .AddInMemoryProvider();
});
```

You can add EvaluationContext, hooks, and handlers at a global/API level as needed.

```csharp
builder.Services.AddOpenFeature(featureBuilder => {
    featureBuilder
        .AddContext((contextBuilder, serviceProvider) => { /* Custom context configuration */ })
        .AddHook<LoggingHook>()
        .AddHandler(ProviderEventTypes.ProviderReady, (eventDetails) => { /* Handle event */ });
});
```

**Domain-Scoped Provider Configuration:**
<br />To set up multiple providers with a selection policy, define logic for choosing the default provider. This example designates `name1` as the default provider:

```csharp
builder.Services.AddOpenFeature(featureBuilder => {
    featureBuilder
        .AddHostedFeatureLifecycle()
        .AddContext((contextBuilder, serviceProvider) => { /* Custom context configuration */ })
        .AddHook((serviceProvider) => new LoggingHook( /* Custom configuration */ ))
        .AddInMemoryProvider("name1")
        .AddInMemoryProvider("name2")
        .AddPolicyName(options => {
            // Custom logic to select a default provider
            options.DefaultNameSelector = serviceProvider => "name1";
        });
});
```

### Registering a Custom Provider

You can register a custom provider, such as `InMemoryProvider`, with OpenFeature using the `AddProvider` method. This approach allows you to dynamically resolve services or configurations during registration.

```csharp
services.AddOpenFeature(builder =>
{
    builder.AddProvider(provider =>
    {
        // Resolve services or configurations as needed
        var variants = new Dictionary<string, bool> { { "on", true } };
        var flags = new Dictionary<string, Flag>
        {
            { "feature-key", new Flag<bool>(variants, "on") }
        };

        // Register a custom provider, such as InMemoryProvider
        return new InMemoryProvider(flags);
    });
});
```

#### Adding a Domain-Scoped Provider

You can also register a domain-scoped custom provider, enabling configurations specific to each domain:

```csharp
services.AddOpenFeature(builder =>
{
    builder.AddProvider("my-domain", (provider, domain) =>
    {
        // Resolve services or configurations as needed for the domain
        var variants = new Dictionary<string, bool> { { "on", true } };
        var flags = new Dictionary<string, Flag>
        {
            { $"{domain}-feature-key", new Flag<bool>(variants, "on") }
        };

        // Register a domain-scoped custom provider such as InMemoryProvider
        return new InMemoryProvider(flags);
    });
});
```

<!-- x-hide-in-docs-start -->

## ⭐️ Support the project

-   Give this repo a ⭐️!
-   Follow us on social media:
    -   Twitter: [@openfeature](https://twitter.com/openfeature)
    -   LinkedIn: [OpenFeature](https://www.linkedin.com/company/openfeature/)
-   Join us on [Slack](https://cloud-native.slack.com/archives/C0344AANLA1)
-   For more information, check out our [community page](https://openfeature.dev/community/)

## 🤝 Contributing

Interested in contributing? Great, we'd love your help! To get started, take a look at the [CONTRIBUTING](CONTRIBUTING.md) guide.

### Thanks to everyone who has already contributed

[![Contrib Rocks](https://contrib.rocks/image?repo=open-feature/dotnet-sdk)](https://github.com/open-feature/dotnet-sdk/graphs/contributors)

Made with [contrib.rocks](https://contrib.rocks).

<!-- x-hide-in-docs-end -->
