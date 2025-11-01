using System.Text.Json;
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
        var options = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new FlagDictionaryJsonConverter());

        var json = File.ReadAllText(Path.Combine("Features", "test-flags.json"));
        var flags = JsonSerializer.Deserialize<Dictionary<string, Flag>>(json, options)
            ?? new Dictionary<string, Flag>();

        var memProvider = new InMemoryProvider(flags);
        await Api.Instance.SetProviderAsync(memProvider).ConfigureAwait(false);
        this.State.Client = Api.Instance.GetClient("TestClient", "1.0.0");
    }

    [Given(@"a (Boolean|boolean|Float|Integer|String|string|Object)(?:-flag)? with key ""(.*)"" and a default value ""(.*)""")]
    [Given(@"a (Boolean|boolean|Float|Integer|String|string|Object)(?:-flag)? with key ""(.*)"" and a fallback value ""(.*)""")]
    public void GivenAFlagType_FlagWithKeyAndADefaultValue(FlagType flagType, string key, string defaultType)
    {
        var flagState = new FlagState(key, defaultType, flagType);
        this.State.Flag = flagState;
    }

    [StepArgumentTransformation(@"^(Boolean|boolean|Float|Integer|String|string|Object)(?:-flag)?$")]
    public static FlagType TransformFlagType(string raw)
        => raw.Replace("-flag", "").ToLowerInvariant() switch
        {
            "boolean" => FlagType.Boolean,
            "float" => FlagType.Float,
            "integer" => FlagType.Integer,
            "string" => FlagType.String,
            "object" => FlagType.Object,
            _ => throw new Exception($"Unsupported flag type '{raw}'")
        };

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
                    .GetBooleanDetailsAsync(flag.Key, bool.Parse(flag.DefaultValue), this.State.EvaluationContext)
                    .ConfigureAwait(false);
                break;
            case FlagType.Float:
                this.State.FlagEvaluationDetailsResult = await this.State.Client!
                    .GetDoubleDetailsAsync(flag.Key, double.Parse(flag.DefaultValue), this.State.EvaluationContext)
                    .ConfigureAwait(false);
                break;
            case FlagType.Integer:
                this.State.FlagEvaluationDetailsResult = await this.State.Client!
                    .GetIntegerDetailsAsync(flag.Key, int.Parse(flag.DefaultValue), this.State.EvaluationContext)
                    .ConfigureAwait(false);
                break;
            case FlagType.String:
                this.State.FlagEvaluationDetailsResult = await this.State.Client!
                    .GetStringDetailsAsync(flag.Key, flag.DefaultValue, this.State.EvaluationContext)
                    .ConfigureAwait(false);
                break;
            case FlagType.Object:
                var defaultStructure = JsonStructureLoader.ParseJsonValue(flag.DefaultValue);
                this.State.FlagEvaluationDetailsResult = await this.State.Client!
                    .GetObjectDetailsAsync(flag.Key, new Value(defaultStructure), this.State.EvaluationContext)
                    .ConfigureAwait(false);
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
