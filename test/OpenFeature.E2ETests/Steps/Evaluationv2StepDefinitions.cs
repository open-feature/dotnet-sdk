using OpenFeature.Constant;
using OpenFeature.E2ETests.Utils;
using OpenFeature.Model;

namespace OpenFeature.E2ETests.Steps;

[Binding]
[Scope(Feature = "Flag Evaluations - Complete OpenFeature Specification Coverage")]
public class Evaluationv2StepDefinitions : BaseStepDefinitions
{
    public Evaluationv2StepDefinitions(State state) : base(state)
    {
    }

    [Then(@"the resolved details value should be ""(.*)""")]
    public void ThenTheResolvedDetailsValueShouldBe(string value)
    {
        switch (this.State.Flag!.Type)
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
                Skip.If(true, "Object e2e test not supported");
                break;
            default:
                Assert.Fail("FlagType not yet supported.");
                break;
        }
    }

    [Then(@"the reason should be ""(.*)""")]
    public void ThenTheReasonShouldBe(string reason)
    {
        switch (this.State.Flag!.Type)
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
                Skip.If(true, "Object e2e test not supported");
                break;
            default:
                Assert.Fail("FlagType not yet supported.");
                break;
        }
    }

    [Given(@"a context containing a key ""(.*)"", with type ""(.*)"" and with value ""(.*)""")]
    public void GivenAContextContainingAKeyWithTypeAndWithValue(string key, string type, string value)
    {
        Type expectedType = typeof(string);
        switch (type)
        {
            case "Integer":
                expectedType = typeof(int);
                break;
            case "Float":
                expectedType = typeof(double);
                break;
            case "String":
                break;
            case "Boolean":
                expectedType = typeof(bool);
                break;
            case "Object":
                Skip.If(true, "Object e2e test not supported");
                break;
            default:
                Assert.Fail("FlagType not yet supported.");
                break;
        }

        var structureBuilder = new StructureBuilder()
            .Set(key, new Value(Convert.ChangeType(value, expectedType)));

        foreach (var item in this.State.EvaluationContext ?? EvaluationContext.Empty)
        {
            structureBuilder.Set(item.Key, item.Value);
        }

        this.State.EvaluationContext = new EvaluationContext(structureBuilder.Build());
    }

    [Then(@"the error-code should be ""(.*)""")]
    public void ThenTheError_CodeShouldBe(string error)
    {
        var errorType = EnumHelpers.ParseFromDescription<ErrorType>(error);
        switch (this.State.Flag!.Type)
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
                Skip.If(true, "Object e2e test not supported");
                break;
            default:
                Assert.Fail("FlagType not yet supported.");
                break;
        }
    }

    [Then(@"the flag key should be ""(.*)""")]
    public void ThenTheFlagKeyShouldBe(string key)
    {
        switch (this.State.Flag!.Type)
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
                Skip.If(true, "Object e2e test not supported");
                break;
            default:
                Assert.Fail("FlagType not yet supported.");
                break;
        }
    }

    [Then(@"the variant should be ""(.*)""")]
    public void ThenTheVariantShouldBe(string variant)
    {
        switch (this.State.Flag!.Type)
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
                Skip.If(true, "Object e2e test not supported");
                break;
            default:
                Assert.Fail("FlagType not yet supported.");
                break;
        }
    }

    [Then("the resolved metadata should contain")]
    public void ThenTheResolvedMetadataShouldContain(DataTable dataTable)
    {
        switch (this.State.Flag!.Type)
        {
            case FlagType.Integer:
                AssertOnDetails<int>(r => AssertMetadataContains(dataTable, r));
                break;
            case FlagType.Float:
                AssertOnDetails<double>(r => AssertMetadataContains(dataTable, r));
                break;
            case FlagType.String:
                AssertOnDetails<string>(r => AssertMetadataContains(dataTable, r));
                break;
            case FlagType.Boolean:
                AssertOnDetails<bool>(r => AssertMetadataContains(dataTable, r));
                break;
            case FlagType.Object:
                Skip.If(true, "Object e2e test not supported");
                break;
            default:
                Assert.Fail("FlagType not yet supported.");
                break;
        }
    }

    [Given(@"a context containing a key ""(.*)"" with null value")]
    public void GivenAContextContainingAKeyWithNullValue(string key)
    {
        this.State.EvaluationContext = EvaluationContext.Builder()
            .Merge(this.State.EvaluationContext ?? EvaluationContext.Empty)
            .Set(key, (string?)null!)
            .Build();
    }

    [Then(@"the resolved details value should be ""(.*)""showImages\\\\""(.*)""title\\\\""(.*)""Check out these pics!\\\\""(.*)""imagesPerPage\\\\""(.*)""")]
    public void ThenTheResolvedDetailsValueShouldBeShowImagesTitleCheckOutThesePicsImagesPerPage(string p0, string p1, string p2, string p3, string p4)
    {
        throw new PendingStepException();
    }

    [Given("evaluation options containing specific hooks")]
    public void GivenEvaluationOptionsContainingSpecificHooks()
    {
        throw new PendingStepException();
    }

    [When("the flag was evaluated with details using the evaluation options")]
    public void WhenTheFlagWasEvaluatedWithDetailsUsingTheEvaluationOptions()
    {
        throw new PendingStepException();
    }

    [Then("the specified hooks should execute during evaluation")]
    public void ThenTheSpecifiedHooksShouldExecuteDuringEvaluation()
    {
        throw new PendingStepException();
    }

    [Then("the hook order should be maintained")]
    public void ThenTheHookOrderShouldBeMaintained()
    {
        throw new PendingStepException();
    }

    [Given("an evaluation context with modifiable data")]
    public void GivenAnEvaluationContextWithModifiableData()
    {
        throw new PendingStepException();
    }

    [Then("the original evaluation context should remain unmodified")]
    public void ThenTheOriginalEvaluationContextShouldRemainUnmodified()
    {
        throw new PendingStepException();
    }

    [Then("the evaluation details should be immutable")]
    public void ThenTheEvaluationDetailsShouldBeImmutable()
    {
        throw new PendingStepException();
    }

    [When("the flag was evaluated with details asynchronously")]
    public void WhenTheFlagWasEvaluatedWithDetailsAsynchronously()
    {
        throw new PendingStepException();
    }

    [Then("the evaluation should complete without blocking")]
    public void ThenTheEvaluationShouldCompleteWithoutBlocking()
    {
        throw new PendingStepException();
    }

    private void AssertOnDetails<T>(Action<FlagEvaluationDetails<T>> assertion)
    {
        var details = this.State.FlagEvaluationDetailsResult as FlagEvaluationDetails<T>;

        Assert.NotNull(details);
        assertion(details);
    }

    private static void AssertMetadataContains<T>(DataTable dataTable, FlagEvaluationDetails<T> details)
    {
        foreach (var row in dataTable.Rows)
        {
            var key = row[0];
            var metadataType = row[1];
            var expected = row[2];

            object expectedValue = metadataType switch
            {
                "String" => expected,
                "Integer" => int.Parse(expected),
                "Float" => double.Parse(expected),
                "Boolean" => bool.Parse(expected),
                _ => throw new ArgumentException("Unsupported metadata type"),
            };
            object? actualValue = metadataType switch
            {
                "String" => details.FlagMetadata!.GetString(key),
                "Integer" => details.FlagMetadata!.GetInt(key),
                "Float" => details.FlagMetadata!.GetDouble(key),
                "Boolean" => details.FlagMetadata!.GetBool(key),
                _ => throw new ArgumentException("Unsupported metadata type")
            };

            Assert.NotNull(actualValue);
            Assert.Equal(expectedValue, actualValue);
        }
    }
}
