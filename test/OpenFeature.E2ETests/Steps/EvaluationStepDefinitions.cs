using System.Collections.Generic;
using System.Threading.Tasks;
using OpenFeature.Constant;
using OpenFeature.Extension;
using OpenFeature.Model;
using OpenFeature.Providers.Memory;
using TechTalk.SpecFlow;
using Xunit;

namespace OpenFeature.E2ETests
{
    [Binding]
    public class EvaluationStepDefinitions
    {
        private static FeatureClient? client;
        private Task<bool>? booleanFlagValue;
        private Task<string>? stringFlagValue;
        private Task<int>? intFlagValue;
        private Task<double>? doubleFlagValue;
        private Task<Value>? objectFlagValue;
        private Task<FlagEvaluationDetails<bool>>? booleanFlagDetails;
        private Task<FlagEvaluationDetails<string>>? stringFlagDetails;
        private Task<FlagEvaluationDetails<int>>? intFlagDetails;
        private Task<FlagEvaluationDetails<double>>? doubleFlagDetails;
        private Task<FlagEvaluationDetails<Value>>? objectFlagDetails;
        private string? contextAwareFlagKey;
        private string? contextAwareDefaultValue;
        private string? contextAwareValue;
        private EvaluationContext? context;
        private string? notFoundFlagKey;
        private string? notFoundDefaultValue;
        private FlagEvaluationDetails<string>? notFoundDetails;
        private string? typeErrorFlagKey;
        private int typeErrorDefaultValue;
        private FlagEvaluationDetails<int>? typeErrorDetails;

        public EvaluationStepDefinitions(ScenarioContext scenarioContext)
        {
        }

        [Given(@"a provider is registered")]
        public void GivenAProviderIsRegistered()
        {
            var memProvider = new InMemoryProvider(this.e2eFlagConfig);
            Api.Instance.SetProviderAsync(memProvider).Wait();
            client = Api.Instance.GetClient("TestClient", "1.0.0");
        }

        [When(@"a boolean flag with key ""(.*)"" is evaluated with default value ""(.*)""")]
        public void Whenabooleanflagwithkeyisevaluatedwithdefaultvalue(string flagKey, bool defaultValue)
        {
            this.booleanFlagValue = client?.GetBooleanValueAsync(flagKey, defaultValue);
        }

        [Then(@"the resolved boolean value should be ""(.*)""")]
        public void Thentheresolvedbooleanvalueshouldbe(bool expectedValue)
        {
            Assert.Equal(expectedValue, this.booleanFlagValue?.Result);
        }

        [When(@"a string flag with key ""(.*)"" is evaluated with default value ""(.*)""")]
        public void Whenastringflagwithkeyisevaluatedwithdefaultvalue(string flagKey, string defaultValue)
        {
            this.stringFlagValue = client?.GetStringValueAsync(flagKey, defaultValue);
        }

        [Then(@"the resolved string value should be ""(.*)""")]
        public void Thentheresolvedstringvalueshouldbe(string expected)
        {
            Assert.Equal(expected, this.stringFlagValue?.Result);
        }

        [When(@"an integer flag with key ""(.*)"" is evaluated with default value (.*)")]
        public void Whenanintegerflagwithkeyisevaluatedwithdefaultvalue(string flagKey, int defaultValue)
        {
            this.intFlagValue = client?.GetIntegerValueAsync(flagKey, defaultValue);
        }

        [Then(@"the resolved integer value should be (.*)")]
        public void Thentheresolvedintegervalueshouldbe(int expected)
        {
            Assert.Equal(expected, this.intFlagValue?.Result);
        }

        [When(@"a float flag with key ""(.*)"" is evaluated with default value (.*)")]
        public void Whenafloatflagwithkeyisevaluatedwithdefaultvalue(string flagKey, double defaultValue)
        {
            this.doubleFlagValue = client?.GetDoubleValueAsync(flagKey, defaultValue);
        }

        [Then(@"the resolved float value should be (.*)")]
        public void Thentheresolvedfloatvalueshouldbe(double expected)
        {
            Assert.Equal(expected, this.doubleFlagValue?.Result);
        }

        [When(@"an object flag with key ""(.*)"" is evaluated with a null default value")]
        public void Whenanobjectflagwithkeyisevaluatedwithanulldefaultvalue(string flagKey)
        {
            this.objectFlagValue = client?.GetObjectValueAsync(flagKey, new Value());
        }

        [Then(@"the resolved object value should be contain fields ""(.*)"", ""(.*)"", and ""(.*)"", with values ""(.*)"", ""(.*)"" and (.*), respectively")]
        public void Thentheresolvedobjectvalueshouldbecontainfieldsandwithvaluesandrespectively(string boolField, string stringField, string numberField, bool boolValue, string stringValue, int numberValue)
        {
            Value? value = this.objectFlagValue?.Result;
            Assert.Equal(boolValue, value?.AsStructure?[boolField].AsBoolean);
            Assert.Equal(stringValue, value?.AsStructure?[stringField].AsString);
            Assert.Equal(numberValue, value?.AsStructure?[numberField].AsInteger);
        }

        [When(@"a boolean flag with key ""(.*)"" is evaluated with details and default value ""(.*)""")]
        public void Whenabooleanflagwithkeyisevaluatedwithdetailsanddefaultvalue(string flagKey, bool defaultValue)
        {
            this.booleanFlagDetails = client?.GetBooleanDetailsAsync(flagKey, defaultValue);
        }

        [Then(@"the resolved boolean details value should be ""(.*)"", the variant should be ""(.*)"", and the reason should be ""(.*)""")]
        public void Thentheresolvedbooleandetailsvalueshouldbethevariantshouldbeandthereasonshouldbe(bool expectedValue, string expectedVariant, string expectedReason)
        {
            var result = this.booleanFlagDetails?.Result;
            Assert.Equal(expectedValue, result?.Value);
            Assert.Equal(expectedVariant, result?.Variant);
            Assert.Equal(expectedReason, result?.Reason);
        }

        [When(@"a string flag with key ""(.*)"" is evaluated with details and default value ""(.*)""")]
        public void Whenastringflagwithkeyisevaluatedwithdetailsanddefaultvalue(string flagKey, string defaultValue)
        {
            this.stringFlagDetails = client?.GetStringDetailsAsync(flagKey, defaultValue);
        }

        [Then(@"the resolved string details value should be ""(.*)"", the variant should be ""(.*)"", and the reason should be ""(.*)""")]
        public void Thentheresolvedstringdetailsvalueshouldbethevariantshouldbeandthereasonshouldbe(string expectedValue, string expectedVariant, string expectedReason)
        {
            var result = this.stringFlagDetails?.Result;
            Assert.Equal(expectedValue, result?.Value);
            Assert.Equal(expectedVariant, result?.Variant);
            Assert.Equal(expectedReason, result?.Reason);
        }

        [When(@"an integer flag with key ""(.*)"" is evaluated with details and default value (.*)")]
        public void Whenanintegerflagwithkeyisevaluatedwithdetailsanddefaultvalue(string flagKey, int defaultValue)
        {
            this.intFlagDetails = client?.GetIntegerDetailsAsync(flagKey, defaultValue);
        }

        [Then(@"the resolved integer details value should be (.*), the variant should be ""(.*)"", and the reason should be ""(.*)""")]
        public void Thentheresolvedintegerdetailsvalueshouldbethevariantshouldbeandthereasonshouldbe(int expectedValue, string expectedVariant, string expectedReason)
        {
            var result = this.intFlagDetails?.Result;
            Assert.Equal(expectedValue, result?.Value);
            Assert.Equal(expectedVariant, result?.Variant);
            Assert.Equal(expectedReason, result?.Reason);
        }

        [When(@"a float flag with key ""(.*)"" is evaluated with details and default value (.*)")]
        public void Whenafloatflagwithkeyisevaluatedwithdetailsanddefaultvalue(string flagKey, double defaultValue)
        {
            this.doubleFlagDetails = client?.GetDoubleDetailsAsync(flagKey, defaultValue);
        }

        [Then(@"the resolved float details value should be (.*), the variant should be ""(.*)"", and the reason should be ""(.*)""")]
        public void Thentheresolvedfloatdetailsvalueshouldbethevariantshouldbeandthereasonshouldbe(double expectedValue, string expectedVariant, string expectedReason)
        {
            var result = this.doubleFlagDetails?.Result;
            Assert.Equal(expectedValue, result?.Value);
            Assert.Equal(expectedVariant, result?.Variant);
            Assert.Equal(expectedReason, result?.Reason);
        }

        [When(@"an object flag with key ""(.*)"" is evaluated with details and a null default value")]
        public void Whenanobjectflagwithkeyisevaluatedwithdetailsandanulldefaultvalue(string flagKey)
        {
            this.objectFlagDetails = client?.GetObjectDetailsAsync(flagKey, new Value());
        }

        [Then(@"the resolved object details value should be contain fields ""(.*)"", ""(.*)"", and ""(.*)"", with values ""(.*)"", ""(.*)"" and (.*), respectively")]
        public void Thentheresolvedobjectdetailsvalueshouldbecontainfieldsandwithvaluesandrespectively(string boolField, string stringField, string numberField, bool boolValue, string stringValue, int numberValue)
        {
            var value = this.objectFlagDetails?.Result.Value;
            Assert.Equal(boolValue, value?.AsStructure?[boolField].AsBoolean);
            Assert.Equal(stringValue, value?.AsStructure?[stringField].AsString);
            Assert.Equal(numberValue, value?.AsStructure?[numberField].AsInteger);
        }

        [Then(@"the variant should be ""(.*)"", and the reason should be ""(.*)""")]
        public void Giventhevariantshouldbeandthereasonshouldbe(string expectedVariant, string expectedReason)
        {
            Assert.Equal(expectedVariant, this.objectFlagDetails?.Result.Variant);
            Assert.Equal(expectedReason, this.objectFlagDetails?.Result.Reason);
        }

        [When(@"context contains keys ""(.*)"", ""(.*)"", ""(.*)"", ""(.*)"" with values ""(.*)"", ""(.*)"", (.*), ""(.*)""")]
        public void Whencontextcontainskeyswithvalues(string field1, string field2, string field3, string field4, string value1, string value2, int value3, string value4)
        {
            this.context = new EvaluationContextBuilder()
                .Set(field1, value1)
                .Set(field2, value2)
                .Set(field3, value3)
                .Set(field4, bool.Parse(value4)).Build();
        }

        [When(@"a flag with key ""(.*)"" is evaluated with default value ""(.*)""")]
        public void Givenaflagwithkeyisevaluatedwithdefaultvalue(string flagKey, string defaultValue)
        {
            this.contextAwareFlagKey = flagKey;
            this.contextAwareDefaultValue = defaultValue;
            this.contextAwareValue = client?.GetStringValueAsync(flagKey, this.contextAwareDefaultValue, this.context)?.Result;
        }

        [Then(@"the resolved string response should be ""(.*)""")]
        public void Thentheresolvedstringresponseshouldbe(string expected)
        {
            Assert.Equal(expected, this.contextAwareValue);
        }

        [Then(@"the resolved flag value is ""(.*)"" when the context is empty")]
        public void Giventheresolvedflagvalueiswhenthecontextisempty(string expected)
        {
            string? emptyContextValue = client?.GetStringValueAsync(this.contextAwareFlagKey!, this.contextAwareDefaultValue!, EvaluationContext.Empty).Result;
            Assert.Equal(expected, emptyContextValue);
        }

        [When(@"a non-existent string flag with key ""(.*)"" is evaluated with details and a default value ""(.*)""")]
        public void Whenanonexistentstringflagwithkeyisevaluatedwithdetailsandadefaultvalue(string flagKey, string defaultValue)
        {
            this.notFoundFlagKey = flagKey;
            this.notFoundDefaultValue = defaultValue;
            this.notFoundDetails = client?.GetStringDetailsAsync(this.notFoundFlagKey, this.notFoundDefaultValue).Result;
        }

        [Then(@"the default string value should be returned")]
        public void Thenthedefaultstringvalueshouldbereturned()
        {
            Assert.Equal(this.notFoundDefaultValue, this.notFoundDetails?.Value);
        }

        [Then(@"the reason should indicate an error and the error code should indicate a missing flag with ""(.*)""")]
        public void Giventhereasonshouldindicateanerrorandtheerrorcodeshouldindicateamissingflagwith(string errorCode)
        {
            Assert.Equal(Reason.Error.ToString(), this.notFoundDetails?.Reason);
            Assert.Equal(errorCode, this.notFoundDetails?.ErrorType.GetDescription());
        }

        [When(@"a string flag with key ""(.*)"" is evaluated as an integer, with details and a default value (.*)")]
        public void Whenastringflagwithkeyisevaluatedasanintegerwithdetailsandadefaultvalue(string flagKey, int defaultValue)
        {
            this.typeErrorFlagKey = flagKey;
            this.typeErrorDefaultValue = defaultValue;
            this.typeErrorDetails = client?.GetIntegerDetailsAsync(this.typeErrorFlagKey, this.typeErrorDefaultValue).Result;
        }

        [Then(@"the default integer value should be returned")]
        public void Thenthedefaultintegervalueshouldbereturned()
        {
            Assert.Equal(this.typeErrorDefaultValue, this.typeErrorDetails?.Value);
        }

        [Then(@"the reason should indicate an error and the error code should indicate a type mismatch with ""(.*)""")]
        public void Giventhereasonshouldindicateanerrorandtheerrorcodeshouldindicateatypemismatchwith(string errorCode)
        {
            Assert.Equal(Reason.Error.ToString(), this.typeErrorDetails?.Reason);
            Assert.Equal(errorCode, this.typeErrorDetails?.ErrorType.GetDescription());
        }

        private IDictionary<string, Flag> e2eFlagConfig = new Dictionary<string, Flag>(){
            {
                "boolean-flag", new Flag<bool>(
                    variants: new Dictionary<string, bool>(){
                        { "on", true },
                        { "off", false }
                    },
                    defaultVariant: "on"
                )
            },
            {
                "string-flag", new Flag<string>(
                    variants: new Dictionary<string, string>(){
                        { "greeting", "hi" },
                        { "parting", "bye" }
                    },
                    defaultVariant: "greeting"
                )
            },
            {
                "integer-flag", new Flag<int>(
                    variants: new Dictionary<string, int>(){
                        { "one", 1 },
                        { "ten", 10 }
                    },
                    defaultVariant: "ten"
                )
            },
            {
                "float-flag", new Flag<double>(
                    variants: new Dictionary<string, double>(){
                        { "tenth", 0.1 },
                        { "half", 0.5 }
                    },
                    defaultVariant: "half"
                )
            },
            {
                "object-flag", new Flag<Value>(
                    variants: new Dictionary<string, Value>(){
                        { "empty", new Value() },
                        { "template", new Value(Structure.Builder()
                                .Set("showImages", true)
                                .Set("title", "Check out these pics!")
                                .Set("imagesPerPage", 100).Build()
                            )
                        }
                    },
                    defaultVariant: "template"
                )
            },
            {
                "context-aware", new Flag<string>(
                    variants: new Dictionary<string, string>(){
                        { "internal", "INTERNAL" },
                        { "external", "EXTERNAL" }
                    },
                    defaultVariant: "external",
                    (context) => {
                        if (context.GetValue("fn").AsString == "Sulisław"
                            && context.GetValue("ln").AsString == "Świętopełk"
                            && context.GetValue("age").AsInteger == 29
                            && context.GetValue("customer").AsBoolean == false)
                        {
                            return "internal";
                        }
                        else return "external";
                    }
                )
            },
            {
                "wrong-flag", new Flag<string>(
                    variants: new Dictionary<string, string>(){
                        { "one", "uno" },
                        { "two", "dos" }
                    },
                    defaultVariant: "one"
                )
            }
        };
    }
}
