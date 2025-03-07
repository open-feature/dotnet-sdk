using System.Collections.Generic;
using System.Threading.Tasks;
using OpenFeature.E2ETests.Utils;
using OpenFeature.Model;
using OpenFeature.Providers.Memory;
using Reqnroll;

namespace OpenFeature.E2ETests.Steps;

[Binding]
public class BaseStepDefinitions
{
    internal object Result = null!;

    protected readonly State State;

    public BaseStepDefinitions(State state)
    {
        this.State = state;
    }

    [Given(@"a stable provider")]
    public void GivenAStableProvider()
    {
        var memProvider = new InMemoryProvider(E2EFlagConfig);
        Api.Instance.SetProviderAsync(memProvider).Wait();
        this.State.Client = Api.Instance.GetClient("TestClient", "1.0.0");
    }

    [Given(@"a Boolean-flag with key ""(.*)"" and a default value ""(.*)""")]
    [Given(@"a boolean-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenABoolean_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        var flagState = new FlagState(key, defaultType, FlagType.Boolean);
        this.State.Flag = flagState;
    }

    [Given(@"a Float-flag with key ""(.*)"" and a default value ""(.*)""")]
    [Given(@"a float-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenAFloat_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        var flagState = new FlagState(key, defaultType, FlagType.Float);
        this.State.Flag = flagState;
    }

    [Given(@"a Integer-flag with key ""(.*)"" and a default value ""(.*)""")]
    [Given(@"a integer-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenAnInteger_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        var flagState = new FlagState(key, defaultType, FlagType.Integer);
        this.State.Flag = flagState;
    }

    [Given(@"a String-flag with key ""(.*)"" and a default value ""(.*)""")]
    [Given(@"a string-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenAString_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        var flagState = new FlagState(key, defaultType, FlagType.String);
        this.State.Flag = flagState;
    }

    [When(@"the flag was evaluated with details")]
    public async Task WhenTheFlagWasEvaluatedWithDetails()
    {
        var flag = this.State.Flag!;

        switch (flag.Type)
        {
            case FlagType.Boolean:
                this.Result = await this.State.Client!
                    .GetBooleanDetailsAsync(flag.Key, bool.Parse(flag.DefaultValue)).ConfigureAwait(false);
                break;
            case FlagType.Float:
                this.Result = await this.State.Client!
                    .GetDoubleDetailsAsync(flag.Key, double.Parse(flag.DefaultValue)).ConfigureAwait(false);
                break;
            case FlagType.Integer:
                this.Result = await this.State.Client!
                    .GetIntegerDetailsAsync(flag.Key, int.Parse(flag.DefaultValue)).ConfigureAwait(false);
                break;
            case FlagType.String:
                this.Result = await this.State.Client!.GetStringDetailsAsync(flag.Key, flag.DefaultValue)
                    .ConfigureAwait(false);
                break;
        }
    }

    private static readonly IDictionary<string, Flag> E2EFlagConfig = new Dictionary<string, Flag>
    {
        {
            "metadata-flag", new Flag<bool>(
                variants: new Dictionary<string, bool> { { "on", true }, { "off", false } },
                defaultVariant: "on",
                flagMetadata: new ImmutableMetadata(new Dictionary<string, object>
                {
                    { "string", "1.0.2" }, { "integer", 2 }, { "float", 0.1 }, { "boolean", true }
                })
            )
        },
        {
            "boolean-flag", new Flag<bool>(
                variants: new Dictionary<string, bool> { { "on", true }, { "off", false } },
                defaultVariant: "on"
            )
        },
        {
            "string-flag", new Flag<string>(
                variants: new Dictionary<string, string>() { { "greeting", "hi" }, { "parting", "bye" } },
                defaultVariant: "greeting"
            )
        },
        {
            "integer-flag", new Flag<int>(
                variants: new Dictionary<string, int>() { { "one", 1 }, { "ten", 10 } },
                defaultVariant: "ten"
            )
        },
        {
            "float-flag", new Flag<double>(
                variants: new Dictionary<string, double>() { { "tenth", 0.1 }, { "half", 0.5 } },
                defaultVariant: "half"
            )
        },
        {
            "object-flag", new Flag<Value>(
                variants: new Dictionary<string, Value>()
                {
                    { "empty", new Value() },
                    {
                        "template", new Value(Structure.Builder()
                            .Set("showImages", true)
                            .Set("title", "Check out these pics!")
                            .Set("imagesPerPage", 100).Build()
                        )
                    }
                },
                defaultVariant: "template"
            )
        },
        {
            "context-aware", new Flag<string>(
                variants: new Dictionary<string, string>() { { "internal", "INTERNAL" }, { "external", "EXTERNAL" } },
                defaultVariant: "external",
                (context) =>
                {
                    if (context.GetValue("fn").AsString == "Sulisław"
                        && context.GetValue("ln").AsString == "Świętopełk"
                        && context.GetValue("age").AsInteger == 29
                        && context.GetValue("customer").AsBoolean == false)
                    {
                        return "internal";
                    }
                    else return "external";
                }
            )
        },
        {
            "wrong-flag", new Flag<string>(
                variants: new Dictionary<string, string>() { { "one", "uno" }, { "two", "dos" } },
                defaultVariant: "one"
            )
        }
    };
}
