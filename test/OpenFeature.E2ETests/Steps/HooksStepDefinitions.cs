using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenFeature.Model;
using OpenFeature.Providers.Memory;
using Reqnroll;
using Xunit;

namespace OpenFeature.E2ETests.Steps;

[Binding]
[Scope(Feature = "Evaluation details through hooks")]
public class HooksStepDefinitions
{
    private FeatureClient? _client;
    private TestHook? _testHook;

    private static readonly IDictionary<string, Flag> E2EFlagConfig = new Dictionary<string, Flag>
    {
        {
            "boolean-flag", new Flag<bool>(
                variants: new Dictionary<string, bool> { { "on", true }, { "off", false } },
                defaultVariant: "on"
            )
        }
    };

    [Given(@"a stable provider")]
    public void GivenAStableProvider()
    {
        var memProvider = new InMemoryProvider(E2EFlagConfig);
        Api.Instance.SetProviderAsync(memProvider).Wait();
        this._client = Api.Instance.GetClient("TestClient", "1.0.0");
    }

    [Given(@"a client with added hook")]
    public void GivenAClientWithAddedHook()
    {
        this._testHook = new TestHook();
        this._client!.AddHooks(this._testHook);
    }

    [Given(@"a boolean-flag with key ""(.*)"" and a default value ""(.*)""")]
    public async Task GivenABoolean_FlagWithKeyAndADefaultValue(string key, string defaultValue)
    {
        _ = await this._client!.GetBooleanValueAsync(key, bool.Parse(defaultValue)).ConfigureAwait(false);
    }

    [When(@"the flag was evaluated with details")]
    public void WhenTheFlagWasEvaluatedWithDetails()
    {
        // This is a no-op, the flag evaluation is done in the Then step
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

    [Given(@"a string-flag with key ""(.*)"" and a default value ""(.*)""")]
    public async Task GivenAString_FlagWithKeyAndADefaultValue(string key, string defaultValue)
    {
        _ = await this._client!.GetStringValueAsync(key, defaultValue).ConfigureAwait(false);
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
                Assert.Equal(1, this._testHook!.BeforeCount);
                break;
            case "after":
                Assert.Equal(1, this._testHook!.AfterCount);
                break;
            case "error":
                Assert.Equal(1, this._testHook!.ErrorCount);
                break;
            case "finally":
                Assert.Equal(1, this._testHook!.FinallyCount);
                break;
        }
    }
}

class TestHook : Hook
{
    private int _afterCount;
    private int _beforeCount;
    private int _errorCount;
    private int _finallyCount;

    public override ValueTask AfterAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> details,
        IReadOnlyDictionary<string, object>? hints = null,
        CancellationToken cancellationToken = default)
    {
        this._afterCount++;
        return base.AfterAsync(context, details, hints, cancellationToken);
    }

    public override ValueTask ErrorAsync<T>(HookContext<T> context, Exception error,
        IReadOnlyDictionary<string, object>? hints = null,
        CancellationToken cancellationToken = default)
    {
        this._errorCount++;
        return base.ErrorAsync(context, error, hints, cancellationToken);
    }

    public override ValueTask FinallyAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> evaluationDetails,
        IReadOnlyDictionary<string, object>? hints = null,
        CancellationToken cancellationToken = default)
    {
        this._finallyCount++;
        return base.FinallyAsync(context, evaluationDetails, hints, cancellationToken);
    }

    public override ValueTask<EvaluationContext> BeforeAsync<T>(HookContext<T> context,
        IReadOnlyDictionary<string, object>? hints = null,
        CancellationToken cancellationToken = default)
    {
        this._beforeCount++;
        return base.BeforeAsync(context, hints, cancellationToken);
    }

    public int AfterCount => this._afterCount;
    public int BeforeCount => this._beforeCount;
    public int ErrorCount => this._errorCount;
    public int FinallyCount => this._finallyCount;
}
