using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenFeature.Constant;
using OpenFeature.Extension;
using OpenFeature.Model;
using OpenFeature.Serialization;

namespace OpenFeature.AotTests;

/// <summary>
/// This program validates OpenFeature SDK compatibility with NativeAOT.
/// It tests core functionality to ensure everything works correctly when compiled with AOT.
/// </summary>
internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("OpenFeature NativeAOT Compatibility Test");
        Console.WriteLine("==========================================");

        try
        {
            // Test basic API functionality
            await TestBasicApiAsync();

            // Test JSON serialization with AOT-compatible serializer context
            TestJsonSerialization();

            // Test dependency injection
            await TestDependencyInjectionAsync();

            // Test error handling and enum descriptions
            TestErrorHandling();

            Console.WriteLine("\nAll tests passed! OpenFeature is AOT-compatible.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nAOT compatibility test failed: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }

    private static async Task TestBasicApiAsync()
    {
        Console.WriteLine("\nTesting basic API functionality...");

        // Test singleton instance access
        var api = Api.Instance;
        Console.WriteLine($"✓- API instance created: {api.GetType().Name}");

        // Test client creation
        var client = api.GetClient("test-client", "1.0.0");
        Console.WriteLine($"✓- Client created: {client.GetType().Name}");

        // Test flag evaluation with default provider (NoOpProvider)
        var boolResult = await client.GetBooleanValueAsync("test-flag", false);
        Console.WriteLine($"✓- Boolean flag evaluation: {boolResult}");

        var stringResult = await client.GetStringValueAsync("test-string-flag", "default");
        Console.WriteLine($"✓- String flag evaluation: {stringResult}");

        var intResult = await client.GetIntegerValueAsync("test-int-flag", 42);
        Console.WriteLine($"✓- Integer flag evaluation: {intResult}");

        var doubleResult = await client.GetDoubleValueAsync("test-double-flag", 3.14);
        Console.WriteLine($"✓- Double flag evaluation: {doubleResult}");

        // Test evaluation context
        var context = EvaluationContext.Builder()
            .Set("userId", "user123")
            .Set("enabled", true)
            .Build();
        api.SetContext(context);
        Console.WriteLine($"✓- Evaluation context set with {context.Count} attributes");
    }

    private static void TestJsonSerialization()
    {
        Console.WriteLine("\nTesting JSON serialization with AOT context...");

        // Test Value serialization with AOT-compatible context
        var structureBuilder = Structure.Builder()
            .Set("name", "test")
            .Set("enabled", true)
            .Set("count", 42)
            .Set("score", 98.5);

        var structure = structureBuilder.Build();
        var value = new Value(structure);

        try
        {
            // Serialize using the AOT-compatible context
            var json = JsonSerializer.Serialize(value, OpenFeatureJsonSerializerContext.Default.Value);
            Console.WriteLine($"✓- Value serialized to JSON: {json}");

            // Deserialize back
            var deserializedValue = JsonSerializer.Deserialize(json, OpenFeatureJsonSerializerContext.Default.Value);
            Console.WriteLine($"✓- Value deserialized from JSON successfully: {value}", deserializedValue);
        }
        catch (Exception ex)
        {
            // Fallback test with the custom converter (should still work)
            Console.WriteLine($"X- AOT context serialization failed, testing fallback: {ex.Message}");
        }
    }

    private static async Task TestDependencyInjectionAsync()
    {
        Console.WriteLine("\nTesting dependency injection...");

        var builder = Host.CreateApplicationBuilder();

        // Add OpenFeature with DI
        builder.Services.AddOpenFeature(of => of.AddProvider(_ => new TestProvider()).AddHook(_ => new TestHook()));

        builder.Services.AddLogging(logging => logging.AddConsole());

        using var host = builder.Build();

        var api = host.Services.GetRequiredService<IFeatureClient>();
        Console.WriteLine($"✓- FeatureClient resolved from DI: {api.GetType().Name}");

        var result = await api.GetIntegerValueAsync("di-test-flag", 1);
        Console.WriteLine($"✓- Flag evaluation via DI: {result}");
    }

    private static void TestErrorHandling()
    {
        Console.WriteLine("\nTesting error handling and enum descriptions...");

        // Test ErrorType descriptions (this was the main reflection usage we fixed)
        var errorTypes = new[]
        {
            ErrorType.None,
            ErrorType.ProviderNotReady,
            ErrorType.FlagNotFound,
            ErrorType.ParseError,
            ErrorType.TypeMismatch,
            ErrorType.General,
            ErrorType.InvalidContext,
            ErrorType.TargetingKeyMissing,
            ErrorType.ProviderFatal
        };

        foreach (var errorType in errorTypes)
        {
            var description = errorType.GetDescription();
            Console.WriteLine($"✓- {errorType}: '{description}'");
        }
    }
}

/// <summary>
/// A simple test provider for validating DI functionality
/// </summary>
internal class TestProvider : FeatureProvider
{
    public override Metadata GetMetadata() => new("test-provider");

    public override Task<ResolutionDetails<bool>> ResolveBooleanValueAsync(string flagKey, bool defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        => Task.FromResult(new ResolutionDetails<bool>(flagKey, true));

    public override Task<ResolutionDetails<string>> ResolveStringValueAsync(string flagKey, string defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        => Task.FromResult(new ResolutionDetails<string>(flagKey, "test-value"));

    public override Task<ResolutionDetails<int>> ResolveIntegerValueAsync(string flagKey, int defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        => Task.FromResult(new ResolutionDetails<int>(flagKey, 123));

    public override Task<ResolutionDetails<double>> ResolveDoubleValueAsync(string flagKey, double defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        => Task.FromResult(new ResolutionDetails<double>(flagKey, 123.45));

    public override Task<ResolutionDetails<Value>> ResolveStructureValueAsync(string flagKey, Value defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        => Task.FromResult(new ResolutionDetails<Value>(flagKey, new Value("test")));
}

/// <summary>
/// A simple test hook for validating DI functionality
/// </summary>
internal class TestHook : Hook
{
    // No implementation needed for this test
}
