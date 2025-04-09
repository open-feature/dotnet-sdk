using System.Threading.Tasks;
using OpenFeature.E2ETests.Utils;
using Reqnroll;
using Xunit;

namespace OpenFeature.E2ETests.Steps;

[Binding]
[Scope(Feature = "Context merging precedence")]
public class ContextMergingPrecedenceStepDefinitions : BaseStepDefinitions
{
    public ContextMergingPrecedenceStepDefinitions(State state) : base(state)
    {
    }

    [When("Some flag was evaluated")]
    public async Task WhenSomeFlagWasEvaluated()
    {
        this.State.Flag = new FlagState("boolean-flag", "true".ToString(), FlagType.Boolean);
        this.State.FlagResult = await this.State.Client!.GetBooleanValueAsync("boolean-flag", true, this.State.InvocationEvaluationContext).ConfigureAwait(false);
    }

    [Then(@"The merged context contains an entry with key ""(.*)"" and value ""(.*)""")]
    public void ThenTheMergedContextContainsAnEntryWithKeyAndValue(string key, string value)
    {
        var mergedContext = this.State.EvaluationContext!;

        Assert.NotNull(mergedContext);

        var actualValue = mergedContext.GetValue(key);
        Assert.Contains(value, actualValue.AsString);
    }
}
