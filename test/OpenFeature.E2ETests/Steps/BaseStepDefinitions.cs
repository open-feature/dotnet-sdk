using OpenFeature.E2ETests.Utils;
using OpenFeature.Model;
using OpenFeature.Providers.Memory;

namespace OpenFeature.E2ETests.Steps;

[Binding]
public class BaseStepDefinitions
{
    protected readonly State State;

    public BaseStepDefinitions(State state)
    {
        this.State = state;
    }

    [Given(@"a stable provider")]
    public async Task GivenAStableProvider()
    {
        var memProvider = new InMemoryProvider(E2EFlagConfig);
        await Api.Instance.SetProviderAsync(memProvider).ConfigureAwait(false);
        this.State.Client = Api.Instance.GetClient("TestClient", "1.0.0");
    }

    [Given(@"a Boolean-flag with key ""(.*)"" and a default value ""(.*)""")]
    [Given(@"a Boolean-flag with key ""(.*)"" and a fallback value ""(.*)""")]
    [Given(@"a boolean-flag with key ""(.*)"" and a default value ""(.*)""")]
    [Given(@"a boolean-flag with key ""(.*)"" and a fallback value ""(.*)""")]
    public void GivenABoolean_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        var flagState = new FlagState(key, defaultType, FlagType.Boolean);
        this.State.Flag = flagState;
    }

    [Given(@"a Float-flag with key ""(.*)"" and a default value ""(.*)""")]
    [Given(@"a Float-flag with key ""(.*)"" and a fallback value ""(.*)""")]
    [Given(@"a float-flag with key ""(.*)"" and a default value ""(.*)""")]
    [Given(@"a float-flag with key ""(.*)"" and a fallback value ""(.*)""")]
    public void GivenAFloat_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        var flagState = new FlagState(key, defaultType, FlagType.Float);
        this.State.Flag = flagState;
    }

    [Given(@"a Integer-flag with key ""(.*)"" and a default value ""(.*)""")]
    [Given(@"a Integer-flag with key ""(.*)"" and a fallback value ""(.*)""")]
    [Given(@"a integer-flag with key ""(.*)"" and a default value ""(.*)""")]
    [Given(@"a integer-flag with key ""(.*)"" and a fallback value ""(.*)""")]
    public void GivenAnInteger_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        var flagState = new FlagState(key, defaultType, FlagType.Integer);
        this.State.Flag = flagState;
    }

    [Given(@"a String-flag with key ""(.*)"" and a default value ""(.*)""")]
    [Given(@"a String-flag with key ""(.*)"" and a fallback value ""(.*)""")]
    [Given(@"a string-flag with key ""(.*)"" and a default value ""(.*)""")]
    [Given(@"a string-flag with key ""(.*)"" and a fallback value ""(.*)""")]
    public void GivenAString_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        var flagState = new FlagState(key, defaultType, FlagType.String);
        this.State.Flag = flagState;
    }

    [Given(@"a Object-flag with key ""(.*)"" and a fallback value ""(.*)""")]
    [Given(@"a object-flag with key ""(.*)"" and a fallback value ""(.*)""")]
    public void GivenAObject_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        Skip.If(true, "Object e2e test not supported");
    }

    [Given("a stable provider with retrievable context is registered")]
    public async Task GivenAStableProviderWithRetrievableContextIsRegistered()
    {
        this.State.ContextStoringProvider = new ContextStoringProvider();

        await Api.Instance.SetProviderAsync(this.State.ContextStoringProvider).ConfigureAwait(false);

        Api.Instance.SetTransactionContextPropagator(new AsyncLocalTransactionContextPropagator());

        this.State.Client = Api.Instance.GetClient("TestClient", "1.0.0");
    }

    [Given(@"A context entry with key ""(.*)"" and value ""(.*)"" is added to the ""(.*)"" level")]
    public void GivenAContextEntryWithKeyAndValueIsAddedToTheLevel(string key, string value, string level)
    {
        var context = EvaluationContext.Builder()
            .Set(key, value)
            .Build();

        this.InitializeContext(level, context);
    }

    [Given("A table with levels of increasing precedence")]
    public void GivenATableWithLevelsOfIncreasingPrecedence(DataTable dataTable)
    {
        var items = dataTable.Rows.ToList();

        var levels = items.Select(r => r.Values.First());

        this.State.ContextPrecedenceLevels = levels.ToArray();
    }

    [Given(@"Context entries for each level from API level down to the ""(.*)"" level, with key ""(.*)"" and value ""(.*)""")]
    public void GivenContextEntriesForEachLevelFromAPILevelDownToTheLevelWithKeyAndValue(string currentLevel, string key, string value)
    {
        if (this.State.ContextPrecedenceLevels == null)
            this.State.ContextPrecedenceLevels = new string[0];

        foreach (var level in this.State.ContextPrecedenceLevels)
        {
            var context = EvaluationContext.Builder()
                .Set(key, value)
                .Build();

            this.InitializeContext(level, context);
        }
    }

    [When(@"the flag was evaluated with details")]
    public async Task WhenTheFlagWasEvaluatedWithDetails()
    {
        var flag = this.State.Flag!;

        switch (flag.Type)
        {
            case FlagType.Boolean:
                this.State.FlagEvaluationDetailsResult = await this.State.Client!
                    .GetBooleanDetailsAsync(flag.Key, bool.Parse(flag.DefaultValue)).ConfigureAwait(false);
                break;
            case FlagType.Float:
                this.State.FlagEvaluationDetailsResult = await this.State.Client!
                    .GetDoubleDetailsAsync(flag.Key, double.Parse(flag.DefaultValue)).ConfigureAwait(false);
                break;
            case FlagType.Integer:
                this.State.FlagEvaluationDetailsResult = await this.State.Client!
                    .GetIntegerDetailsAsync(flag.Key, int.Parse(flag.DefaultValue)).ConfigureAwait(false);
                break;
            case FlagType.String:
                this.State.FlagEvaluationDetailsResult = await this.State.Client!.GetStringDetailsAsync(flag.Key, flag.DefaultValue)
                    .ConfigureAwait(false);
                break;
            case FlagType.Object:
                Skip.If(true, "Object e2e test not supported");
                break;
        }
    }
    private void InitializeContext(string level, EvaluationContext context)
    {
        switch (level)
        {
            case "API":
                {
                    Api.Instance.SetContext(context);
                    break;
                }
            case "Transaction":
                {
                    Api.Instance.SetTransactionContext(context);
                    break;
                }
            case "Client":
                {
                    if (this.State.Client != null)
                    {
                        this.State.Client.SetContext(context);
                    }
                    else
                    {
                        throw new PendingStepException("You must initialise a FeatureClient before adding some EvaluationContext");
                    }
                    break;
                }
            case "Invocation":
                {
                    this.State.InvocationEvaluationContext = context;
                    break;
                }
            case "Before Hooks": // Assumed before hooks is the same as Invocation
                {
                    if (this.State.Client != null)
                    {
                        this.State.Client.AddHooks(new BeforeHook(context));
                    }
                    else
                    {
                        throw new PendingStepException("You must initialise a FeatureClient before adding some EvaluationContext");
                    }

                    break;
                }
            default:
                throw new PendingStepException("Context level not defined");
        }
    }

    private static readonly IDictionary<string, Flag> E2EFlagConfig = new Dictionary<string, Flag>
    {
        {
            "boolean-flag", new Flag<bool>(
                variants: new Dictionary<string, bool> { { "on", true }, { "off", false } },
                defaultVariant: "on"
            )
        },
        {
            "boolean-disabled-flag", new Flag<bool>(
                variants: new Dictionary<string, bool> { { "on", true }, { "off", false } },
                defaultVariant: "on"
            )
        },
        {
            "boolean-targeted-zero-flag", new Flag<bool>(
                variants: new Dictionary<string, bool> { { "zero", false }, { "non-zero", true } },
                defaultVariant: "zero",
                contextEvaluator: (context) =>
                {
                    return context.GetValue("email").AsString == "ballmer@macrosoft.com" ? "zero" : "";
                }
            )
        },
        {
            "boolean-zero-flag", new Flag<bool>(
                variants: new Dictionary<string, bool> { { "zero", false }, { "non-zero", true } },
                defaultVariant: "zero"
            )
        },
        {
            "complex-targeted", new Flag<string>(
                variants: new Dictionary<string, string>() { { "internal", "INTERNAL" }, { "external", "EXTERNAL" } },
                defaultVariant: "external",
                (context) =>
                {
                    if (context.GetValue("email").AsString == "ballmer@macrosoft.com"
                        && context.GetValue("age").AsInteger > 10
                        && context.GetValue("customer").AsBoolean == false)
                    {
                        return "internal";
                    }
                    return "external";
                }
            )
        },
        {
            "float-flag", new Flag<double>(
                variants: new Dictionary<string, double>() { { "tenth", 0.1 }, { "half", 0.5 } },
                defaultVariant: "half"
            )
        },
        {
            "float-disabled-flag", new Flag<double>(
                variants: new Dictionary<string, double>() { { "tenth", 0.1 }, { "half", 0.5 } },
                defaultVariant: "half"
            )
        },
        {
            "float-targeted-zero-flag", new Flag<double>(
                variants: new Dictionary<string, double>() { { "zero", 0.0 }, { "non-zero", 1.0 } },
                defaultVariant: "zero",
                contextEvaluator: (context) =>
                {
                    return context.GetValue("email").AsString == "ballmer@macrosoft.com" ? "zero" : "";
                }
            )
        },
        {
            "float-zero-flag", new Flag<double>(
                variants: new Dictionary<string, double>() { { "zero", 0.0 }, { "non-zero", 1.0 } },
                defaultVariant: "zero"
            )
        },
        {
            "integer-flag", new Flag<int>(
                variants: new Dictionary<string, int>() { { "one", 1 }, { "ten", 10 } },
                defaultVariant: "ten"
            )
        },
        {
            "integer-disabled-flag", new Flag<int>(
                variants: new Dictionary<string, int>() { { "one", 1 }, { "ten", 10 } },
                defaultVariant: "ten"
            )
        },
        {
            "integer-targeted-zero-flag", new Flag<int>(
                variants: new Dictionary<string, int>() { { "zero", 0 }, { "non-zero", 1 } },
                defaultVariant: "zero",
                contextEvaluator: (context) =>
                {
                    return context.GetValue("email").AsString == "ballmer@macrosoft.com" ? "zero" : "";
                }
            )
        },
        {
            "integer-zero-flag", new Flag<int>(
                variants: new Dictionary<string, int>() { { "zero", 0 }, { "non-zero", 1 } },
                defaultVariant: "zero"
            )
        },
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
            "null-default-flag", new Flag<bool>(
                variants: new Dictionary<string, bool> { { "on", true }, { "off", false } },
                defaultVariant: null!
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
            "object-disabled-flag", new Flag<Value>(
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
            "object-targeted-zero-flag", new Flag<Value>(
                variants: new Dictionary<string, Value>()
                {
                    { "zero", new Value() },
                    {
                        "non-zero", new Value(Structure.Builder()
                            .Set("showImages", true)
                            .Set("title", "Check out these pics!")
                            .Set("imagesPerPage", 100).Build()
                        )
                    }
                },
                defaultVariant: "zero",
                contextEvaluator: (context) =>
                {
                    return context.GetValue("email").AsString == "ballmer@macrosoft.com" ? "zero" : "";
                }
            )
        },
        {
            "object-zero-flag", new Flag<Value>(
                variants: new Dictionary<string, Value>()
                {
                    { "zero", new Value() },
                    {
                        "non-zero", new Value(Structure.Builder()
                            .Set("showImages", true)
                            .Set("title", "Check out these pics!")
                            .Set("imagesPerPage", 100).Build()
                        )
                    }
                },
                defaultVariant: "zero"
            )
        },
        {
            "string-flag", new Flag<string>(
                variants: new Dictionary<string, string>() { { "greeting", "hi" }, { "parting", "bye" } },
                defaultVariant: "greeting"
            )
        },
        {
            "string-disabled-flag", new Flag<string>(
                variants: new Dictionary<string, string>() { { "greeting", "hi" }, { "parting", "bye" } },
                defaultVariant: "greeting"
            )
        },
        {
            "string-targeted-zero-flag", new Flag<string>(
                variants: new Dictionary<string, string>() { { "zero", "" }, { "non-zero", "str" } },
                defaultVariant: "zero",
                contextEvaluator: (context) =>
                {
                    return context.GetValue("email").AsString == "ballmer@macrosoft.com" ? "zero" : "";
                }
            )
        },
        {
            "string-zero-flag", new Flag<string>(
                variants: new Dictionary<string, string>() { { "zero", "" }, { "non-zero", "str" } },
                defaultVariant: "zero"
            )
        },
        {
            "undefined-default-flag", new Flag<int>(
                variants: new Dictionary<string, int>() { { "small", 10 }, { "big", 1000 } },
                defaultVariant: null!
            )
        },
        {
            "wrong-flag", new Flag<string>(
                variants: new Dictionary<string, string>() { { "one", "uno" }, { "two", "dos" } },
                defaultVariant: "one"
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
        }
    };

    public class BeforeHook : Hook
    {
        private readonly EvaluationContext context;

        public BeforeHook(EvaluationContext context)
        {
            this.context = context;
        }

        public override ValueTask<EvaluationContext> BeforeAsync<T>(HookContext<T> context, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            return new ValueTask<EvaluationContext>(this.context);
        }
    }
}
