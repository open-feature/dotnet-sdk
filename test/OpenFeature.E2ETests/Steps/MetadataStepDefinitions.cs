using System.Collections.Generic;
using System.Threading.Tasks;
using OpenFeature.Model;
using OpenFeature.Providers.Memory;
using Reqnroll;

namespace OpenFeature.E2ETests.Steps;

[Binding]
[Scope(Feature = "Metadata")]
public class MetadataStepDefinitions
{
    private FlagEvaluationDetails<bool> _boolResult = null!;
    private FlagEvaluationDetails<Value> _objResult = null!;

    private static FeatureClient? _client;
    private static IDictionary<string, Flag> E2EFlagConfig = new Dictionary<string, Flag>
    {
        {
            "metadata-flag", new Flag<bool>(
                variants: new Dictionary<string, bool> { { "on", true }, { "off", false } },
                defaultVariant: "on"
            )
        }
    };

    [Given("a stable provider")]
    public void GivenAStableProvider()
    {
        var memProvider = new InMemoryProvider(E2EFlagConfig);
        Api.Instance.SetProviderAsync(memProvider).Wait();
        _client = Api.Instance.GetClient("TestClient", "1.0.0");
    }

    [Given("a Boolean-flag with key \"metadata-flag\" and a default value \"true\"")]
    public void GivenABooleanFlagWithKeyMetadataFlagAndADefaultValueTrue()
    {
        // This is a no-op, as the flag is already defined in the provider
    }

    [When("the flag was evaluated with details")]
    [Scope(Scenario = "Returns metadata")]
    public async Task WhenTheFlagWasEvaluatedWithDetails()
    {
        this._boolResult = await _client!.GetBooleanDetailsAsync("metadata-flag", true).ConfigureAwait(false);
    }

    [Then("the resolved metadata should contain")]
    public void ThenTheResolvedMetadataShouldContain(DataTable itemsTable)
    {
        ScenarioContext.StepIsPending();
    }

    [Given(@"a ""(.*)"" with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenAFlagWithKeyAndADefaultValue(string key, string flag_Type, object default_Value)
    {
        // This is a no-op, as the flag is already defined in the provider
    }

    [When(@"the flag was evaluated with details")]
    public void WhenTheFlagWasEvaluatedWithDetails_NoMetadata(string key, string flag_Type, object default_Value)
    {
        var defaultValue = new Value(default_Value);
        this._objResult = _client!.GetObjectDetailsAsync(key, defaultValue).Result;
    }

    [Then("the resolved metadata is empty")]
    public void ThenTheResolvedMetadataIsEmpty()
    {
        ScenarioContext.StepIsPending();
    }
}
