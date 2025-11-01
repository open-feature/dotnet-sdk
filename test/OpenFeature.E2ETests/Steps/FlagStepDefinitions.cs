using OpenFeature.Constant;
using OpenFeature.E2ETests.Utils;
using OpenFeature.Model;

namespace OpenFeature.E2ETests.Steps;

[Binding]
public class FlagStepDefinitions
{
    private readonly State _state;

    public FlagStepDefinitions(State state)
    {
        this._state = state;
    }

    [Given(@"a (Boolean|boolean|Float|Integer|String|string|Object)(?:-flag)? with key ""(.*)"" and a default value ""(.*)""")]
    [Given(@"a (Boolean|boolean|Float|Integer|String|string|Object)(?:-flag)? with key ""(.*)"" and a fallback value ""(.*)""")]
    public void GivenAFlagType_FlagWithKeyAndADefaultValue(FlagType flagType, string key, string defaultType)
    {
        var flagState = new FlagState(key, defaultType, flagType);
        this._state.Flag = flagState;
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

    [When("Some flag was evaluated")]
    public async Task WhenSomeFlagWasEvaluated()
    {
        this._state.Flag = new FlagState("boolean-flag", "true", FlagType.Boolean);
        this._state.FlagResult = await this._state.Client!.GetBooleanValueAsync("boolean-flag", true, this._state.InvocationEvaluationContext).ConfigureAwait(false);
    }

    [When(@"the flag was evaluated with details")]
    public async Task WhenTheFlagWasEvaluatedWithDetails()
    {
        var flag = this._state.Flag!;

        switch (flag.Type)
        {
            case FlagType.Boolean:
                this._state.FlagEvaluationDetailsResult = await this._state.Client!
                    .GetBooleanDetailsAsync(flag.Key, bool.Parse(flag.DefaultValue), this._state.EvaluationContext)
                    .ConfigureAwait(false);
                break;
            case FlagType.Float:
                this._state.FlagEvaluationDetailsResult = await this._state.Client!
                    .GetDoubleDetailsAsync(flag.Key, double.Parse(flag.DefaultValue), this._state.EvaluationContext)
                    .ConfigureAwait(false);
                break;
            case FlagType.Integer:
                this._state.FlagEvaluationDetailsResult = await this._state.Client!
                    .GetIntegerDetailsAsync(flag.Key, int.Parse(flag.DefaultValue), this._state.EvaluationContext)
                    .ConfigureAwait(false);
                break;
            case FlagType.String:
                this._state.FlagEvaluationDetailsResult = await this._state.Client!
                    .GetStringDetailsAsync(flag.Key, flag.DefaultValue, this._state.EvaluationContext)
                    .ConfigureAwait(false);
                break;
            case FlagType.Object:
                var defaultStructure = JsonStructureLoader.ParseJsonValue(flag.DefaultValue);
                this._state.FlagEvaluationDetailsResult = await this._state.Client!
                    .GetObjectDetailsAsync(flag.Key, new Value(defaultStructure), this._state.EvaluationContext)
                    .ConfigureAwait(false);
                break;
        }
    }

    [Then(@"the resolved details value should be ""(.*)""")]
    public void ThenTheResolvedDetailsValueShouldBe(string value)
    {
        switch (this._state.Flag!.Type)
        {
            case FlagType.Integer:
                var intValue = int.Parse(value);
                AssertOnDetails<int>(r => Assert.Equal(intValue, r.Value));
                break;
            case FlagType.Float:
                var floatValue = double.Parse(value);
                AssertOnDetails<double>(r => Assert.Equal(floatValue, r.Value));
                break;
            case FlagType.String:
                var stringValue = value;
                AssertOnDetails<string>(r => Assert.Equal(stringValue, r.Value));
                break;
            case FlagType.Boolean:
                var booleanValue = bool.Parse(value);
                AssertOnDetails<bool>(r => Assert.Equal(booleanValue, r.Value));
                break;
            case FlagType.Object:
                var objectValue = JsonStructureLoader.ParseJsonValue(value);
                AssertOnDetails<Value>(r => Assert.Equal(new Value(objectValue), r.Value));
                break;
            default:
                Assert.Fail("FlagType not yet supported.");
                break;
        }
    }

    [Then(@"the reason should be ""(.*)""")]
    public void ThenTheReasonShouldBe(string reason)
    {
        switch (this._state.Flag!.Type)
        {
            case FlagType.Integer:
                AssertOnDetails<int>(r => Assert.Equal(reason, r.Reason));
                break;
            case FlagType.Float:
                AssertOnDetails<double>(r => Assert.Equal(reason, r.Reason));
                break;
            case FlagType.String:
                AssertOnDetails<string>(r => Assert.Equal(reason, r.Reason));
                break;
            case FlagType.Boolean:
                AssertOnDetails<bool>(r => Assert.Equal(reason, r.Reason));
                break;
            case FlagType.Object:
                AssertOnDetails<Value>(r => Assert.Equal(reason, r.Reason));
                break;
            default:
                Assert.Fail("FlagType not yet supported.");
                break;
        }
    }

    [Then(@"the error-code should be ""(.*)""")]
    public void ThenTheError_CodeShouldBe(string error)
    {
        var errorType = EnumHelpers.ParseFromDescription<ErrorType>(error);
        switch (this._state.Flag!.Type)
        {
            case FlagType.Integer:
                AssertOnDetails<int>(r => Assert.Equal(errorType, r.ErrorType));
                break;
            case FlagType.Float:
                AssertOnDetails<double>(r => Assert.Equal(errorType, r.ErrorType));
                break;
            case FlagType.String:
                AssertOnDetails<string>(r => Assert.Equal(errorType, r.ErrorType));
                break;
            case FlagType.Boolean:
                AssertOnDetails<bool>(r => Assert.Equal(errorType, r.ErrorType));
                break;
            case FlagType.Object:
                AssertOnDetails<Value>(r => Assert.Equal(errorType, r.ErrorType));
                break;
            default:
                Assert.Fail("FlagType not yet supported.");
                break;
        }
    }

    [Then(@"the flag key should be ""(.*)""")]
    public void ThenTheFlagKeyShouldBe(string key)
    {
        switch (this._state.Flag!.Type)
        {
            case FlagType.Integer:
                AssertOnDetails<int>(r => Assert.Equal(key, r.FlagKey));
                break;
            case FlagType.Float:
                AssertOnDetails<double>(r => Assert.Equal(key, r.FlagKey));
                break;
            case FlagType.String:
                AssertOnDetails<string>(r => Assert.Equal(key, r.FlagKey));
                break;
            case FlagType.Boolean:
                AssertOnDetails<bool>(r => Assert.Equal(key, r.FlagKey));
                break;
            case FlagType.Object:
                AssertOnDetails<Value>(r => Assert.Equal(key, r.FlagKey));
                break;
            default:
                Assert.Fail("FlagType not yet supported.");
                break;
        }
    }

    [Then(@"the variant should be ""(.*)""")]
    public void ThenTheVariantShouldBe(string variant)
    {
        switch (this._state.Flag!.Type)
        {
            case FlagType.Integer:
                AssertOnDetails<int>(r => Assert.Equal(variant, r.Variant));
                break;
            case FlagType.Float:
                AssertOnDetails<double>(r => Assert.Equal(variant, r.Variant));
                break;
            case FlagType.String:
                AssertOnDetails<string>(r => Assert.Equal(variant, r.Variant));
                break;
            case FlagType.Boolean:
                AssertOnDetails<bool>(r => Assert.Equal(variant, r.Variant));
                break;
            case FlagType.Object:
                AssertOnDetails<Value>(r => Assert.Equal(variant, r.Variant));
                break;
            default:
                Assert.Fail("FlagType not yet supported.");
                break;
        }
    }

    [Given(@"a context containing a key ""(.*)"" with null value")]
    public void GivenAContextContainingAKeyWithNullValue(string key)
    {
        this._state.EvaluationContext = EvaluationContext.Builder()
            .Merge(this._state.EvaluationContext ?? EvaluationContext.Empty)
            .Set(key, (string?)null!)
            .Build();
    }

    private void AssertOnDetails<T>(Action<FlagEvaluationDetails<T>> assertion)
    {
        var details = this._state.FlagEvaluationDetailsResult as FlagEvaluationDetails<T>;

        Assert.NotNull(details);
        assertion(details);
    }
}
