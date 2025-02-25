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
    public void ThenTheHookShouldHaveBeenExecuted(string before)
    {
        Assert.Equal(1, this._testHook!.BeforeCount);
    }

    [Then(@"the ""(.*)"" hooks should be called with evaluation details")]
    public void ThenTheHooksShouldBeCalledWithEvaluationDetails(string hook, Table table)
    {
        ScenarioContext.StepIsPending();
    }

    [Given(@"a string-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenAString_FlagWithKeyAndADefaultValue(string p0, string p1)
    {
        ScenarioContext.StepIsPending();
    }
}

class TestHook : Hook
{
    private int _afterCount;
    private int _beforeCount;
    private int _errorCount;
    private int _finallyCount;

    public override ValueTask AfterAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> details, IReadOnlyDictionary<string, object>? hints = null,
        CancellationToken cancellationToken = default)
    {
        this._afterCount++;
        return base.AfterAsync(context, details, hints, cancellationToken);
    }

    public override ValueTask ErrorAsync<T>(HookContext<T> context, Exception error, IReadOnlyDictionary<string, object>? hints = null,
        CancellationToken cancellationToken = default)
    {
        this._errorCount++;
        return base.ErrorAsync(context, error, hints, cancellationToken);
    }

    public override ValueTask FinallyAsync<T>(HookContext<T> context, FlagEvaluationDetails<T> evaluationDetails, IReadOnlyDictionary<string, object>? hints = null,
        CancellationToken cancellationToken = default)
    {
        this._finallyCount++;
        return base.FinallyAsync(context, evaluationDetails, hints, cancellationToken);
    }

    public override ValueTask<EvaluationContext> BeforeAsync<T>(HookContext<T> context, IReadOnlyDictionary<string, object>? hints = null,
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
