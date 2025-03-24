using OpenFeature.E2ETests.Utils;
using Reqnroll;

namespace OpenFeature.E2ETests.Steps;

[Binding]
[Scope(Feature = "Context merging precedence")]
public class ContextMergingDefinitions : BaseStepDefinitions
{
    public ContextMergingDefinitions(State state) : base(state)
    {
    }

    [Given(@"a stable provider with retrievable context is registered")]
    public void GivenAStableProviderWithRetrievableContextIsRegistered()
    {
        this.GivenAStableProvider();
    }

    [Given(@"A context entry with key ""(.*)"" and value ""(.*)"" is added to the ""(.*)"" level")]
    public void GivenAContextEntryWithKeyAndValueIsAddedToTheLevel(string aPI, string p1, string aPI2)
    {
        ScenarioContext.StepIsPending();
    }

    [When(@"Some flag was evaluated")]
    public void WhenSomeFlagWasEvaluated()
    {
        ScenarioContext.StepIsPending();
    }

    [Then(@"The merged context contains an entry with key ""(.*)"" and value ""(.*)""")]
    public void ThenTheMergedContextContainsAnEntryWithKeyAndValue(string aPI, string p1)
    {
        ScenarioContext.StepIsPending();
    }

    [Given(@"A table with levels of increasing precedence")]
    public void GivenATableWithLevelsOfIncreasingPrecedence(Table table)
    {
        ScenarioContext.StepIsPending();
    }

    [Given(@"Context entries for each level from API level down to the ""(.*)"" level, with key ""(.*)"" and value ""(.*)""")]
    public void GivenContextEntriesForEachLevelFromApiLevelDownToTheLevelWithKeyAndValue(string aPI, string key, string aPI2)
    {
        ScenarioContext.StepIsPending();
    }
}
