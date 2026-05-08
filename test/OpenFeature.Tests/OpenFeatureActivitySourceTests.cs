using System.Diagnostics;
using OpenFeature.Constant;

namespace OpenFeature.Tests;

public class OpenFeatureActivitySourceTests
{
    [Fact]
    public void StartActivity_ReturnsActivityWithCorrectName()
    {
        using var activityListener = new ActivityListener()
        {
            ShouldListenTo = source => true,
            Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded
        };

        ActivitySource.AddActivityListener(activityListener);

        var activity = OpenFeatureActivitySource.StartActivity("test_activity");

        Assert.NotNull(activity);
        Assert.Equal("test_activity", activity.OperationName);
        Assert.Equal("OpenFeature", activity.Source.Name);
        Assert.NotNull(activity.Source.Version);
        Assert.NotEmpty(activity.Source.Version);
    }

    [Theory]
    [InlineData(ErrorType.ProviderNotReady, "provider_not_ready")]
    [InlineData(ErrorType.FlagNotFound, "flag_not_found")]
    [InlineData(ErrorType.ParseError, "parse_error")]
    [InlineData(ErrorType.TypeMismatch, "type_mismatch")]
    [InlineData(ErrorType.General, "general")]
    [InlineData(ErrorType.InvalidContext, "invalid_context")]
    [InlineData(ErrorType.TargetingKeyMissing, "targeting_key_missing")]
    [InlineData(ErrorType.ProviderFatal, "provider_fatal")]
    [InlineData((ErrorType)999, "_OTHER")]
    public void GetFlagEvaluationErrorDescription_ReturnsCorrectDescription(ErrorType errorType, string expectedDescription)
    {
        var actual = errorType.GetFlagEvaluationErrorDescription();

        Assert.Equal(expectedDescription, actual);
    }

    [Theory]
    [InlineData("TARGETING_MATCH", "targeting_match")]
    [InlineData("SPLIT", "split")]
    [InlineData("DISABLED", "disabled")]
    [InlineData("DEFAULT", "default")]
    [InlineData("STATIC", "static")]
    [InlineData("CACHED", "cached")]
    [InlineData("UNKNOWN", "unknown")]
    [InlineData("ERROR", "error")]
    [InlineData("OTHER", "OTHER")]
    public void GetFlagEvaluationReasonDescription(string? reason, string expectedDescription)
    {
        var actual = OpenFeatureActivitySource.GetFlagEvaluationReasonDescription(reason);

        Assert.Equal(expectedDescription, actual);
    }
}
