# OpenFeature SDK for .NET

[![a](https://img.shields.io/badge/slack-%40cncf%2Fopenfeature-brightgreen?style=flat&logo=slack)](https://cloud-native.slack.com/archives/C0344AANLA1)
[![Project Status: WIP – Initial development is in progress, but there has not yet been a stable, usable release suitable for the public.](https://www.repostatus.org/badges/latest/wip.svg)]()
[![codecov](https://codecov.io/gh/open-feature/dotnet-sdk/branch/main/graph/badge.svg?token=MONAVJBXUJ)](https://codecov.io/gh/open-feature/dotnet-sdk)
[![nuget](https://img.shields.io/nuget/vpre/OpenFeature)](https://www.nuget.org/packages/OpenFeature)

OpenFeature is an open standard for feature flag management, created to support a robust feature flag ecosystem using cloud native technologies. OpenFeature will provide a unified API and SDK, and a developer-first, cloud-native implementation, with extensibility for open source and commercial offerings.

## Supported .Net Versions

The packages will aim to support all current .NET versions. Refer to the currently supported versions [.NET](https://dotnet.microsoft.com/download/dotnet) and [.NET Framework](https://dotnet.microsoft.com/download/dotnet-framework) Excluding .NET Framework 3.5

## Providers

| Provider    | Package Name |
| ----------- | ----------- |
| TBA | TBA       |

## Basic Usage

```csharp
OpenFeature.Instance.SetProvider(new NoOpProvider());
var client = OpenFeature.Instance.GetClient();

var isEnabled = await client.GetBooleanValue("my-feature", false);
```

## Contributors

Thanks so much to your contributions to the OpenFeature project.

<a href="https://github.com/open-feature/dotnet-sdk/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=open-feature/dotnet-sdk" />
</a>

Made with [contrib.rocks](https://contrib.rocks).
