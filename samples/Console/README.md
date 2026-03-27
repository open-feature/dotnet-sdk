# OpenFeature .NET Console Sample

This sample demonstrates how to use the OpenFeature .NET SDK in a console application. It includes a simple .NET 10 console app that defines several feature flags using the `InMemoryProvider` and evaluates them across all supported flag types: `bool`, `int`, `string`, `double`, and `object`.

The sample can easily be extended with alternative providers, which you can find in the [dotnet-sdk-contrib](https://github.com/open-feature/dotnet-sdk-contrib) repository.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) installed on your machine.

## Setup

1. Clone the repository:

   ```shell
   git clone https://github.com/open-feature/dotnet-sdk.git openfeature-dotnet-sdk
   ```

1. Navigate to the Console sample project directory:

   ```shell
   cd openfeature-dotnet-sdk/samples/Console
   ```

1. Run the following command to start the application:

   ```shell
   dotnet run app.cs
   ```

## Feature Flags

The sample defines the following flags using the `InMemoryProvider`:

| Flag Key       | Type     | Variants                                                          | Default Variant |
| -------------- | -------- | ----------------------------------------------------------------- | --------------- |
| `bool-flag`    | `bool`   | `on` → `true`, `off` → `false`                                   | `on`            |
| `numeric-flag` | `int`    | `one` → `1`, `two` → `2`                                         | `one`           |
| `string-flag`  | `string` | `greeting` → `"Hello, World!"`, `farewell` → `"Goodbye, World!"` | `greeting`      |
| `float-flag`   | `double` | `pi` → `3.14159`, `euler` → `0.577215`                           | `pi`            |
| `object-flag`  | `object` | `user1` → `"Ralph"`, `user2` → `"Lewis"`                         | `user2`         |

## NativeAOT

This sample is published with [NativeAOT](https://learn.microsoft.com/dotnet/core/deploying/native-aot/) enabled (`PublishAot=true`), demonstrating that the OpenFeature .NET SDK is fully compatible with NativeAOT compilation. See the [AOT Compatibility Guide](../../../docs/AOT_COMPATIBILITY.md) for more details.
