using System.Threading.Tasks;
using OpenFeature.Constant;
using OpenFeature.Extension;
using OpenFeature.Model;
using Reqnroll;
using Xunit;

namespace OpenFeature.E2ETests.Steps;

[Binding]
[Scope(Feature = "Flag evaluation")]
public class EvaluationStepDefinitions : BaseStepDefinitions
{
    private Task<bool>? _booleanFlagValue;
    private Task<string>? _stringFlagValue;
    private Task<int>? _intFlagValue;
    private Task<double>? _doubleFlagValue;
    private Task<Value>? _objectFlagValue;
    private Task<FlagEvaluationDetails<bool>>? _booleanFlagDetails;
    private Task<FlagEvaluationDetails<string>>? _stringFlagDetails;
    private Task<FlagEvaluationDetails<int>>? _intFlagDetails;
    private Task<FlagEvaluationDetails<double>>? _doubleFlagDetails;
    private Task<FlagEvaluationDetails<Value>>? _objectFlagDetails;
    private string? _contextAwareFlagKey;
    private string? _contextAwareDefaultValue;
    private string? _contextAwareValue;
    private EvaluationContext? _context;
    private string? _notFoundFlagKey;
    private string? _notFoundDefaultValue;
    private FlagEvaluationDetails<string>? _notFoundDetails;
    private string? _typeErrorFlagKey;
    private int _typeErrorDefaultValue;
    private FlagEvaluationDetails<int>? _typeErrorDetails;

    [When(@"a boolean flag with key ""(.*)"" is evaluated with default value ""(.*)""")]
    public void Whenabooleanflagwithkeyisevaluatedwithdefaultvalue(string flagKey, bool defaultValue)
    {
        this._booleanFlagValue = this.Client?.GetBooleanValueAsync(flagKey, defaultValue);
    }

    [Then(@"the resolved boolean value should be ""(.*)""")]
    public void Thentheresolvedbooleanvalueshouldbe(bool expectedValue)
    {
        Assert.Equal(expectedValue, this._booleanFlagValue?.Result);
    }

    [When(@"a string flag with key ""(.*)"" is evaluated with default value ""(.*)""")]
    public void Whenastringflagwithkeyisevaluatedwithdefaultvalue(string flagKey, string defaultValue)
    {
        this._stringFlagValue = this.Client?.GetStringValueAsync(flagKey, defaultValue);
    }

    [Then(@"the resolved string value should be ""(.*)""")]
    public void Thentheresolvedstringvalueshouldbe(string expected)
    {
        Assert.Equal(expected, this._stringFlagValue?.Result);
    }

    [When(@"an integer flag with key ""(.*)"" is evaluated with default value (.*)")]
    public void Whenanintegerflagwithkeyisevaluatedwithdefaultvalue(string flagKey, int defaultValue)
    {
        this._intFlagValue = this.Client?.GetIntegerValueAsync(flagKey, defaultValue);
    }

    [Then(@"the resolved integer value should be (.*)")]
    public void Thentheresolvedintegervalueshouldbe(int expected)
    {
        Assert.Equal(expected, this._intFlagValue?.Result);
    }

    [When(@"a float flag with key ""(.*)"" is evaluated with default value (.*)")]
    public void Whenafloatflagwithkeyisevaluatedwithdefaultvalue(string flagKey, double defaultValue)
    {
        this._doubleFlagValue = this.Client?.GetDoubleValueAsync(flagKey, defaultValue);
    }

    [Then(@"the resolved float value should be (.*)")]
    public void Thentheresolvedfloatvalueshouldbe(double expected)
    {
        Assert.Equal(expected, this._doubleFlagValue?.Result);
    }

    [When(@"an object flag with key ""(.*)"" is evaluated with a null default value")]
    public void Whenanobjectflagwithkeyisevaluatedwithanulldefaultvalue(string flagKey)
    {
        this._objectFlagValue = this.Client?.GetObjectValueAsync(flagKey, new Value());
    }

    [Then(@"the resolved object value should be contain fields ""(.*)"", ""(.*)"", and ""(.*)"", with values ""(.*)"", ""(.*)"" and (.*), respectively")]
    public void Thentheresolvedobjectvalueshouldbecontainfieldsandwithvaluesandrespectively(string boolField, string stringField, string numberField, bool boolValue, string stringValue, int numberValue)
    {
        Value? value = this._objectFlagValue?.Result;
        Assert.Equal(boolValue, value?.AsStructure?[boolField].AsBoolean);
        Assert.Equal(stringValue, value?.AsStructure?[stringField].AsString);
        Assert.Equal(numberValue, value?.AsStructure?[numberField].AsInteger);
    }

    [When(@"a boolean flag with key ""(.*)"" is evaluated with details and default value ""(.*)""")]
    public void Whenabooleanflagwithkeyisevaluatedwithdetailsanddefaultvalue(string flagKey, bool defaultValue)
    {
        this._booleanFlagDetails = this.Client?.GetBooleanDetailsAsync(flagKey, defaultValue);
    }

    [Then(@"the resolved boolean details value should be ""(.*)"", the variant should be ""(.*)"", and the reason should be ""(.*)""")]
    public void Thentheresolvedbooleandetailsvalueshouldbethevariantshouldbeandthereasonshouldbe(bool expectedValue, string expectedVariant, string expectedReason)
    {
        var result = this._booleanFlagDetails?.Result;
        Assert.Equal(expectedValue, result?.Value);
        Assert.Equal(expectedVariant, result?.Variant);
        Assert.Equal(expectedReason, result?.Reason);
    }

    [When(@"a string flag with key ""(.*)"" is evaluated with details and default value ""(.*)""")]
    public void Whenastringflagwithkeyisevaluatedwithdetailsanddefaultvalue(string flagKey, string defaultValue)
    {
        this._stringFlagDetails = this.Client?.GetStringDetailsAsync(flagKey, defaultValue);
    }

    [Then(@"the resolved string details value should be ""(.*)"", the variant should be ""(.*)"", and the reason should be ""(.*)""")]
    public void Thentheresolvedstringdetailsvalueshouldbethevariantshouldbeandthereasonshouldbe(string expectedValue, string expectedVariant, string expectedReason)
    {
        var result = this._stringFlagDetails?.Result;
        Assert.Equal(expectedValue, result?.Value);
        Assert.Equal(expectedVariant, result?.Variant);
        Assert.Equal(expectedReason, result?.Reason);
    }

    [When(@"an integer flag with key ""(.*)"" is evaluated with details and default value (.*)")]
    public void Whenanintegerflagwithkeyisevaluatedwithdetailsanddefaultvalue(string flagKey, int defaultValue)
    {
        this._intFlagDetails = this.Client?.GetIntegerDetailsAsync(flagKey, defaultValue);
    }

    [Then(@"the resolved integer details value should be (.*), the variant should be ""(.*)"", and the reason should be ""(.*)""")]
    public void Thentheresolvedintegerdetailsvalueshouldbethevariantshouldbeandthereasonshouldbe(int expectedValue, string expectedVariant, string expectedReason)
    {
        var result = this._intFlagDetails?.Result;
        Assert.Equal(expectedValue, result?.Value);
        Assert.Equal(expectedVariant, result?.Variant);
        Assert.Equal(expectedReason, result?.Reason);
    }

    [When(@"a float flag with key ""(.*)"" is evaluated with details and default value (.*)")]
    public void Whenafloatflagwithkeyisevaluatedwithdetailsanddefaultvalue(string flagKey, double defaultValue)
    {
        this._doubleFlagDetails = this.Client?.GetDoubleDetailsAsync(flagKey, defaultValue);
    }

    [Then(@"the resolved float details value should be (.*), the variant should be ""(.*)"", and the reason should be ""(.*)""")]
    public void Thentheresolvedfloatdetailsvalueshouldbethevariantshouldbeandthereasonshouldbe(double expectedValue, string expectedVariant, string expectedReason)
    {
        var result = this._doubleFlagDetails?.Result;
        Assert.Equal(expectedValue, result?.Value);
        Assert.Equal(expectedVariant, result?.Variant);
        Assert.Equal(expectedReason, result?.Reason);
    }

    [When(@"an object flag with key ""(.*)"" is evaluated with details and a null default value")]
    public void Whenanobjectflagwithkeyisevaluatedwithdetailsandanulldefaultvalue(string flagKey)
    {
        this._objectFlagDetails = this.Client?.GetObjectDetailsAsync(flagKey, new Value());
    }

    [Then(@"the resolved object details value should be contain fields ""(.*)"", ""(.*)"", and ""(.*)"", with values ""(.*)"", ""(.*)"" and (.*), respectively")]
    public void Thentheresolvedobjectdetailsvalueshouldbecontainfieldsandwithvaluesandrespectively(string boolField, string stringField, string numberField, bool boolValue, string stringValue, int numberValue)
    {
        var value = this._objectFlagDetails?.Result.Value;
        Assert.Equal(boolValue, value?.AsStructure?[boolField].AsBoolean);
        Assert.Equal(stringValue, value?.AsStructure?[stringField].AsString);
        Assert.Equal(numberValue, value?.AsStructure?[numberField].AsInteger);
    }

    [Then(@"the variant should be ""(.*)"", and the reason should be ""(.*)""")]
    public void Giventhevariantshouldbeandthereasonshouldbe(string expectedVariant, string expectedReason)
    {
        Assert.Equal(expectedVariant, this._objectFlagDetails?.Result.Variant);
        Assert.Equal(expectedReason, this._objectFlagDetails?.Result.Reason);
    }

    [When(@"context contains keys ""(.*)"", ""(.*)"", ""(.*)"", ""(.*)"" with values ""(.*)"", ""(.*)"", (.*), ""(.*)""")]
    public void Whencontextcontainskeyswithvalues(string field1, string field2, string field3, string field4, string value1, string value2, int value3, string value4)
    {
        this._context = new EvaluationContextBuilder()
            .Set(field1, value1)
            .Set(field2, value2)
            .Set(field3, value3)
            .Set(field4, bool.Parse(value4)).Build();
    }

    [When(@"a flag with key ""(.*)"" is evaluated with default value ""(.*)""")]
    public void Givenaflagwithkeyisevaluatedwithdefaultvalue(string flagKey, string defaultValue)
    {
        this._contextAwareFlagKey = flagKey;
        this._contextAwareDefaultValue = defaultValue;
        this._contextAwareValue = this.Client?.GetStringValueAsync(flagKey, this._contextAwareDefaultValue, this._context).Result;
    }

    [Then(@"the resolved string response should be ""(.*)""")]
    public void Thentheresolvedstringresponseshouldbe(string expected)
    {
        Assert.Equal(expected, this._contextAwareValue);
    }

    [Then(@"the resolved flag value is ""(.*)"" when the context is empty")]
    public void Giventheresolvedflagvalueiswhenthecontextisempty(string expected)
    {
        string? emptyContextValue = this.Client?.GetStringValueAsync(this._contextAwareFlagKey!, this._contextAwareDefaultValue!, EvaluationContext.Empty).Result;
        Assert.Equal(expected, emptyContextValue);
    }

    [When(@"a non-existent string flag with key ""(.*)"" is evaluated with details and a default value ""(.*)""")]
    public void Whenanonexistentstringflagwithkeyisevaluatedwithdetailsandadefaultvalue(string flagKey, string defaultValue)
    {
        this._notFoundFlagKey = flagKey;
        this._notFoundDefaultValue = defaultValue;
        this._notFoundDetails = this.Client?.GetStringDetailsAsync(this._notFoundFlagKey, this._notFoundDefaultValue).Result;
    }

    [Then(@"the default string value should be returned")]
    public void Thenthedefaultstringvalueshouldbereturned()
    {
        Assert.Equal(this._notFoundDefaultValue, this._notFoundDetails?.Value);
    }

    [Then(@"the reason should indicate an error and the error code should indicate a missing flag with ""(.*)""")]
    public void Giventhereasonshouldindicateanerrorandtheerrorcodeshouldindicateamissingflagwith(string errorCode)
    {
        Assert.Equal(Reason.Error, this._notFoundDetails?.Reason);
        Assert.Equal(errorCode, this._notFoundDetails?.ErrorType.GetDescription());
    }

    [When(@"a string flag with key ""(.*)"" is evaluated as an integer, with details and a default value (.*)")]
    public void Whenastringflagwithkeyisevaluatedasanintegerwithdetailsandadefaultvalue(string flagKey, int defaultValue)
    {
        this._typeErrorFlagKey = flagKey;
        this._typeErrorDefaultValue = defaultValue;
        this._typeErrorDetails = this.Client?.GetIntegerDetailsAsync(this._typeErrorFlagKey, this._typeErrorDefaultValue).Result;
    }

    [Then(@"the default integer value should be returned")]
    public void Thenthedefaultintegervalueshouldbereturned()
    {
        Assert.Equal(this._typeErrorDefaultValue, this._typeErrorDetails?.Value);
    }

    [Then(@"the reason should indicate an error and the error code should indicate a type mismatch with ""(.*)""")]
    public void Giventhereasonshouldindicateanerrorandtheerrorcodeshouldindicateatypemismatchwith(string errorCode)
    {
        Assert.Equal(Reason.Error, this._typeErrorDetails?.Reason);
        Assert.Equal(errorCode, this._typeErrorDetails?.ErrorType.GetDescription());
    }
}
