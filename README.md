<!-- markdownlint-disable MD033 -->
<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/open-feature/community/0e23508c163a6a1ac8c0ced3e4bd78faafe627c7/assets/logo/horizontal/white/openfeature-horizontal-white.svg">
    <source media="(prefers-color-scheme: light)" srcset="https://raw.githubusercontent.com/open-feature/community/0e23508c163a6a1ac8c0ced3e4bd78faafe627c7/assets/logo/horizontal/black/openfeature-horizontal-black.svg">
    <img align="center" alt="OpenFeature Logo">
  </picture>
</p>

<h2 align="center">OpenFeature .NET SDK</h2>

[![a](https://img.shields.io/badge/slack-%40cncf%2Fopenfeature-brightgreen?style=flat&logo=slack)](https://cloud-native.slack.com/archives/C0344AANLA1)
[![spec version badge](https://img.shields.io/badge/Specification-v0.5.2-yellow)](https://github.com/open-feature/spec/tree/v0.5.2?rgh-link-date=2023-01-20T21%3A37%3A52Z)
[![codecov](https://codecov.io/gh/open-feature/dotnet-sdk/branch/main/graph/badge.svg?token=MONAVJBXUJ)](https://codecov.io/gh/open-feature/dotnet-sdk)
[![nuget](https://img.shields.io/nuget/vpre/OpenFeature)](https://www.nuget.org/packages/OpenFeature)
[![CII Best Practices](https://bestpractices.coreinfrastructure.org/projects/6250/badge)](https://bestpractices.coreinfrastructure.org/projects/6250)

## 👋 Hey there! Thanks for checking out the OpenFeature .NET SDK

### What is OpenFeature?

[OpenFeature][openfeature-website] is an open specification that provides a vendor-agnostic, community-driven API for feature flagging that works with your favorite feature flag management tool.

### Why standardize feature flags?

Standardizing feature flags unifies tools and vendors behind a common interface which avoids vendor lock-in at the code level. Additionally, it offers a framework for building extensions and integrations and allows providers to focus on their unique value proposition.

## 🔍 Requirements:

- .NET 6+
- .NET Core 6+
- .NET Framework 4.6.2+

Note that the packages will aim to support all current .NET versions. Refer to the currently supported versions [.NET](https://dotnet.microsoft.com/download/dotnet) and [.NET Framework](https://dotnet.microsoft.com/download/dotnet-framework) excluding .NET Framework 3.5


## 📦 Installation:

Use the following to initialize your project:

```sh
dotnet new console
```

and install OpenFeature:

```sh
dotnet add package OpenFeature
```

## 🌟 Features:

- support for various backend [providers](https://openfeature.dev/docs/reference/concepts/provider)
- easy integration and extension via [hooks](https://openfeature.dev/docs/reference/concepts/hooks)
- bool, string, numeric and object flag types
- [context-aware](https://openfeature.dev/docs/reference/concepts/evaluation-context) evaluation

## 🚀 Usage:

### Basics:

```csharp
using OpenFeature.Model;

// Sets the provider used by the client
// If no provider is set, then a default NoOpProvider will be used.
//OpenFeature.Api.Instance.SetProvider(new MyProvider());

// Gets a instance of the feature flag client
var client = OpenFeature.Api.Instance.GetClient();
// Evaluation the `my-feature` feature flag
var isEnabled = await client.GetBooleanValue("my-feature", false);
```

For complete documentation, visit: https://openfeature.dev/docs/category/concepts

### Context-aware evaluation:

Sometimes the value of a flag must take into account some dynamic criteria about the application or user, such as the user location, IP, email address, or the location of the server.
In OpenFeature, we refer to this as [`targeting`](https://openfeature.dev/specification/glossary#targeting).
If the flag system you're using supports targeting, you can provide the input data using the `EvaluationContext`.

```csharp
using OpenFeature.Model;

var client = OpenFeature.Api.Instance.GetClient();

// Evaluating with a context.
var evaluationContext = EvaluationContext.Builder()
    .Set("my-key", "my-value")
    .Build();

// Evaluation the `my-conditional` feature flag
var isEnabled = await client.GetBooleanValue("my-conditional", false, evaluationContext);
```

### Providers:

To develop a provider, you need to create a new project and include the OpenFeature SDK as a dependency. This can be a new repository or included in [the existing contrib repository](https://github.com/open-feature/dotnet-sdk-contrib) available under the OpenFeature organization. Finally, you’ll then need to write the provider itself. This can be accomplished by implementing the `FeatureProvider` interface exported by the OpenFeature SDK.

```csharp
using OpenFeature;
using OpenFeature.Model;

public class MyFeatureProvider : FeatureProvider
{
    public static string Name => "My Feature Provider";

    public Metadata GetMetadata()
    {
        return new Metadata(Name);
    }

    public Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue,
        EvaluationContext context = null)
    {
        // code to resolve boolean details
    }

    public Task<ResolutionDetails<string>> ResolveStringValue(string flagKey, string defaultValue,
        EvaluationContext context = null)
    {
        // code to resolve string details
    }

    public Task<ResolutionDetails<int>> ResolveIntegerValue(string flagKey, int defaultValue,
        EvaluationContext context = null)
    {
        // code to resolve integer details
    }

    public Task<ResolutionDetails<double>> ResolveDoubleValue(string flagKey, double defaultValue,
        EvaluationContext context = null)
    {
        // code to resolve integer details
    }

    public Task<ResolutionDetails<T>> ResolveStructureValue<T>(string flagKey, T defaultValue,
        EvaluationContext context = null)
    {
        // code to resolve object details
    }
}
```

See [here](https://openfeature.dev/docs/reference/technologies/server/dotnet) for a catalog of available providers.

### Hooks:

Hooks are a mechanism that allow for the addition of arbitrary behavior at well-defined points of the flag evaluation life-cycle. Use cases include validation of the resolved flag value, modifying or adding data to the evaluation context, logging, telemetry, and tracking.

```csharp
// add a hook globally, to run on all evaluations
Api.Instance.AddHooks(new ExampleGlobalHook());

// add a hook on this client, to run on all evaluations made by this client
var client = Api.Instance.GetClient();
client.AddHooks(new ExampleClientHook());

// add a hook for this evaluation only
var value = await client.GetBooleanValue("boolFlag", false, context, new FlagEvaluationOptions(new ExampleInvocationHook()));
```

Example of implementing a hook

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

See [here](https://openfeature.dev/docs/reference/technologies/server/dotnet) for a catalog of available hooks.

### Logging:

The .NET SDK uses Microsoft Extensions Logger. See the [manual](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line) for complete documentation.

## ⭐️ Support the project

- Give this repo a ⭐️!
- Follow us social media:
  - Twitter: [@openfeature](https://twitter.com/openfeature)
  - LinkedIn: [OpenFeature](https://www.linkedin.com/company/openfeature/)
- Join us on [Slack](https://cloud-native.slack.com/archives/C0344AANLA1)
- For more check out our [community page](https://openfeature.dev/community/)

## 🤝 Contributing

Interested in contributing? Great, we'd love your help! To get started, take a look at the [CONTRIBUTING](CONTRIBUTING.md) guide.

### Thanks to everyone that has already contributed

<a href="https://github.com/open-feature/dotnet-sdk/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=open-feature/dotnet-sdk" />
</a>

Made with [contrib.rocks](https://contrib.rocks).

## 📜 License

[Apache License 2.0](LICENSE)

[openfeature-website]: https://openfeature.dev
