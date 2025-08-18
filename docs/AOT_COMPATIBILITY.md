# OpenFeature .NET SDK - NativeAOT Compatibility

The OpenFeature .NET SDK is compatible with .NET NativeAOT compilation, allowing you to create self-contained, native executables with faster startup times and lower memory usage.

## Compatibility Status

**Fully Compatible** - The SDK can be used in NativeAOT applications without any issues.

### What's AOT-Compatible

-   Core API functionality (`Api.Instance`, `GetClient()`, flag evaluations)
-   All built-in providers (`NoOpProvider`, etc.)
-   JSON serialization of `Value`, `Structure`, and `EvaluationContext`
-   Error handling and enum descriptions
-   Hook system
-   Event handling
-   Metrics collection
-   Dependency injection

## Using OpenFeature with NativeAOT

### 1. Project Configuration

To enable NativeAOT in your project, add these properties to your `.csproj` file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework> <!-- or net9.0 -->
    <OutputType>Exe</OutputType>

    <!-- Enable NativeAOT -->
    <PublishAot>true</PublishAot>
    <IsAotCompatible>true</IsAotCompatible>
    <InvariantGlobalization>true</InvariantGlobalization>
    <TrimMode>full</TrimMode>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="OpenFeature" Version="2.x.x" />
  </ItemGroup>
</Project>
```

### 2. Basic Usage

```csharp
using OpenFeature;
using OpenFeature.Model;

// Basic OpenFeature usage - fully AOT compatible
var api = Api.Instance;
var client = api.GetClient("my-app");

// All flag evaluation methods work
var boolFlag = await client.GetBooleanValueAsync("feature-enabled", false);
var stringFlag = await client.GetStringValueAsync("welcome-message", "Hello");
var intFlag = await client.GetIntegerValueAsync("max-items", 10);
```

### 3. JSON Serialization (Recommended)

For optimal AOT performance, use the provided `JsonSerializerContext`:

```csharp
using System.Text.Json;
using OpenFeature.Model;
using OpenFeature.Serialization;

var value = new Value(Structure.Builder()
    .Set("name", "test")
    .Set("enabled", true)
    .Build());

// Use AOT-compatible serialization
var json = JsonSerializer.Serialize(value, OpenFeatureJsonSerializerContext.Default.Value);
var deserialized = JsonSerializer.Deserialize(json, OpenFeatureJsonSerializerContext.Default.Value);
```

### 4. Publishing for NativeAOT

Build and publish your AOT application:

```bash
# Build with AOT analysis
dotnet build -c Release

# Publish as native executable
dotnet publish -c Release

# Run the native executable (example path for macOS ARM64)
./bin/Release/net9.0/osx-arm64/publish/MyApp
```

## Performance Benefits

NativeAOT compilation provides several benefits:

-   **Faster Startup**: Native executables start faster than JIT-compiled applications
-   **Lower Memory Usage**: Reduced memory footprint
-   **Self-Contained**: No .NET runtime dependency required
-   **Smaller Deployment**: Optimized for size with trimming

## Testing AOT Compatibility

The SDK includes an AOT compatibility test project at `test/OpenFeature.AotCompatibility/` that:

-   Tests all core SDK functionality
-   Validates JSON serialization with source generation
-   Verifies error handling works correctly
-   Can be compiled and run as a native executable

Run the test:

```bash
cd test/OpenFeature.AotCompatibility
dotnet publish -c Release
./bin/Release/net9.0/[runtime]/publish/OpenFeature.AotCompatibility
```

## Limitations

Currently, there are no known limitations when using OpenFeature with NativeAOT. All core functionality is fully supported.

## Provider Compatibility

When using third-party providers, ensure they are also AOT-compatible. Check the provider's documentation for AOT support.

## Troubleshooting

### Trimming Warnings

If you encounter trimming warnings, you can:

1. Use the provided `JsonSerializerContext` for JSON operations
2. Ensure your providers are AOT-compatible
3. Add appropriate `[DynamicallyAccessedMembers]` attributes if needed

### Build Issues

-   Ensure you're targeting .NET 8.0 or later
-   Verify all dependencies support NativeAOT
-   Check that `PublishAot` is set to `true`

## Migration Guide

If migrating from a non-AOT setup:

1. **JSON Serialization**: Replace direct `JsonSerializer` calls with the provided context
2. **Reflection**: The SDK no longer uses reflection, but ensure your custom code doesn't
3. **Dynamic Loading**: Avoid dynamic assembly loading; register providers at compile time

## Example AOT Application

See the complete example in `test/OpenFeature.AotCompatibility/Program.cs` for a working AOT application that demonstrates all SDK features.
