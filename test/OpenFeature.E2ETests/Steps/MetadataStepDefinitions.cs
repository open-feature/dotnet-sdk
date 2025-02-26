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
    private FlagEvaluationDetails<Value> _objResult = null!;

    private string _flagKey = null!;
    private object _defaultValue = null!;

    private FeatureClient? _client;
    private static readonly IDictionary<string, Flag> E2EFlagConfig = new Dictionary<string, Flag>
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
        },
        {
            "boolean-flag", new Flag<bool>(
                variants: new Dictionary<string, bool> { { "on", true }, { "off", false } },
                defaultVariant: "on"
            )
        },
        {
            "integer-flag", new Flag<int>(
                variants: new Dictionary<string, int> { { "23", 23 }, { "42", 42 } },
                defaultVariant: "23"
            )
        },
        {
            "float-flag", new Flag<double>(
                variants: new Dictionary<string, double> { { "2.3", 2.3 }, { "4.2", 4.2 } },
                defaultVariant: "2.3"
            )
        },
        {
            "string-flag", new Flag<string>(
                variants: new Dictionary<string, string> { { "value", "value" }, { "value2", "value2" } },
                defaultVariant: "value"
            )
        }
    };

    [Given("a stable provider")]
    public void GivenAStableProvider()
    {
        var memProvider = new InMemoryProvider(E2EFlagConfig);
        Api.Instance.SetProviderAsync(memProvider).Wait();
        this._client = Api.Instance.GetClient("TestClient", "1.0.0");
    }

    [Then("the resolved metadata should contain")]
    [Scope(Scenario = "Returns metadata")]
    public void ThenTheResolvedMetadataShouldContain(DataTable itemsTable)
    {
        var items = itemsTable.Rows.ToDictionary(row => row["key"], row => (row["value"], row["metadata_type"]));
        var metadata = this._objResult.FlagMetadata;

#if NET8_0_OR_GREATER
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
#else
        foreach (var item in items)
        {
            var key = item.Key;
            var value = item.Value.value;
            var metadataType = item.Value.metadata_type;

            string actual = null;
            switch (metadataType)
            {
                case "Boolean":
                    actual = metadata!.GetBool(key).ToString();
                    break;
                case "Integer":
                    actual = metadata!.GetInt(key).ToString();
                    break;
                case "Float":
                    actual = metadata!.GetDouble(key).ToString();
                    break;
                case "String":
                    actual = metadata!.GetString(key);
                    break;
            }

            Assert.Equal(value.ToLowerInvariant(), actual?.ToLowerInvariant());
        }
#endif
    }

    [Given(@"a Boolean-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenABoolean_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        this._flagKey = key;
        this._defaultValue = defaultType;
    }

    [Given(@"a Float-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenAFloat_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        this._flagKey = key;
        this._defaultValue = defaultType;
    }

    [Given(@"a Integer-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenAnInteger_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        this._flagKey = key;
        this._defaultValue = defaultType;
    }

    [Given(@"a String-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenAString_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        this._flagKey = key;
        this._defaultValue = defaultType;
    }

    [When(@"the flag was evaluated with details")]
    public async Task WhenTheFlagWasEvaluatedWithDetails_NoMetadata()
    {
        var defaultValue = new Value(this._defaultValue);
        this._objResult = await this._client!.GetObjectDetailsAsync(this._flagKey, defaultValue).ConfigureAwait(false);
    }

    [Then("the resolved metadata is empty")]
    public void ThenTheResolvedMetadataIsEmpty()
    {
        Assert.Null(this._objResult.FlagMetadata?.Count);
    }
}
