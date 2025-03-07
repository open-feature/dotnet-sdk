using System.Threading.Tasks;
using OpenFeature.Constant;
using OpenFeature.E2ETests.Utils;
using OpenFeature.Extension;
using OpenFeature.Model;
using Reqnroll;
using Xunit;

namespace OpenFeature.E2ETests.Steps;

[Binding]
[Scope(Feature = "Flag evaluation")]
public class EvaluationStepDefinitions : BaseStepDefinitions
{
    public EvaluationStepDefinitions(State state) : base(state)
    {
    }

    private bool? _booleanFlagValue;
    private string? _stringFlagValue;
    private int? _intFlagValue;
    private double? _doubleFlagValue;
    private Value? _objectFlagValue;
    private FlagEvaluationDetails<bool>? _booleanFlagDetails;
    private FlagEvaluationDetails<string>? _stringFlagDetails;
    private FlagEvaluationDetails<int>? _intFlagDetails;
    private FlagEvaluationDetails<double>? _doubleFlagDetails;
    private FlagEvaluationDetails<Value>? _objectFlagDetails;
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
    public async Task Whenabooleanflagwithkeyisevaluatedwithdefaultvalue(string flagKey, bool defaultValue)
    {
        this._booleanFlagValue = await this.State.Client!.GetBooleanValueAsync(flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then(@"the resolved boolean value should be ""(.*)""")]
    public void Thentheresolvedbooleanvalueshouldbe(bool expectedValue)
    {
        Assert.Equal(expectedValue, this._booleanFlagValue);
    }

    [When(@"a string flag with key ""(.*)"" is evaluated with default value ""(.*)""")]
    public async Task Whenastringflagwithkeyisevaluatedwithdefaultvalue(string flagKey, string defaultValue)
    {
        this._stringFlagValue = await this.State.Client!.GetStringValueAsync(flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then(@"the resolved string value should be ""(.*)""")]
    public void Thentheresolvedstringvalueshouldbe(string expected)
    {
        Assert.Equal(expected, this._stringFlagValue);
    }

    [When(@"an integer flag with key ""(.*)"" is evaluated with default value (.*)")]
    public async Task Whenanintegerflagwithkeyisevaluatedwithdefaultvalue(string flagKey, int defaultValue)
    {
        this._intFlagValue = await this.State.Client!.GetIntegerValueAsync(flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then(@"the resolved integer value should be (.*)")]
    public void Thentheresolvedintegervalueshouldbe(int expected)
    {
        Assert.Equal(expected, this._intFlagValue);
    }

    [When(@"a float flag with key ""(.*)"" is evaluated with default value (.*)")]
    public async Task Whenafloatflagwithkeyisevaluatedwithdefaultvalue(string flagKey, double defaultValue)
    {
        this._doubleFlagValue = await this.State.Client!.GetDoubleValueAsync(flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then(@"the resolved float value should be (.*)")]
    public void Thentheresolvedfloatvalueshouldbe(double expected)
    {
        Assert.Equal(expected, this._doubleFlagValue);
    }

    [When(@"an object flag with key ""(.*)"" is evaluated with a null default value")]
    public async Task Whenanobjectflagwithkeyisevaluatedwithanulldefaultvalue(string flagKey)
    {
        this._objectFlagValue = await this.State.Client!.GetObjectValueAsync(flagKey, new Value()).ConfigureAwait(false);
    }

    [Then(@"the resolved object value should be contain fields ""(.*)"", ""(.*)"", and ""(.*)"", with values ""(.*)"", ""(.*)"" and (.*), respectively")]
    public void Thentheresolvedobjectvalueshouldbecontainfieldsandwithvaluesandrespectively(string boolField, string stringField, string numberField, bool boolValue, string stringValue, int numberValue)
    {
        Value? value = this._objectFlagValue;
        Assert.Equal(boolValue, value?.AsStructure?[boolField].AsBoolean);
        Assert.Equal(stringValue, value?.AsStructure?[stringField].AsString);
        Assert.Equal(numberValue, value?.AsStructure?[numberField].AsInteger);
    }

    [When(@"a boolean flag with key ""(.*)"" is evaluated with details and default value ""(.*)""")]
    public async Task Whenabooleanflagwithkeyisevaluatedwithdetailsanddefaultvalue(string flagKey, bool defaultValue)
    {
        this._booleanFlagDetails = await this.State.Client!.GetBooleanDetailsAsync(flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then(@"the resolved boolean details value should be ""(.*)"", the variant should be ""(.*)"", and the reason should be ""(.*)""")]
    public void Thentheresolvedbooleandetailsvalueshouldbethevariantshouldbeandthereasonshouldbe(bool expectedValue, string expectedVariant, string expectedReason)
    {
        var result = this._booleanFlagDetails;
        Assert.Equal(expectedValue, result?.Value);
        Assert.Equal(expectedVariant, result?.Variant);
        Assert.Equal(expectedReason, result?.Reason);
    }

    [When(@"a string flag with key ""(.*)"" is evaluated with details and default value ""(.*)""")]
    public async Task Whenastringflagwithkeyisevaluatedwithdetailsanddefaultvalue(string flagKey, string defaultValue)
    {
        this._stringFlagDetails = await this.State.Client!.GetStringDetailsAsync(flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then(@"the resolved string details value should be ""(.*)"", the variant should be ""(.*)"", and the reason should be ""(.*)""")]
    public void Thentheresolvedstringdetailsvalueshouldbethevariantshouldbeandthereasonshouldbe(string expectedValue, string expectedVariant, string expectedReason)
    {
        var result = this._stringFlagDetails;
        Assert.Equal(expectedValue, result?.Value);
        Assert.Equal(expectedVariant, result?.Variant);
        Assert.Equal(expectedReason, result?.Reason);
    }

    [When(@"an integer flag with key ""(.*)"" is evaluated with details and default value (.*)")]
    public async Task Whenanintegerflagwithkeyisevaluatedwithdetailsanddefaultvalue(string flagKey, int defaultValue)
    {
        this._intFlagDetails = await this.State.Client!.GetIntegerDetailsAsync(flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then(@"the resolved integer details value should be (.*), the variant should be ""(.*)"", and the reason should be ""(.*)""")]
    public void Thentheresolvedintegerdetailsvalueshouldbethevariantshouldbeandthereasonshouldbe(int expectedValue, string expectedVariant, string expectedReason)
    {
        var result = this._intFlagDetails;
        Assert.Equal(expectedValue, result?.Value);
        Assert.Equal(expectedVariant, result?.Variant);
        Assert.Equal(expectedReason, result?.Reason);
    }

    [When(@"a float flag with key ""(.*)"" is evaluated with details and default value (.*)")]
    public async Task Whenafloatflagwithkeyisevaluatedwithdetailsanddefaultvalue(string flagKey, double defaultValue)
    {
        this._doubleFlagDetails = await this.State.Client!.GetDoubleDetailsAsync(flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then(@"the resolved float details value should be (.*), the variant should be ""(.*)"", and the reason should be ""(.*)""")]
    public void Thentheresolvedfloatdetailsvalueshouldbethevariantshouldbeandthereasonshouldbe(double expectedValue, string expectedVariant, string expectedReason)
    {
        var result = this._doubleFlagDetails;
        Assert.Equal(expectedValue, result?.Value);
        Assert.Equal(expectedVariant, result?.Variant);
        Assert.Equal(expectedReason, result?.Reason);
    }

    [When(@"an object flag with key ""(.*)"" is evaluated with details and a null default value")]
    public async Task Whenanobjectflagwithkeyisevaluatedwithdetailsandanulldefaultvalue(string flagKey)
    {
        this._objectFlagDetails = await this.State.Client!.GetObjectDetailsAsync(flagKey, new Value()).ConfigureAwait(false);
    }

    [Then(@"the resolved object details value should be contain fields ""(.*)"", ""(.*)"", and ""(.*)"", with values ""(.*)"", ""(.*)"" and (.*), respectively")]
    public void Thentheresolvedobjectdetailsvalueshouldbecontainfieldsandwithvaluesandrespectively(string boolField, string stringField, string numberField, bool boolValue, string stringValue, int numberValue)
    {
        var value = this._objectFlagDetails?.Value;
        Assert.Equal(boolValue, value?.AsStructure?[boolField].AsBoolean);
        Assert.Equal(stringValue, value?.AsStructure?[stringField].AsString);
        Assert.Equal(numberValue, value?.AsStructure?[numberField].AsInteger);
    }

    [Then(@"the variant should be ""(.*)"", and the reason should be ""(.*)""")]
    public void Giventhevariantshouldbeandthereasonshouldbe(string expectedVariant, string expectedReason)
    {
        Assert.Equal(expectedVariant, this._objectFlagDetails?.Variant);
        Assert.Equal(expectedReason, this._objectFlagDetails?.Reason);
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
    public async Task Givenaflagwithkeyisevaluatedwithdefaultvalue(string flagKey, string defaultValue)
    {
        this._contextAwareFlagKey = flagKey;
        this._contextAwareDefaultValue = defaultValue;
        this._contextAwareValue = await this.State.Client!.GetStringValueAsync(flagKey, this._contextAwareDefaultValue, this._context).ConfigureAwait(false);
    }

    [Then(@"the resolved string response should be ""(.*)""")]
    public void Thentheresolvedstringresponseshouldbe(string expected)
    {
        Assert.Equal(expected, this._contextAwareValue);
    }

    [Then(@"the resolved flag value is ""(.*)"" when the context is empty")]
    public async Task Giventheresolvedflagvalueiswhenthecontextisempty(string expected)
    {
        string? emptyContextValue = await this.State.Client!.GetStringValueAsync(this._contextAwareFlagKey!, this._contextAwareDefaultValue!, EvaluationContext.Empty).ConfigureAwait(false);
        Assert.Equal(expected, emptyContextValue);
    }

    [When(@"a non-existent string flag with key ""(.*)"" is evaluated with details and a default value ""(.*)""")]
    public async Task Whenanonexistentstringflagwithkeyisevaluatedwithdetailsandadefaultvalue(string flagKey, string defaultValue)
    {
        this._notFoundFlagKey = flagKey;
        this._notFoundDefaultValue = defaultValue;
        this._notFoundDetails = await this.State.Client!.GetStringDetailsAsync(this._notFoundFlagKey, this._notFoundDefaultValue).ConfigureAwait(false);
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
    public async Task Whenastringflagwithkeyisevaluatedasanintegerwithdetailsandadefaultvalue(string flagKey, int defaultValue)
    {
        this._typeErrorFlagKey = flagKey;
        this._typeErrorDefaultValue = defaultValue;
        this._typeErrorDetails = await this.State.Client!.GetIntegerDetailsAsync(this._typeErrorFlagKey, this._typeErrorDefaultValue).ConfigureAwait(false);
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
