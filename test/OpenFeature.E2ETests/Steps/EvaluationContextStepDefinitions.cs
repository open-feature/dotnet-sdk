using OpenFeature.E2ETests.Utils;
using OpenFeature.Model;

namespace OpenFeature.E2ETests.Steps;

[Binding]
public class EvaluationContextStepDefinitions
{
    private readonly State _state;

    public EvaluationContextStepDefinitions(State state)
    {
        this._state = state;
    }

    [Given(@"A context entry with key ""(.*)"" and value ""(.*)"" is added to the ""(.*)"" level")]
    public void GivenAContextEntryWithKeyAndValueIsAddedToTheLevel(string key, string value, string level)
    {
        var context = EvaluationContext.Builder()
            .Set(key, value)
            .Build();

        this.InitializeContext(level, context);
    }

    [Given(@"a context containing a key ""(.*)"", with type ""(.*)"" and with value ""(.*)""")]
    public void GivenAContextContainingAKeyWithTypeAndWithValue(string key, string type, string value)
    {
        var context = EvaluationContext.Builder()
            .Merge(this._state.EvaluationContext ?? EvaluationContext.Empty);

        switch (type)
        {
            case "Integer":
                context = context.Set(key, int.Parse(value));
                break;
            case "Float":
                context = context.Set(key, double.Parse(value));
                break;
            case "String":
                context = context.Set(key, value);
                break;
            case "Boolean":
                context = context.Set(key, bool.Parse(value));
                break;
            case "Object":
                context = context.Set(key, new Value(value));
                break;
            default:
                Assert.Fail("FlagType not yet supported.");
                break;
        }

        this._state.EvaluationContext = context.Build();
    }

    [Given(@"Context entries for each level from API level down to the ""(.*)"" level, with key ""(.*)"" and value ""(.*)""")]
    public void GivenContextEntriesForEachLevelFromAPILevelDownToTheLevelWithKeyAndValue(string currentLevel, string key, string value)
    {
        if (this._state.ContextPrecedenceLevels == null)
            this._state.ContextPrecedenceLevels = new string[0];

        foreach (var level in this._state.ContextPrecedenceLevels)
        {
            var context = EvaluationContext.Builder()
                .Set(key, value)
                .Build();

            this.InitializeContext(level, context);
        }
    }

    [Given("A table with levels of increasing precedence")]
    public void GivenATableWithLevelsOfIncreasingPrecedence(DataTable dataTable)
    {
        var items = dataTable.Rows.ToList();

        var levels = items.Select(r => r.Values.First());

        this._state.ContextPrecedenceLevels = levels.ToArray();
    }

    [Then(@"The merged context contains an entry with key ""(.*)"" and value ""(.*)""")]
    public void ThenTheMergedContextContainsAnEntryWithKeyAndValue(string key, string value)
    {
        var provider = this._state.ContextStoringProvider;

        var mergedContext = provider!.EvaluationContext!;

        Assert.NotNull(mergedContext);

        var actualValue = mergedContext.GetValue(key);
        Assert.Contains(value, actualValue.AsString);
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
                    if (this._state.Client != null)
                    {
                        this._state.Client.SetContext(context);
                    }
                    else
                    {
                        throw new PendingStepException("You must initialise a FeatureClient before adding some EvaluationContext");
                    }
                    break;
                }
            case "Invocation":
                {
                    this._state.InvocationEvaluationContext = context;
                    break;
                }
            case "Before Hooks": // Assumed before hooks is the same as Invocation
                {
                    if (this._state.Client != null)
                    {
                        this._state.Client.AddHooks(new BeforeHook(context));
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
}
