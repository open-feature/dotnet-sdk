using OpenFeature.E2ETests.Utils;
using OpenFeature.Model;

namespace OpenFeature.E2ETests.Steps;

[Binding]
public class MetadataStepDefinitions
{
    private readonly State _state;

    public MetadataStepDefinitions(State _state)
    {
        this._state = _state;
    }

    [Then("the resolved metadata should contain")]
    public void ThenTheResolvedMetadataShouldContain(DataTable dataTable)
    {
        switch (this._state.Flag!.Type)
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
                AssertOnDetails<Value>(r => AssertMetadataContains(dataTable, r));
                break;
            default:
                Assert.Fail("FlagType not yet supported.");
                break;
        }
    }

    [Then("the resolved metadata is empty")]
    public void ThenTheResolvedMetadataIsEmpty()
    {
        var flag = this._state.Flag!;
        switch (flag.Type)
        {
            case FlagType.Boolean:
                AssertOnDetails<bool>(d => Assert.Null(d.FlagMetadata?.Count));
                break;
            case FlagType.Float:
                AssertOnDetails<double>(d => Assert.Null(d.FlagMetadata?.Count));
                break;
            case FlagType.Integer:
                AssertOnDetails<int>(d => Assert.Null(d.FlagMetadata?.Count));
                break;
            case FlagType.String:
                AssertOnDetails<string>(d => Assert.Null(d.FlagMetadata?.Count));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void AssertOnDetails<T>(Action<FlagEvaluationDetails<T>> assertion)
    {
        var details = this._state.FlagEvaluationDetailsResult as FlagEvaluationDetails<T>;

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
