#:project ../../src/OpenFeature/OpenFeature.csproj
#:property PublishAot=true

using OpenFeature;
using OpenFeature.Isolated;
using OpenFeature.Model;
using OpenFeature.Providers.Memory;

var flags = new Dictionary<string, Flag>
{
    { "bool-flag", new Flag<bool>(new Dictionary<string, bool> { { "on", true }, { "off", false } }, defaultVariant: "on") },
    { "numeric-flag", new Flag<int>(new Dictionary<string, int> { { "one", 1 }, { "two", 2 } }, defaultVariant: "one")  },
    { "string-flag", new Flag<string>(new Dictionary<string, string> { { "greeting", "Hello, World!" }, { "farewell", "Goodbye, World!" } }, defaultVariant: "greeting") },
    { "float-flag", new Flag<double>(new Dictionary<string, double> { { "pi", 3.14159 }, { "euler", 0.577215 } }, defaultVariant: "pi") },
    { "object-flag", new Flag<Value>(new Dictionary<string, Value> { { "user1", new Value("Ralph") }, { "user2", new Value("Lewis") } }, defaultVariant: "user2" ) }
};

await Api.Instance.SetProviderAsync(new InMemoryProvider(flags));

IFeatureClient client = Api.Instance.GetClient();

// Evaluate the `bool-flag` flag and print the result to the console
var helloWorldResult = await client.GetBooleanValueAsync("bool-flag", false);
if (helloWorldResult)
{
    Console.WriteLine("The `bool-flag` flag was enabled!");
}
else
{
    Console.WriteLine("The `bool-flag` flag was disabled!");
}

// Evaluate the `numeric-flag` flag and print the result to the console
var numericResult = await client.GetIntegerValueAsync("numeric-flag", 0);
Console.WriteLine("The `numeric-flag` flag returned {0}", numericResult);

// Evaluate the `string-flag` flag and print the result to the console
var stringResult = await client.GetStringValueAsync("string-flag", "default");
Console.WriteLine("The `string-flag` flag returned {0}", stringResult);

// Evaluate the `float-flag` flag and print the result to the console
var floatResult = await client.GetDoubleValueAsync("float-flag", 0.0);
Console.WriteLine("The `float-flag` flag returned {0}", floatResult);

// Evaluate the `object-flag` flag and print the result to the console
var objectResult = await client.GetObjectValueAsync("object-flag", new Value("Ben"));
Console.WriteLine("The `object-flag` flag returned {0}", objectResult.AsString);

// --- Isolated API Instance ---
// Create an independent API instance with its own provider and state.
// This is useful for testing, multi-tenant apps, and DI scenarios.

var isolatedFlags = new Dictionary<string, Flag>
{
    { "bool-flag", new Flag<bool>(new Dictionary<string, bool> { { "on", true }, { "off", false } }, defaultVariant: "off") },
    { "string-flag", new Flag<string>(new Dictionary<string, string> { { "greeting", "Howdy, Isolated World!" }, { "farewell", "See ya!" } }, defaultVariant: "greeting") },
};

#pragma warning disable OFISO001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var isolated = OpenFeatureFactory.CreateIsolated();
#pragma warning restore OFISO001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
await isolated.SetProviderAsync(new InMemoryProvider(isolatedFlags));

var isolatedClient = isolated.GetClient();

Console.WriteLine();
Console.WriteLine("--- Isolated API Instance ---");

// The isolated instance has its own flag values
var isolatedBool = await isolatedClient.GetBooleanValueAsync("bool-flag", true);
Console.WriteLine("Isolated `bool-flag`: {0} (singleton was: {1})", isolatedBool, helloWorldResult);

var isolatedString = await isolatedClient.GetStringValueAsync("string-flag", "default");
Console.WriteLine("Isolated `string-flag`: {0} (singleton was: {1})", isolatedString, stringResult);

// Shut down the isolated instance — does not affect the global singleton
await isolated.ShutdownAsync();
Console.WriteLine("Isolated instance shut down. Singleton is unaffected.");
