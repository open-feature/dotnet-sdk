<!-- markdownlint-disable MD033 -->
<!-- x-hide-in-docs-start -->
<!-- NuGet doesn't support most HTML tags. Disabling dark mode support until https://github.com/NuGet/NuGetGallery/issues/8644 is resolved. -->
![OpenFeature Dark Logo](https://raw.githubusercontent.com/open-feature/community/0e23508c163a6a1ac8c0ced3e4bd78faafe627c7/assets/logo/horizontal/black/openfeature-horizontal-black.svg)

## .NET SDK

<!-- x-hide-in-docs-end -->

[![Specification](https://img.shields.io/static/v1?label=specification&message=v0.7.0&color=yellow&style=for-the-badge)](https://github.com/open-feature/spec/releases/tag/v0.7.0)
[
  ![Release](https://img.shields.io/static/v1?label=release&message=v1.5.0&color=blue&style=for-the-badge) <!-- x-release-please-version -->
](https://github.com/open-feature/dotnet-sdk/releases/tag/v1.5.0) <!-- x-release-please-version -->

[![Slack](https://img.shields.io/badge/slack-%40cncf%2Fopenfeature-brightgreen?style=flat&logo=slack)](https://cloud-native.slack.com/archives/C0344AANLA1)
[![Codecov](https://codecov.io/gh/open-feature/dotnet-sdk/branch/main/graph/badge.svg?token=MONAVJBXUJ)](https://codecov.io/gh/open-feature/dotnet-sdk)
[![NuGet](https://img.shields.io/nuget/vpre/OpenFeature)](https://www.nuget.org/packages/OpenFeature)
[![CII Best Practices](https://bestpractices.coreinfrastructure.org/projects/6250/badge)](https://www.bestpractices.dev/en/projects/6250)
<!-- x-hide-in-docs-start -->

[OpenFeature](https://openfeature.dev) is an open specification that provides a vendor-agnostic, community-driven API for feature flagging that works with your favorite feature flag management tool or in-house solution.

<!-- x-hide-in-docs-end -->

## 🚀 Quick start

### Requirements

-   .NET 6+
-   .NET Core 6+
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
    await Api.Instance.SetProvider(new InMemoryProvider());

    // Create a new client
    FeatureClient client = Api.Instance.GetClient();

    // Evaluate your feature flag
    bool v2Enabled = await client.GetBooleanValue("v2_enabled", false);

    if ( v2Enabled )
    {
        //Do some work
    }
}
```

## 🌟 Features

| Status | Features                        | Description                                                                                                                        |
| ------ | ------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| ✅     | [Providers](#providers)         | Integrate with a commercial, open source, or in-house feature management tool.                                                     |
| ✅     | [Targeting](#targeting)         | Contextually-aware flag evaluation using [evaluation context](https://openfeature.dev/docs/reference/concepts/evaluation-context). |
| ✅     | [Hooks](#hooks)                 | Add functionality to various stages of the flag evaluation life-cycle.                                                             |
| ✅     | [Logging](#logging)             | Integrate with popular logging packages.                                                                                           |
| ✅     | [Named clients](#named-clients) | Utilize multiple providers in a single application.                                                                                |
| ✅     | [Eventing](#eventing)           | React to state changes in the provider or flag management system.                                                                  |
| ✅    | [Shutdown](#shutdown)           | Gracefully clean up a provider during application shutdown.                                                                        |
| ✅     | [Extending](#extending)         | Extend OpenFeature with custom providers and hooks.                                                                                |

> Implemented: ✅ | In-progress: ⚠️ | Not implemented yet: ❌

### Providers

[Providers](https://openfeature.dev/docs/reference/concepts/provider) are an abstraction between a flag management system and the OpenFeature SDK.
Here is [a complete list of available providers](https://openfeature.dev/ecosystem?instant_search%5BrefinementList%5D%5Btype%5D%5B0%5D=Provider&instant_search%5BrefinementList%5D%5Btechnology%5D%5B0%5D=.NET).

If the provider you're looking for hasn't been created yet, see the [develop a provider](#develop-a-provider) section to learn how to build it yourself.

Once you've added a provider as a dependency, it can be registered with OpenFeature like this:

```csharp
await Api.Instance.SetProvider(new MyProvider());
```

In some situations, it may be beneficial to register multiple providers in the same application.
This is possible using [named clients](#named-clients), which is covered in more detail below.

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

bool flagValue = await client.GetBooleanValue("some-flag", false, reqCtx);

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
var value = await client.GetBooleanValue("boolFlag", false, context, new FlagEvaluationOptions(new ExampleInvocationHook()));
```

### Logging

The .NET SDK uses Microsoft.Extensions.Logging. See the [manual](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line) for complete documentation.

### Named clients

Clients can be given a name.
A name is a logical identifier that can be used to associate clients with a particular provider.
If a name has no associated provider, the global provider is used.

```csharp
// registering the default provider
await Api.Instance.SetProvider(new LocalProvider());

// registering a named provider
await Api.Instance.SetProvider("clientForCache", new CachedProvider());

// a client backed by default provider
FeatureClient clientDefault = Api.Instance.GetClient();

// a client backed by CachedProvider
FeatureClient clientNamed = Api.Instance.GetClient("clientForCache");

```

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

var provider = new ExampleProvider();
await Api.Instance.SetProvider(myClient.GetMetadata().Name, provider);

myClient.AddHandler(ProviderEventTypes.ProviderReady, callback);
```

### Shutdown

The OpenFeature API provides a close function to perform a cleanup of all registered providers. This should only be called when your application is in the process of shutting down.

```csharp
// Shut down all providers
await Api.Instance.Shutdown();
```

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

    public override Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue, EvaluationContext context = null)
    {
        // resolve a boolean flag value
    }

    public override Task<ResolutionDetails<double>> ResolveDoubleValue(string flagKey, double defaultValue, EvaluationContext context = null)
    {
        // resolve a double flag value
    }

    public override Task<ResolutionDetails<int>> ResolveIntegerValue(string flagKey, int defaultValue, EvaluationContext context = null)
    {
        // resolve an int flag value
    }

    public override Task<ResolutionDetails<string>> ResolveStringValue(string flagKey, string defaultValue, EvaluationContext context = null)
    {
        // resolve a string flag value
    }

    public override Task<ResolutionDetails<Value>> ResolveStructureValue(string flagKey, Value defaultValue, EvaluationContext context = null)
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
  public Task<EvaluationContext> Before<T>(HookContext<T> context,
      IReadOnlyDictionary<string, object> hints = null)
  {
    // code to run before flag evaluation
  }

  public virtual Task After<T>(HookContext<T> context, FlagEvaluationDetails<T> details,
      IReadOnlyDictionary<string, object> hints = null)
  {
    // code to run after successful flag evaluation
  }

  public virtual Task Error<T>(HookContext<T> context, Exception error,
      IReadOnlyDictionary<string, object> hints = null)
  {
    // code to run if there's an error during before hooks or during flag evaluation
  }

  public virtual Task Finally<T>(HookContext<T> context, IReadOnlyDictionary<string, object> hints = null)
  {
    // code to run after all other stages, regardless of success/failure
  }
}
```

Built a new hook? [Let us know](https://github.com/open-feature/openfeature.dev/issues/new?assignees=&labels=hook&projects=&template=document-hook.yaml&title=%5BHook%5D%3A+) so we can add it to the docs!

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
