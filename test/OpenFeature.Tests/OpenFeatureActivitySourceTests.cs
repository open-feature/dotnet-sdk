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
        Assert.Equal(ActivityKind.Internal, activity.Kind);
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

    [Fact]
    public void SetTagIfRequested_AddsTag()
    {
        var exportedActivities = new List<Activity>();
        using var activityListener = new ActivityListener()
        {
            ShouldListenTo = source => source.Name == "OpenFeature",
            Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => exportedActivities.Add(activity)
        };

        ActivitySource.AddActivityListener(activityListener);

        using (var activity = OpenFeatureActivitySource.StartActivity("set_tag_if_requested"))
        {
            activity?.AddTagIfRequested("custom_tag_name", true);
        }

        Assert.Single(exportedActivities);

        var actualActivity = exportedActivities.First();
        Assert.Equal("custom_tag_name", actualActivity.TagObjects.First().Key);
        Assert.Equal(true, actualActivity.TagObjects.First().Value);
    }

    [Fact]
    public void SetTagIfRequested_WhenDataNotRequested_DoesNotAddTag()
    {
        var exportedActivities = new List<Activity>();
        using var activityListener = new ActivityListener()
        {
            ShouldListenTo = source => source.Name == "OpenFeature",
            Sample = (ref _) => ActivitySamplingResult.PropagationData,
            ActivityStopped = activity => exportedActivities.Add(activity)
        };

        ActivitySource.AddActivityListener(activityListener);

        using (var activity = OpenFeatureActivitySource.StartActivity("set_tag_if_requested"))
        {
            activity?.AddTagIfRequested("custom_tag_name", true);
        }

        Assert.Single(exportedActivities);

        var actualActivity = exportedActivities.First();
        Assert.Empty(actualActivity.TagObjects);
    }

    [Fact]
    public void SetTagIfRequested_WhenActivityIsNull_DoesNothing()
    {
        var ex = Record.Exception(() => OpenFeatureActivitySource.AddTagIfRequested(null!, "custom_tag_name", true));
        Assert.Null(ex);
    }
}
