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
    private string _flagKey = null!;
    private string _defaultValue = null!;
    internal FlagType? FlagTypeEnum;
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
        this._flagKey = key;
        this._defaultValue = defaultType;
        this.FlagTypeEnum = FlagType.Boolean;
    }

    [Given(@"a Float-flag with key ""(.*)"" and a default value ""(.*)""")]
    [Given(@"a float-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenAFloat_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        this._flagKey = key;
        this._defaultValue = defaultType;
        this.FlagTypeEnum = FlagType.Float;
    }

    [Given(@"a Integer-flag with key ""(.*)"" and a default value ""(.*)""")]
    [Given(@"a integer-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenAnInteger_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        this._flagKey = key;
        this._defaultValue = defaultType;
        this.FlagTypeEnum = FlagType.Integer;
    }

    [Given(@"a String-flag with key ""(.*)"" and a default value ""(.*)""")]
    [Given(@"a string-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenAString_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        this._flagKey = key;
        this._defaultValue = defaultType;
        this.FlagTypeEnum = FlagType.String;
    }

    [When(@"the flag was evaluated with details")]
    public async Task WhenTheFlagWasEvaluatedWithDetails()
    {
        switch (this.FlagTypeEnum)
        {
            case FlagType.Boolean:
                this.Result = await this.State.Client!
                    .GetBooleanDetailsAsync(this._flagKey, bool.Parse(this._defaultValue)).ConfigureAwait(false);
                break;
            case FlagType.Float:
                this.Result = await this.State.Client!
                    .GetDoubleDetailsAsync(this._flagKey, double.Parse(this._defaultValue)).ConfigureAwait(false);
                break;
            case FlagType.Integer:
                this.Result = await this.State.Client!
                    .GetIntegerDetailsAsync(this._flagKey, int.Parse(this._defaultValue)).ConfigureAwait(false);
                break;
            case FlagType.String:
                this.Result = await this.State.Client!.GetStringDetailsAsync(this._flagKey, this._defaultValue)
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
