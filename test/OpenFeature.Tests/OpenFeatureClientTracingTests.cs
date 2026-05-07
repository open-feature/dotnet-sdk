using System.Diagnostics;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenFeature.Constant;
using OpenFeature.Error;
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
            ["bool-flag"] = new Flag<bool>(new Dictionary<string, bool> { { "on", true } }, "on"),
            ["string-flag"] = new Flag<string>(new Dictionary<string, string> { { "on", "hello" } }, "on"),
            ["int-flag"] = new Flag<int>(new Dictionary<string, int> { { "on", 42 } }, "on"),
            ["double-flag"] = new Flag<double>(new Dictionary<string, double> { { "on", 3.14 } }, "on"),
            ["object-flag"] = new Flag<Value>(new Dictionary<string, Value> { { "on", new Value(Structure.Builder().Set("value1", true).Build()) } }, "on")
        };
        var provider = new InMemoryProvider(flags);

        return this._api.SetProviderAsync(provider);
    }

    public static IEnumerable<object[]> ResolveValue()
    {
        yield return new object[]
        {
            new Func<FeatureClient, Task<FlagEvaluationDetails<bool>>>((r) => r.GetBooleanDetailsAsync("bool-flag", false))
        };
        yield return new object[]
        {
            new Func<FeatureClient, Task<FlagEvaluationDetails<string>>>((r) => r.GetStringDetailsAsync("string-flag", "def"))
        };
        yield return new object[]
        {
            new Func<FeatureClient, Task<FlagEvaluationDetails<int>>>((r) => r.GetIntegerDetailsAsync("int-flag", 3))
        };
        yield return new object[]
        {
            new Func<FeatureClient, Task<FlagEvaluationDetails<double>>>((r) => r.GetDoubleDetailsAsync("double-flag", 3.5))
        };
        yield return new object[]
        {
            new Func<FeatureClient, Task<FlagEvaluationDetails<Value>>>((r) => r.GetObjectDetailsAsync("object-flag", new Value(Structure.Builder().Set("value1", true).Build())))
        };
    }

    [Theory]
    [MemberData(nameof(ResolveValue))]
    public async Task GetValueAsync_ShouldCreateSpan<T>(Func<FeatureClient, Task<FlagEvaluationDetails<T>>> act)
    {
        // Arrange
        var client = this._api.GetClient("TestClient");

        // Act
        var result = await act(client);

        // Assert
        Assert.Single(this._exportedActivities);

        var trace = this._exportedActivities[0];
        var tags = trace.TagObjects.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        Assert.Contains("feature_flag.key", tags);
        Assert.Equal(result.FlagKey, tags["feature_flag.key"]);

        Assert.Contains("feature_flag.provider.name", tags);
        Assert.Equal("InMemory", tags["feature_flag.provider.name"]);

        Assert.Contains("feature_flag.result.reason", tags);
        Assert.Equal("static", tags["feature_flag.result.reason"]);

        Assert.Contains("feature_flag.result.value", tags);
        Assert.Equal(result.Value, tags["feature_flag.result.value"]);

        Assert.Contains("feature_flag.result.variant", tags);
        Assert.Equal("on", tags["feature_flag.result.variant"]);
    }

    [Theory]
    [MemberData(nameof(ResolveValue))]
    public async Task GetValueAsync_WhenProviderErrors_ShouldCreateSpanWithErrorTags<T>(Func<FeatureClient, Task<FlagEvaluationDetails<T>>> act)
    {
        // Arrange
        var mockProvider = Substitute.For<FeatureProvider>();
        mockProvider.GetMetadata().Returns(new Metadata("TestProvider"));
        mockProvider.ResolveBooleanValueAsync("bool-flag", Arg.Any<bool>(), Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ResolutionDetails<bool>("bool-flag", true, ErrorType.ProviderFatal, errorMessage: "Error!")));
        mockProvider.ResolveStringValueAsync("string-flag", Arg.Any<string>(), Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ResolutionDetails<string>("string-flag", "world", ErrorType.ProviderFatal, errorMessage: "Error!")));
        mockProvider.ResolveIntegerValueAsync("int-flag", Arg.Any<int>(), Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ResolutionDetails<int>("int-flag", 42, ErrorType.ProviderFatal, errorMessage: "Error!")));
        mockProvider.ResolveDoubleValueAsync("double-flag", Arg.Any<double>(), Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ResolutionDetails<double>("double-flag", 1.0f, ErrorType.ProviderFatal, errorMessage: "Error!")));
        mockProvider.ResolveStructureValueAsync("object-flag", Arg.Any<Value>(), Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ResolutionDetails<Value>("object-flag", new Value(Structure.Builder().Build()), ErrorType.ProviderFatal, errorMessage: "Error!")));

        await this._api.SetProviderAsync("domain", mockProvider);

        var client = this._api.GetClient("domain");

        // Act
        await act(client);

        // Assert
        Assert.Single(this._exportedActivities);

        var trace = this._exportedActivities[0];
        var tags = trace.TagObjects.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        Assert.Contains("error.type", tags);
        Assert.Equal("provider_fatal", tags["error.type"]);

        Assert.Contains("feature_flag.error.message", tags);
        Assert.Equal("Error!", tags["feature_flag.error.message"]);
    }

    [Theory]
    [MemberData(nameof(ResolveValue))]
    public async Task GetValueAsync_WhenProviderThrowsException_ShouldCreateSpanWithErrorTags<T>(Func<FeatureClient, Task<FlagEvaluationDetails<T>>> act)
    {
        // Arrange
        var mockProvider = Substitute.For<FeatureProvider>();
        mockProvider.GetMetadata().Returns(new Metadata("TestProvider"));
        mockProvider.ResolveBooleanValueAsync("bool-flag", Arg.Any<bool>(), Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new FeatureProviderException(ErrorType.TargetingKeyMissing, "Error!"));
        mockProvider.ResolveStringValueAsync("string-flag", Arg.Any<string>(), Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new FeatureProviderException(ErrorType.TargetingKeyMissing, "Error!"));
        mockProvider.ResolveIntegerValueAsync("int-flag", Arg.Any<int>(), Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new FeatureProviderException(ErrorType.TargetingKeyMissing, "Error!"));
        mockProvider.ResolveDoubleValueAsync("double-flag", Arg.Any<double>(), Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new FeatureProviderException(ErrorType.TargetingKeyMissing, "Error!"));
        mockProvider.ResolveStructureValueAsync("object-flag", Arg.Any<Value>(), Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new FeatureProviderException(ErrorType.TargetingKeyMissing, "Error!"));

        await this._api.SetProviderAsync("domain", mockProvider);

        var client = this._api.GetClient("domain");

        // Act
        await act(client);

        // Assert
        Assert.Single(this._exportedActivities);

        var trace = this._exportedActivities[0];
        var tags = trace.TagObjects.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        Assert.Contains("error.type", tags);
        Assert.Equal("targeting_key_missing", tags["error.type"]);

        Assert.Contains("feature_flag.error.message", tags);
        Assert.Equal("Error!", tags["feature_flag.error.message"]);
    }

    [Theory]
    [MemberData(nameof(ResolveValue))]
    public async Task GetValueAsync_WhenProviderThrowsGeneralException_ShouldCreateSpanWithErrorTags<T>(Func<FeatureClient, Task<FlagEvaluationDetails<T>>> act)
    {
        // Arrange
        var mockProvider = Substitute.For<FeatureProvider>();
        mockProvider.GetMetadata().Returns(new Metadata("TestProvider"));
        mockProvider.ResolveBooleanValueAsync("bool-flag", Arg.Any<bool>(), Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Error!"));
        mockProvider.ResolveStringValueAsync("string-flag", Arg.Any<string>(), Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Error!"));
        mockProvider.ResolveIntegerValueAsync("int-flag", Arg.Any<int>(), Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Error!"));
        mockProvider.ResolveDoubleValueAsync("double-flag", Arg.Any<double>(), Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Error!"));
        mockProvider.ResolveStructureValueAsync("object-flag", Arg.Any<Value>(), Arg.Any<EvaluationContext>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Error!"));

        await this._api.SetProviderAsync("domain", mockProvider);

        var client = this._api.GetClient("domain");

        // Act
        await act(client);

        // Assert
        Assert.Single(this._exportedActivities);

        var trace = this._exportedActivities[0];
        var tags = trace.TagObjects.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        Assert.Contains("error.type", tags);
        Assert.Equal("general", tags["error.type"]);

        Assert.Contains("feature_flag.error.message", tags);
        Assert.Equal("Error!", tags["feature_flag.error.message"]);
    }

    public Task DisposeAsync()
    {
        this._activityListener.Dispose();

        return this._api.ShutdownAsync();
    }
}
