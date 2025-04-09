using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenFeature.E2ETests.Utils;
using OpenFeature.Model;
using OpenFeature.Providers.Memory;
using Reqnroll;

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

    [Given("a stable provider with retrievable context is registered")]
    public void GivenAStableProviderWithRetrievableContextIsRegistered()
    {
        var memProvider = new InMemoryProvider(E2EFlagConfig);
        Api.Instance.SetProviderAsync(memProvider).Wait();

        var hook = new MockHook((ctx) => this.State.EvaluationContext = ctx);
        Api.Instance.AddHooks(hook);

        Api.Instance.SetTransactionContextPropagator(new AsyncLocalTransactionContextPropagator());

        this.State.Client = Api.Instance.GetClient("TestClient", "1.0.0");
    }

    [Given(@"A context entry with key ""(.*)"" and value ""(.*)"" is added to the ""(.*)"" level")]
    public void GivenAContextEntryWithKeyAndValueIsAddedToTheLevel(string key, string value, string level)
    {
        var context = EvaluationContext.Builder()
            .Set(key, value)
            .Build();

        this.InitialiseContext(level, context);
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

        foreach (var level in this.State.ContextPrecedenceLevels )
        {
            var context = EvaluationContext.Builder()
                .Set(key, value)
                .Build();

            this.InitialiseContext(level, context);
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
        }
    }
    private void InitialiseContext(string level, EvaluationContext context)
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
                    if (this.State.InvocationEvaluationContext != null)
                    {
                        this.State.InvocationEvaluationContext = EvaluationContext.Builder()
                            .Merge(context)
                            .Merge(this.State.InvocationEvaluationContext!)
                            .Build();
                    }
                    else
                    {
                        this.State.InvocationEvaluationContext = context;
                    }

                    break;
                }
            default:
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

    public class MockHook : Hook
    {
        private readonly Func<EvaluationContext, EvaluationContext> mergedFinallyContext;

        public MockHook(Func<EvaluationContext, EvaluationContext> mergedFinallyContext)
        {
            this.mergedFinallyContext = mergedFinallyContext;
        }

        public override ValueTask FinallyAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> evaluationDetails, IReadOnlyDictionary<string, object>? hints = null, CancellationToken cancellationToken = default)
        {
            this.mergedFinallyContext(context.EvaluationContext);

            return base.FinallyAsync(context, evaluationDetails, hints, cancellationToken);
        }
    }

}
