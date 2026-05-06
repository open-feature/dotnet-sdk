using System.Diagnostics;
using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.Memory;

namespace OpenFeature.Tests;

public class OpenFeatureClientTracingTests : IAsyncLifetime
{
    private readonly Api _api;

    private readonly List<Activity> _exportedActivities;
    private readonly ActivityListener _activityListener;

    public OpenFeatureClientTracingTests()
    {
        this._api = Api.Instance;
        this._exportedActivities = [];
        this._activityListener = new ActivityListener()
        {
            ShouldListenTo = source => source.Name == "OpenFeature",
            Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => this._exportedActivities.Add(activity)
        };

        ActivitySource.AddActivityListener(this._activityListener);
    }

    public Task InitializeAsync()
    {
        var flags = new Dictionary<string, Flag>
        {
            ["bool-flag"] = new Flag<bool>(new Dictionary<string, bool> { { "on", true } }, "on")
        };
        var provider = new InMemoryProvider(flags);

        return this._api.SetProviderAsync(provider);
    }

    [Fact]
    public async Task GetBooleanValueAsync_ShouldCreateSpan()
    {
        // Arrange
        var client = this._api.GetClient("TestClient");

        // Act
        await client.GetBooleanValueAsync("bool-flag", false);

        // Assert
        Assert.Single(this._exportedActivities);

        var trace = this._exportedActivities[0];
        var tags = trace.TagObjects.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        Assert.Contains("feature_flag.key", tags);
        Assert.Equal("bool-flag", tags["feature_flag.key"]);

        Assert.Contains("feature_flag.provider.name", tags);
        Assert.Equal("InMemory", tags["feature_flag.provider.name"]);

        Assert.Contains("feature_flag.result.reason", tags);
        Assert.Equal("static", tags["feature_flag.result.reason"]);

        Assert.Contains("feature_flag.result.value", tags);
        Assert.Equal(true, tags["feature_flag.result.value"]);

        Assert.Contains("feature_flag.result.variant", tags);
        Assert.Equal("on", tags["feature_flag.result.variant"]);
    }

    [Fact]
    public async Task GetBooleanValueAsync_WhenProviderErrors_ShouldCreateSpanWithErrorTags()
    {
        // Arrange
        var mockProvider = Substitute.For<FeatureProvider>();
        mockProvider.GetMetadata().Returns(new Metadata("TestProvider"));
        mockProvider.ResolveBooleanValueAsync("error-flag", Arg.Any<bool>(), Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ResolutionDetails<bool>("error-flag", true, ErrorType.ProviderFatal, errorMessage: "Error!")));

        await this._api.SetProviderAsync("domain", mockProvider);

        var client = this._api.GetClient("domain");

        // Act
        await client.GetBooleanValueAsync("error-flag", false);

        // Assert
        Assert.Single(this._exportedActivities);

        var trace = this._exportedActivities[0];
        var tags = trace.TagObjects.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        Assert.Contains("error.type", tags);
        Assert.Equal("provider_fatal", tags["error.type"]);

        Assert.Contains("feature_flag.error.message", tags);
        Assert.Equal("Error!", tags["feature_flag.error.message"]);
    }

    public Task DisposeAsync()
    {
        this._activityListener.Dispose();

        return this._api.ShutdownAsync();
    }
}
