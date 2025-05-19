using OpenFeature.E2ETests.Utils;
using OpenFeature.Model;
using Reqnroll;
using Xunit;

namespace OpenFeature.E2ETests.Steps;

[Binding]
[Scope(Feature = "Metadata")]
public class MetadataStepDefinitions : BaseStepDefinitions
{
    MetadataStepDefinitions(State state) : base(state)
    {
    }

    [Then("the resolved metadata should contain")]
    [Scope(Scenario = "Returns metadata")]
    public void ThenTheResolvedMetadataShouldContain(DataTable itemsTable)
    {
        var items = itemsTable.Rows.Select(row => new DataTableRows(row["key"], row["value"], row["metadata_type"])).ToList();
        var metadata = (this.State.FlagEvaluationDetailsResult as FlagEvaluationDetails<bool>)?.FlagMetadata;

        foreach (var item in items)
        {
            var key = item.Key;
            var value = item.Value;
            var metadataType = item.MetadataType;

            string? actual = null!;
            switch (metadataType)
            {
                case FlagType.Boolean:
                    actual = metadata!.GetBool(key).ToString();
                    break;
                case FlagType.Integer:
                    actual = metadata!.GetInt(key).ToString();
                    break;
                case FlagType.Float:
                    actual = metadata!.GetDouble(key).ToString();
                    break;
                case FlagType.String:
                    actual = metadata!.GetString(key);
                    break;
            }

            Assert.Equal(value.ToLowerInvariant(), actual?.ToLowerInvariant());
        }
    }

    [Then("the resolved metadata is empty")]
    public void ThenTheResolvedMetadataIsEmpty()
    {
        var flag = this.State.Flag!;
        switch (flag.Type)
        {
            case FlagType.Boolean:
                Assert.Null((this.State.FlagEvaluationDetailsResult as FlagEvaluationDetails<bool>)?.FlagMetadata?.Count);
                break;
            case FlagType.Float:
                Assert.Null((this.State.FlagEvaluationDetailsResult as FlagEvaluationDetails<double>)?.FlagMetadata?.Count);
                break;
            case FlagType.Integer:
                Assert.Null((this.State.FlagEvaluationDetailsResult as FlagEvaluationDetails<int>)?.FlagMetadata?.Count);
                break;
            case FlagType.String:
                Assert.Null((this.State.FlagEvaluationDetailsResult as FlagEvaluationDetails<string>)?.FlagMetadata?.Count);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
