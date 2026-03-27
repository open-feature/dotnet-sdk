#:project ../../src/OpenFeature/OpenFeature.csproj
#:property PublishAot=true

using OpenFeature;
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
