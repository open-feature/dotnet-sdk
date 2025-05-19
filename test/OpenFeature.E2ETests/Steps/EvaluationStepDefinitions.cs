using OpenFeature.Constant;
using OpenFeature.E2ETests.Utils;
using OpenFeature.Extension;
using OpenFeature.Model;
using Reqnroll;

namespace OpenFeature.E2ETests.Steps;

[Binding]
[Scope(Feature = "Flag evaluation")]
public class EvaluationStepDefinitions : BaseStepDefinitions
{
    public EvaluationStepDefinitions(State state) : base(state)
    {
    }

    [When(@"a boolean flag with key ""(.*)"" is evaluated with default value ""(.*)""")]
    public async Task Whenabooleanflagwithkeyisevaluatedwithdefaultvalue(string flagKey, bool defaultValue)
    {
        this.State.Flag = new FlagState(flagKey, defaultValue.ToString(), FlagType.Boolean);
        this.State.FlagResult = await this.State.Client!.GetBooleanValueAsync(flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then(@"the resolved boolean value should be ""(.*)""")]
    public void Thentheresolvedbooleanvalueshouldbe(bool expectedValue)
    {
        var result = this.State.FlagResult as bool?;
        Assert.Equal(expectedValue, result);
    }

    [When(@"a string flag with key ""(.*)"" is evaluated with default value ""(.*)""")]
    public async Task Whenastringflagwithkeyisevaluatedwithdefaultvalue(string flagKey, string defaultValue)
    {
        this.State.Flag = new FlagState(flagKey, defaultValue, FlagType.String);
        this.State.FlagResult = await this.State.Client!.GetStringValueAsync(flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then(@"the resolved string value should be ""(.*)""")]
    public void Thentheresolvedstringvalueshouldbe(string expected)
    {
        var result = this.State.FlagResult as string;
        Assert.Equal(expected, result);
    }

    [When(@"an integer flag with key ""(.*)"" is evaluated with default value (.*)")]
    public async Task Whenanintegerflagwithkeyisevaluatedwithdefaultvalue(string flagKey, int defaultValue)
    {
        this.State.Flag = new FlagState(flagKey, defaultValue.ToString(), FlagType.Integer);
        this.State.FlagResult = await this.State.Client!.GetIntegerValueAsync(flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then(@"the resolved integer value should be (.*)")]
    public void Thentheresolvedintegervalueshouldbe(int expected)
    {
        var result = this.State.FlagResult as int?;
        Assert.Equal(expected, result);
    }

    [When(@"a float flag with key ""(.*)"" is evaluated with default value (.*)")]
    public async Task Whenafloatflagwithkeyisevaluatedwithdefaultvalue(string flagKey, double defaultValue)
    {
        this.State.Flag = new FlagState(flagKey, defaultValue.ToString(), FlagType.Float);
        this.State.FlagResult = await this.State.Client!.GetDoubleValueAsync(flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then(@"the resolved float value should be (.*)")]
    public void Thentheresolvedfloatvalueshouldbe(double expected)
    {
        var result = this.State.FlagResult as double?;
        Assert.Equal(expected, result);
    }

    [When(@"an object flag with key ""(.*)"" is evaluated with a null default value")]
    public async Task Whenanobjectflagwithkeyisevaluatedwithanulldefaultvalue(string flagKey)
    {
        this.State.Flag = new FlagState(flagKey, null!, FlagType.Object);
        this.State.FlagResult = await this.State.Client!.GetObjectValueAsync(flagKey, new Value()).ConfigureAwait(false);
    }

    [Then(@"the resolved object value should be contain fields ""(.*)"", ""(.*)"", and ""(.*)"", with values ""(.*)"", ""(.*)"" and (.*), respectively")]
    public void Thentheresolvedobjectvalueshouldbecontainfieldsandwithvaluesandrespectively(string boolField, string stringField, string numberField, bool boolValue, string stringValue, int numberValue)
    {
        Value? value = this.State.FlagResult as Value;
        Assert.Equal(boolValue, value?.AsStructure?[boolField].AsBoolean);
        Assert.Equal(stringValue, value?.AsStructure?[stringField].AsString);
        Assert.Equal(numberValue, value?.AsStructure?[numberField].AsInteger);
    }

    [When(@"a boolean flag with key ""(.*)"" is evaluated with details and default value ""(.*)""")]
    public async Task Whenabooleanflagwithkeyisevaluatedwithdetailsanddefaultvalue(string flagKey, bool defaultValue)
    {
        this.State.Flag = new FlagState(flagKey, defaultValue.ToString(), FlagType.Boolean);
        this.State.FlagEvaluationDetailsResult = await this.State.Client!.GetBooleanDetailsAsync(flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then(@"the resolved boolean details value should be ""(.*)"", the variant should be ""(.*)"", and the reason should be ""(.*)""")]
    public void Thentheresolvedbooleandetailsvalueshouldbethevariantshouldbeandthereasonshouldbe(bool expectedValue, string expectedVariant, string expectedReason)
    {
        var result = this.State.FlagEvaluationDetailsResult as FlagEvaluationDetails<bool>;
        Assert.Equal(expectedValue, result?.Value);
        Assert.Equal(expectedVariant, result?.Variant);
        Assert.Equal(expectedReason, result?.Reason);
    }

    [When(@"a string flag with key ""(.*)"" is evaluated with details and default value ""(.*)""")]
    public async Task Whenastringflagwithkeyisevaluatedwithdetailsanddefaultvalue(string flagKey, string defaultValue)
    {
        this.State.Flag = new FlagState(flagKey, defaultValue, FlagType.String);
        this.State.FlagEvaluationDetailsResult = await this.State.Client!.GetStringDetailsAsync(flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then(@"the resolved string details value should be ""(.*)"", the variant should be ""(.*)"", and the reason should be ""(.*)""")]
    public void Thentheresolvedstringdetailsvalueshouldbethevariantshouldbeandthereasonshouldbe(string expectedValue, string expectedVariant, string expectedReason)
    {
        var result = this.State.FlagEvaluationDetailsResult as FlagEvaluationDetails<string>;
        Assert.Equal(expectedValue, result?.Value);
        Assert.Equal(expectedVariant, result?.Variant);
        Assert.Equal(expectedReason, result?.Reason);
    }

    [When(@"an integer flag with key ""(.*)"" is evaluated with details and default value (.*)")]
    public async Task Whenanintegerflagwithkeyisevaluatedwithdetailsanddefaultvalue(string flagKey, int defaultValue)
    {
        this.State.Flag = new FlagState(flagKey, defaultValue.ToString(), FlagType.Integer);
        this.State.FlagEvaluationDetailsResult = await this.State.Client!.GetIntegerDetailsAsync(flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then(@"the resolved integer details value should be (.*), the variant should be ""(.*)"", and the reason should be ""(.*)""")]
    public void Thentheresolvedintegerdetailsvalueshouldbethevariantshouldbeandthereasonshouldbe(int expectedValue, string expectedVariant, string expectedReason)
    {
        var result = this.State.FlagEvaluationDetailsResult as FlagEvaluationDetails<int>;
        Assert.Equal(expectedValue, result?.Value);
        Assert.Equal(expectedVariant, result?.Variant);
        Assert.Equal(expectedReason, result?.Reason);
    }

    [When(@"a float flag with key ""(.*)"" is evaluated with details and default value (.*)")]
    public async Task Whenafloatflagwithkeyisevaluatedwithdetailsanddefaultvalue(string flagKey, double defaultValue)
    {
        this.State.Flag = new FlagState(flagKey, defaultValue.ToString(), FlagType.Float);
        this.State.FlagEvaluationDetailsResult = await this.State.Client!.GetDoubleDetailsAsync(flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then(@"the resolved float details value should be (.*), the variant should be ""(.*)"", and the reason should be ""(.*)""")]
    public void Thentheresolvedfloatdetailsvalueshouldbethevariantshouldbeandthereasonshouldbe(double expectedValue, string expectedVariant, string expectedReason)
    {
        var result = this.State.FlagEvaluationDetailsResult as FlagEvaluationDetails<double>;
        Assert.Equal(expectedValue, result?.Value);
        Assert.Equal(expectedVariant, result?.Variant);
        Assert.Equal(expectedReason, result?.Reason);
    }

    [When(@"an object flag with key ""(.*)"" is evaluated with details and a null default value")]
    public async Task Whenanobjectflagwithkeyisevaluatedwithdetailsandanulldefaultvalue(string flagKey)
    {
        this.State.Flag = new FlagState(flagKey, null!, FlagType.Object);
        this.State.FlagEvaluationDetailsResult = await this.State.Client!.GetObjectDetailsAsync(flagKey, new Value()).ConfigureAwait(false);
    }

    [Then(@"the resolved object details value should be contain fields ""(.*)"", ""(.*)"", and ""(.*)"", with values ""(.*)"", ""(.*)"" and (.*), respectively")]
    public void Thentheresolvedobjectdetailsvalueshouldbecontainfieldsandwithvaluesandrespectively(string boolField, string stringField, string numberField, bool boolValue, string stringValue, int numberValue)
    {
        var result = this.State.FlagEvaluationDetailsResult as FlagEvaluationDetails<Value>;
        var value = result?.Value;
        Assert.NotNull(value);
        Assert.Equal(boolValue, value?.AsStructure?[boolField].AsBoolean);
        Assert.Equal(stringValue, value?.AsStructure?[stringField].AsString);
        Assert.Equal(numberValue, value?.AsStructure?[numberField].AsInteger);
    }

    [Then(@"the variant should be ""(.*)"", and the reason should be ""(.*)""")]
    public void Giventhevariantshouldbeandthereasonshouldbe(string expectedVariant, string expectedReason)
    {
        var result = this.State.FlagEvaluationDetailsResult as FlagEvaluationDetails<Value>;
        Assert.NotNull(result);
        Assert.Equal(expectedVariant, result?.Variant);
        Assert.Equal(expectedReason, result?.Reason);
    }

    [When(@"context contains keys ""(.*)"", ""(.*)"", ""(.*)"", ""(.*)"" with values ""(.*)"", ""(.*)"", (.*), ""(.*)""")]
    public void Whencontextcontainskeyswithvalues(string field1, string field2, string field3, string field4, string value1, string value2, int value3, string value4)
    {
        this.State.EvaluationContext = new EvaluationContextBuilder()
            .Set(field1, value1)
            .Set(field2, value2)
            .Set(field3, value3)
            .Set(field4, bool.Parse(value4)).Build();
    }

    [When(@"a flag with key ""(.*)"" is evaluated with default value ""(.*)""")]
    public async Task Givenaflagwithkeyisevaluatedwithdefaultvalue(string flagKey, string defaultValue)
    {
        this.State.Flag = new FlagState(flagKey, defaultValue, FlagType.String);
        this.State.FlagResult = await this.State.Client!.GetStringValueAsync(flagKey, defaultValue, this.State.EvaluationContext).ConfigureAwait(false);
    }

    [Then(@"the resolved string response should be ""(.*)""")]
    public void Thentheresolvedstringresponseshouldbe(string expected)
    {
        var result = this.State.FlagResult as string;
        Assert.Equal(expected, result);
    }

    [Then(@"the resolved flag value is ""(.*)"" when the context is empty")]
    public async Task Giventheresolvedflagvalueiswhenthecontextisempty(string expected)
    {
        var key = this.State.Flag!.Key;
        var defaultValue = this.State.Flag.DefaultValue;

        string? emptyContextValue = await this.State.Client!.GetStringValueAsync(key, defaultValue, EvaluationContext.Empty).ConfigureAwait(false);
        Assert.Equal(expected, emptyContextValue);
    }

    [When(@"a non-existent string flag with key ""(.*)"" is evaluated with details and a default value ""(.*)""")]
    public async Task Whenanonexistentstringflagwithkeyisevaluatedwithdetailsandadefaultvalue(string flagKey, string defaultValue)
    {
        this.State.Flag = new FlagState(flagKey, defaultValue, FlagType.String);
        this.State.FlagEvaluationDetailsResult = await this.State.Client!.GetStringDetailsAsync(flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then(@"the default string value should be returned")]
    public void Thenthedefaultstringvalueshouldbereturned()
    {
        var result = this.State.FlagEvaluationDetailsResult as FlagEvaluationDetails<string>;
        var defaultValue = this.State.Flag!.DefaultValue;
        Assert.Equal(defaultValue, result?.Value);
    }

    [Then(@"the reason should indicate an error and the error code should indicate a missing flag with ""(.*)""")]
    public void Giventhereasonshouldindicateanerrorandtheerrorcodeshouldindicateamissingflagwith(string errorCode)
    {
        var result = this.State.FlagEvaluationDetailsResult as FlagEvaluationDetails<string>;
        Assert.Equal(Reason.Error, result?.Reason);
        Assert.Equal(errorCode, result?.ErrorType.GetDescription());
    }

    [When(@"a string flag with key ""(.*)"" is evaluated as an integer, with details and a default value (.*)")]
    public async Task Whenastringflagwithkeyisevaluatedasanintegerwithdetailsandadefaultvalue(string flagKey, int defaultValue)
    {
        this.State.Flag = new FlagState(flagKey, defaultValue.ToString(), FlagType.String);
        this.State.FlagEvaluationDetailsResult = await this.State.Client!.GetIntegerDetailsAsync(flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then(@"the default integer value should be returned")]
    public void Thenthedefaultintegervalueshouldbereturned()
    {
        var result = this.State.FlagEvaluationDetailsResult as FlagEvaluationDetails<int>;
        var defaultValue = int.Parse(this.State.Flag!.DefaultValue);
        Assert.Equal(defaultValue, result?.Value);
    }

    [Then(@"the reason should indicate an error and the error code should indicate a type mismatch with ""(.*)""")]
    public void Giventhereasonshouldindicateanerrorandtheerrorcodeshouldindicateatypemismatchwith(string errorCode)
    {
        var result = this.State.FlagEvaluationDetailsResult as FlagEvaluationDetails<int>;
        Assert.Equal(Reason.Error, result?.Reason);
        Assert.Equal(errorCode, result?.ErrorType.GetDescription());
    }
}
