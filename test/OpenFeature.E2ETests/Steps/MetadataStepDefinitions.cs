using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenFeature.Model;
using OpenFeature.Providers.Memory;
using Reqnroll;
using Xunit;

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
                defaultVariant: "on",
                flagMetadata: new ImmutableMetadata(new Dictionary<string, object>
                {
                    { "string", "1.0.2" }, { "integer", 2 }, { "float", 0.1 }, { "boolean", true }
                })
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

    [When("the flag was evaluated with details")]
    [Scope(Scenario = "Returns metadata")]
    public async Task WhenTheFlagWasEvaluatedWithDetails()
    {
        this._boolResult = await _client!.GetBooleanDetailsAsync("metadata-flag", true).ConfigureAwait(false);
    }

    [Then("the resolved metadata should contain")]
    [Scope(Scenario = "Returns metadata")]
    public void ThenTheResolvedMetadataShouldContain(DataTable itemsTable)
    {
        var items = itemsTable.Rows.ToDictionary(row => row["key"], row => (row["value"], row["metadata_type"]));
        var metadata = this._boolResult.FlagMetadata;

        foreach (var (key, (value, metadataType)) in items)
        {
            var actual = metadataType switch
            {
                "Boolean" => metadata!.GetBool(key).ToString(),
                "Integer" => metadata!.GetInt(key).ToString(),
                "Float" => metadata!.GetDouble(key).ToString(),
                "String" => metadata!.GetString(key),
                _ => null
            };

            Assert.Equal(value.ToLowerInvariant(), actual?.ToLowerInvariant());
        }
    }

    [Given(@"a Boolean-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenABoolean_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        // This is a no-op, as the flag is already defined in the provider
    }

    [Given(@"a Float-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenAFloat_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        // This is a no-op, as the flag is already defined in the provider
    }

    [Given(@"a Integer-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenAnInteger_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        // This is a no-op, as the flag is already defined in the provider
    }

    [Given(@"a String-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenAString_FlagWithKeyAndADefaultValue(string key, string defaultType)
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
        Assert.Null(this._objResult.FlagMetadata?.Count);
    }
}
