using System;
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
    private object _objResult = null!;

    private string _flagKey = null!;
    private string _defaultValue = null!;
    private FlagType? _flagType;

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
        var items = itemsTable.Rows.Select(row => new DataTableRows(row["key"], row["value"], row["metadata_type"])).ToList();
        var metadata = (this._objResult as FlagEvaluationDetails<bool>)?.FlagMetadata;

        foreach (var item in items)
        {
            var key = item.Key;
            var value = item.Value;
            var metadataType = item.MetadataType;

            string? actual = null!;
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
    }

    [Given(@"a Boolean-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenABoolean_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        this._flagKey = key;
        this._defaultValue = defaultType;
        this._flagType = FlagType.Boolean;
    }

    [Given(@"a Float-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenAFloat_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        this._flagKey = key;
        this._defaultValue = defaultType;
        this._flagType = FlagType.Float;
    }

    [Given(@"a Integer-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenAnInteger_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        this._flagKey = key;
        this._defaultValue = defaultType;
        this._flagType = FlagType.Integer;
    }

    [Given(@"a String-flag with key ""(.*)"" and a default value ""(.*)""")]
    public void GivenAString_FlagWithKeyAndADefaultValue(string key, string defaultType)
    {
        this._flagKey = key;
        this._defaultValue = defaultType;
        this._flagType = FlagType.String;
    }

    [When(@"the flag was evaluated with details")]
    public async Task WhenTheFlagWasEvaluatedWithDetails()
    {
        switch (this._flagType)
        {
            case FlagType.Boolean:
                this._objResult = await this._client!.GetBooleanDetailsAsync(this._flagKey, bool.Parse(this._defaultValue)).ConfigureAwait(false);
                break;
            case FlagType.Float:
                this._objResult = await this._client!.GetDoubleDetailsAsync(this._flagKey, double.Parse(this._defaultValue)).ConfigureAwait(false);
                break;
            case FlagType.Integer:
                this._objResult = await this._client!.GetIntegerDetailsAsync(this._flagKey, int.Parse(this._defaultValue)).ConfigureAwait(false);
                break;
            case FlagType.String:
                this._objResult = await this._client!.GetStringDetailsAsync(this._flagKey, this._defaultValue).ConfigureAwait(false);
                break;
        }
    }

    [Then("the resolved metadata is empty")]
    public void ThenTheResolvedMetadataIsEmpty()
    {
        switch (this._flagType)
        {
            case FlagType.Boolean:
                Assert.Null((this._objResult as FlagEvaluationDetails<bool>)?.FlagMetadata?.Count);
                break;
            case FlagType.Float:
                Assert.Null((this._objResult as FlagEvaluationDetails<double>)?.FlagMetadata?.Count);
                break;
            case FlagType.Integer:
                Assert.Null((this._objResult as FlagEvaluationDetails<int>)?.FlagMetadata?.Count);
                break;
            case FlagType.String:
                Assert.Null((this._objResult as FlagEvaluationDetails<string>)?.FlagMetadata?.Count);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private enum FlagType
    {
        Integer,
        Float,
        String,
        Boolean
    }

    private class DataTableRows
    {
        public DataTableRows(string key, string value, string metadataType)
        {
            this.Key = key;
            this.Value = value;
            this.MetadataType = metadataType;
        }

        public string Key { get; }
        public string Value { get; }
        public string MetadataType { get; }
    }
}
