using OpenFeature.E2ETests.Utils;
using Reqnroll;
using Xunit;

namespace OpenFeature.E2ETests.Steps;

[Binding]
[Scope(Feature = "Evaluation details through hooks")]
public class HooksStepDefinitions : BaseStepDefinitions
{
    public HooksStepDefinitions(State state) : base(state)
    {
    }

    [Given(@"a client with added hook")]
    public void GivenAClientWithAddedHook()
    {
        this.State.TestHook = new TestHook();
        this.State.Client!.AddHooks(this.State.TestHook);
    }

    [Then(@"the ""(.*)"" hook should have been executed")]
    public void ThenTheHookShouldHaveBeenExecuted(string hook)
    {
        this.CheckHookExecution(hook);
    }

    [Then(@"the ""(.*)"" hooks should be called with evaluation details")]
    public void ThenTheHooksShouldBeCalledWithEvaluationDetails(string hook, Table table)
    {
        this.CheckHookExecution(hook);
        var key = table.Rows[0]["value"];
        switch (key)
        {
            case "boolean-flag":
                CheckCorrectFlag(table);
                break;
            case "missing-flag":
                CheckMissingFlag(table);
                break;
            case "wrong-flag":
                this.CheckWrongFlag(table);
                break;
        }
    }

    private static void CheckCorrectFlag(Table table)
    {
        Assert.Equal("string", table.Rows[0]["data_type"]);
        Assert.Equal("flag_key", table.Rows[0]["key"]);
        Assert.Equal("boolean-flag", table.Rows[0]["value"]);

        Assert.Equal("boolean", table.Rows[1]["data_type"]);
        Assert.Equal("value", table.Rows[1]["key"]);
        Assert.Equal("true", table.Rows[1]["value"]);

        Assert.Equal("string", table.Rows[2]["data_type"]);
        Assert.Equal("variant", table.Rows[2]["key"]);
        Assert.Equal("on", table.Rows[2]["value"]);

        Assert.Equal("string", table.Rows[3]["data_type"]);
        Assert.Equal("reason", table.Rows[3]["key"]);
        Assert.Equal("STATIC", table.Rows[3]["value"]);

        Assert.Equal("string", table.Rows[4]["data_type"]);
        Assert.Equal("error_code", table.Rows[4]["key"]);
        Assert.Equal("null", table.Rows[4]["value"]);
    }

    private static void CheckMissingFlag(Table table)
    {
        Assert.Equal("string", table.Rows[0]["data_type"]);
        Assert.Equal("flag_key", table.Rows[0]["key"]);
        Assert.Equal("missing-flag", table.Rows[0]["value"]);

        Assert.Equal("string", table.Rows[1]["data_type"]);
        Assert.Equal("value", table.Rows[1]["key"]);
        Assert.Equal("uh-oh", table.Rows[1]["value"]);

        Assert.Equal("string", table.Rows[2]["data_type"]);
        Assert.Equal("variant", table.Rows[2]["key"]);
        Assert.Equal("null", table.Rows[2]["value"]);

        Assert.Equal("string", table.Rows[3]["data_type"]);
        Assert.Equal("reason", table.Rows[3]["key"]);
        Assert.Equal("ERROR", table.Rows[3]["value"]);

        Assert.Equal("string", table.Rows[4]["data_type"]);
        Assert.Equal("error_code", table.Rows[4]["key"]);
        Assert.Equal("FLAG_NOT_FOUND", table.Rows[4]["value"]);
    }

    private void CheckWrongFlag(Table table)
    {
        Assert.Equal("string", table.Rows[0]["data_type"]);
        Assert.Equal("flag_key", table.Rows[0]["key"]);
        Assert.Equal("wrong-flag", table.Rows[0]["value"]);

        Assert.Equal("boolean", table.Rows[1]["data_type"]);
        Assert.Equal("value", table.Rows[1]["key"]);
        Assert.Equal("false", table.Rows[1]["value"]);

        Assert.Equal("string", table.Rows[2]["data_type"]);
        Assert.Equal("variant", table.Rows[2]["key"]);
        Assert.Equal("null", table.Rows[2]["value"]);

        Assert.Equal("string", table.Rows[3]["data_type"]);
        Assert.Equal("reason", table.Rows[3]["key"]);
        Assert.Equal("ERROR", table.Rows[3]["value"]);

        Assert.Equal("string", table.Rows[4]["data_type"]);
        Assert.Equal("error_code", table.Rows[4]["key"]);
        Assert.Equal("TYPE_MISMATCH", table.Rows[4]["value"]);
    }

    private void CheckHookExecution(string hook)
    {
        switch (hook)
        {
            case "before":
                Assert.Equal(1, this.State.TestHook!.BeforeCount);
                break;
            case "after":
                Assert.Equal(1, this.State.TestHook!.AfterCount);
                break;
            case "error":
                Assert.Equal(1, this.State.TestHook!.ErrorCount);
                break;
            case "finally":
                Assert.Equal(1, this.State.TestHook!.FinallyCount);
                break;
        }
    }
}
