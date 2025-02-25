using Reqnroll;

namespace OpenFeature.E2ETests.Steps;

[Binding]
[Scope(Feature = "Evaluation details through hooks")]
public class HooksStepDefinitions
{
    [Given(@"a stable provider")]
    public void GivenAStableProvider()
    {
        ScenarioContext.StepIsPending();
    }

    [Given(@"a client with added hook")]
    public void GivenAClientWithAddedHook()
    {
        ScenarioContext.StepIsPending();
    }

    [Given(@"a boolean-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenABoolean_FlagWithKeyAndADefaultValue(string p0, string @false)
    {
        ScenarioContext.StepIsPending();
    }

    [When(@"the flag was evaluated with details")]
    public void WhenTheFlagWasEvaluatedWithDetails()
    {
        ScenarioContext.StepIsPending();
    }

    [Then(@"the ""(.*)"" hook should have been executed")]
    public void ThenTheHookShouldHaveBeenExecuted(string before)
    {
        ScenarioContext.StepIsPending();
    }

    [Then(@"the ""(.*)"" hooks should be called with evaluation details")]
    public void ThenTheHooksShouldBeCalledWithEvaluationDetails(string p0, Table table)
    {
        ScenarioContext.StepIsPending();
    }

    [Given(@"a string-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenAString_FlagWithKeyAndADefaultValue(string p0, string p1)
    {
        ScenarioContext.StepIsPending();
    }
}
