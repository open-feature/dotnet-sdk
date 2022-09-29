# OpenFeature SDK for .NET

[![a](https://img.shields.io/badge/slack-%40cncf%2Fopenfeature-brightgreen?style=flat&logo=slack)](https://cloud-native.slack.com/archives/C0344AANLA1)
[![spec version badge](https://img.shields.io/badge/Specification-v0.5.0-yellow)](https://github.com/open-feature/spec/tree/v0.5.0?rgh-link-date=2022-09-27T17%3A53%3A52Z)
[![codecov](https://codecov.io/gh/open-feature/dotnet-sdk/branch/main/graph/badge.svg?token=MONAVJBXUJ)](https://codecov.io/gh/open-feature/dotnet-sdk)
[![nuget](https://img.shields.io/nuget/vpre/OpenFeature)](https://www.nuget.org/packages/OpenFeature)
[![CII Best Practices](https://bestpractices.coreinfrastructure.org/projects/6250/badge)](https://bestpractices.coreinfrastructure.org/projects/6250)

OpenFeature is an open standard for feature flag management, created to support a robust feature flag ecosystem using cloud native technologies. OpenFeature will provide a unified API and SDK, and a developer-first, cloud-native implementation, with extensibility for open source and commercial offerings.

## Supported .Net Versions

The packages will aim to support all current .NET versions. Refer to the currently supported versions [.NET](https://dotnet.microsoft.com/download/dotnet) and [.NET Framework](https://dotnet.microsoft.com/download/dotnet-framework) Excluding .NET Framework 3.5

## Getting Started

### Basic Usage

```csharp
using OpenFeatureSDK;

// Sets the provider used by the client
OpenFeature.Instance.SetProvider(new NoOpProvider());
// Gets a instance of the feature flag client
var client = OpenFeature.Instance.GetClient();
// Evaluation the `my-feature` feature flag
var isEnabled = await client.GetBooleanValue("my-feature", false);
```

### Provider

To develop a provider, you need to create a new project and include the OpenFeature SDK as a dependency. This can be a new repository or included in an existing contrib repository available under the OpenFeature organization. Finally, you’ll then need to write the provider itself. In most languages, this can be accomplished by implementing the provider interface exported by the OpenFeature SDK.

Example of implementing a feature flag provider

```csharp
using OpenFeatureSDK;
using OpenFeatureSDK.Model;

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

### Hook

Hooks are a mechanism that allow for the addition of arbitrary behavior at well-defined points of the flag evaluation life-cycle. Use cases include validation of the resolved flag value, modifying or adding data to the evaluation context, logging, telemetry, and tracking.

Example of adding a hook

```csharp
// add a hook globally, to run on all evaluations
openFeature.AddHooks(new ExampleGlobalHook());

// add a hook on this client, to run on all evaluations made by this client
var client = OpenFeature.Instance.GetClient();
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

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for details on how to contribute to the OpenFeature project.

Our community meetings are held regularly and open to everyone. Check the [OpenFeature community calendar](https://calendar.google.com/calendar/u/0?cid=MHVhN2kxaGl2NWRoMThiMjd0b2FoNjM2NDRAZ3JvdXAuY2FsZW5kYXIuZ29vZ2xlLmNvbQ) for specific dates and for the Zoom meeting links.


Thanks so much for your contributions to the OpenFeature project.

<a href="https://github.com/open-feature/dotnet-sdk/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=open-feature/dotnet-sdk" />
</a>

Made with [contrib.rocks](https://contrib.rocks).

## License

[Apache License 2.0](LICENSE)
