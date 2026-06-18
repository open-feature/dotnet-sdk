using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.Models;
using OpenFeature.Providers.MultiProvider.Strategies;
using OpenFeature.Providers.MultiProvider.Strategies.Models;
using OpenFeature.Providers.MultiProvider.Tests.Utils;

namespace OpenFeature.Providers.MultiProvider.Tests;

public class MultiProviderTrackingTests
{
    private const string TestTrackingEventName = "test-event";
    private const string Provider1Name = "provider1";
    private const string Provider2Name = "provider2";
    private const string Provider3Name = "provider3";

    private readonly TestProvider _testProvider1 = new(Provider1Name);
    private readonly TestProvider _testProvider2 = new(Provider2Name);
    private readonly TestProvider _testProvider3 = new(Provider3Name);
    private readonly EvaluationContext _evaluationContext = EvaluationContext.Builder().Build();

    [Fact]
    public async Task Track_WithMultipleReadyProviders_CallsTrackOnAllReadyProviders()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._testProvider1, Provider1Name),
            new(this._testProvider2, Provider2Name),
            new(this._testProvider3, Provider3Name)
        };

        var multiProvider = new MultiProvider(providerEntries, new FirstMatchStrategy());
        await multiProvider.InitializeAsync(this._evaluationContext, TestContext.Current.CancellationToken);

        var trackingDetails = TrackingEventDetails.Builder().SetValue(99.99).Build();

        // Act
        multiProvider.Track(TestTrackingEventName, this._evaluationContext, trackingDetails);

        // Assert
        var provider1Invocations = this._testProvider1.GetTrackingInvocations();
        var provider2Invocations = this._testProvider2.GetTrackingInvocations();
        var provider3Invocations = this._testProvider3.GetTrackingInvocations();

        Assert.Single(provider1Invocations);
        Assert.Single(provider2Invocations);
        Assert.Single(provider3Invocations);

        Assert.Equal(TestTrackingEventName, provider1Invocations[0].EventName);
        Assert.Equal(TestTrackingEventName, provider2Invocations[0].EventName);
        Assert.Equal(TestTrackingEventName, provider3Invocations[0].EventName);

        Assert.Equal(trackingDetails.Value, provider1Invocations[0].TrackingEventDetails?.Value);
        Assert.Equal(trackingDetails.Value, provider2Invocations[0].TrackingEventDetails?.Value);
        Assert.Equal(trackingDetails.Value, provider3Invocations[0].TrackingEventDetails?.Value);
    }

    [Fact]
    public async Task Track_WithNullEvaluationContext_CallsTrackWithNullContext()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._testProvider1, Provider1Name),
            new(this._testProvider2, Provider2Name)
        };

        var multiProvider = new MultiProvider(providerEntries, new FirstMatchStrategy());
        await multiProvider.InitializeAsync(this._evaluationContext, TestContext.Current.CancellationToken);

        // Act
        multiProvider.Track(TestTrackingEventName);

        // Assert
        var provider1Invocations = this._testProvider1.GetTrackingInvocations();
        var provider2Invocations = this._testProvider2.GetTrackingInvocations();

        Assert.Single(provider1Invocations);
        Assert.Single(provider2Invocations);

        Assert.Equal(TestTrackingEventName, provider1Invocations[0].EventName);
        Assert.Equal(TestTrackingEventName, provider2Invocations[0].EventName);
    }

    [Fact]
    public async Task Track_WithNullTrackingDetails_CallsTrackWithNullDetails()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._testProvider1, Provider1Name),
            new(this._testProvider2, Provider2Name)
        };

        var multiProvider = new MultiProvider(providerEntries, new FirstMatchStrategy());
        await multiProvider.InitializeAsync(this._evaluationContext, TestContext.Current.CancellationToken);

        // Act
        multiProvider.Track(TestTrackingEventName, this._evaluationContext);

        // Assert
        var provider1Invocations = this._testProvider1.GetTrackingInvocations();
        var provider2Invocations = this._testProvider2.GetTrackingInvocations();

        Assert.Single(provider1Invocations);
        Assert.Single(provider2Invocations);

        Assert.Equal(TestTrackingEventName, provider1Invocations[0].EventName);
        Assert.Null(provider1Invocations[0].TrackingEventDetails);

        Assert.Equal(TestTrackingEventName, provider2Invocations[0].EventName);
        Assert.Null(provider2Invocations[0].TrackingEventDetails);
    }

    [Fact]
    public async Task Track_WhenProviderThrowsException_ContinuesWithOtherProviders()
    {
        // Arrange
        var throwingProvider = Substitute.For<FeatureProvider>();
        throwingProvider.GetMetadata().Returns(new Metadata(Provider2Name));
        throwingProvider.When(x => x.Track(Arg.Any<string>(), Arg.Any<EvaluationContext>(), Arg.Any<TrackingEventDetails>()))
            .Do(_ => throw new InvalidOperationException("Test exception"));

        var providerEntries = new List<ProviderEntry>
        {
            new(this._testProvider1, Provider1Name),
            new(throwingProvider, Provider2Name),
            new(this._testProvider3, Provider3Name)
        };

        var multiProvider = new MultiProvider(providerEntries, new FirstMatchStrategy());
        await multiProvider.InitializeAsync(this._evaluationContext, TestContext.Current.CancellationToken);

        // Manually set all providers to Ready status
        throwingProvider.Status.Returns(ProviderStatus.Ready);

        var trackingDetails = TrackingEventDetails.Builder().SetValue(99.99).Build();

        // Act
        multiProvider.Track(TestTrackingEventName, this._evaluationContext, trackingDetails);

        // Assert - should not throw and should continue with other providers
        var provider1Invocations = this._testProvider1.GetTrackingInvocations();
        var provider3Invocations = this._testProvider3.GetTrackingInvocations();

        Assert.Single(provider1Invocations);
        Assert.Single(provider3Invocations);

        throwingProvider.Received(1).Track(TestTrackingEventName, Arg.Any<EvaluationContext>(), trackingDetails);
    }

    [Fact]
    public async Task Track_WhenDisposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._testProvider1, Provider1Name)
        };

        var multiProvider = new MultiProvider(providerEntries, new FirstMatchStrategy());
        await multiProvider.InitializeAsync(this._evaluationContext, TestContext.Current.CancellationToken);
        await multiProvider.DisposeAsync();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => multiProvider.Track(TestTrackingEventName, this._evaluationContext));
    }

    [Fact]
    public async Task Track_WithCustomStrategy_RespectsStrategyDecision()
    {
        // Arrange
        var customStrategy = Substitute.For<BaseEvaluationStrategy>();
        customStrategy.RunMode.Returns(RunMode.Sequential);

        // Only allow tracking with the first provider
        customStrategy.ShouldTrackWithThisProvider(
            Arg.Is<StrategyPerProviderContext<object>>(ctx => ctx.ProviderName == Provider1Name),
            Arg.Any<EvaluationContext>(),
            Arg.Any<string>(),
            Arg.Any<TrackingEventDetails>()
        ).Returns(true);

        customStrategy.ShouldTrackWithThisProvider(
            Arg.Is<StrategyPerProviderContext<object>>(ctx => ctx.ProviderName != Provider1Name),
            Arg.Any<EvaluationContext>(),
            Arg.Any<string>(),
            Arg.Any<TrackingEventDetails>()
        ).Returns(false);

        var providerEntries = new List<ProviderEntry>
        {
            new(this._testProvider1, Provider1Name),
            new(this._testProvider2, Provider2Name),
            new(this._testProvider3, Provider3Name)
        };

        var multiProvider = new MultiProvider(providerEntries, customStrategy);
        await multiProvider.InitializeAsync(this._evaluationContext, TestContext.Current.CancellationToken);

        var trackingDetails = TrackingEventDetails.Builder().SetValue(99.99).Build();

        // Act
        multiProvider.Track(TestTrackingEventName, this._evaluationContext, trackingDetails);

        // Assert - only provider1 should receive the tracking call
        var provider1Invocations = this._testProvider1.GetTrackingInvocations();
        var provider2Invocations = this._testProvider2.GetTrackingInvocations();
        var provider3Invocations = this._testProvider3.GetTrackingInvocations();

        Assert.Single(provider1Invocations);
        Assert.Empty(provider2Invocations);
        Assert.Empty(provider3Invocations);

        customStrategy.Received(3).ShouldTrackWithThisProvider(
            Arg.Any<StrategyPerProviderContext<object>>(),
            Arg.Any<EvaluationContext>(),
            TestTrackingEventName,
            trackingDetails
        );
    }

    [Fact]
    public async Task Track_WithComplexTrackingDetails_PropagatesAllDetails()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._testProvider1, Provider1Name),
            new(this._testProvider2, Provider2Name)
        };

        var multiProvider = new MultiProvider(providerEntries, new FirstMatchStrategy());
        await multiProvider.InitializeAsync(this._evaluationContext, TestContext.Current.CancellationToken);

        var trackingDetails = TrackingEventDetails.Builder()
            .SetValue(199.99)
            .Set("currency", new Value("USD"))
            .Set("productId", new Value("prod-123"))
            .Set("quantity", new Value(5))
            .Build();

        // Act
        multiProvider.Track(TestTrackingEventName, this._evaluationContext, trackingDetails);

        // Assert
        var provider1Invocations = this._testProvider1.GetTrackingInvocations();
        var provider2Invocations = this._testProvider2.GetTrackingInvocations();

        Assert.Single(provider1Invocations);
        Assert.Single(provider2Invocations);

        var details1 = provider1Invocations[0].TrackingEventDetails;
        var details2 = provider2Invocations[0].TrackingEventDetails;

        Assert.NotNull(details1);
        Assert.NotNull(details2);

        Assert.Equal(199.99, details1.Value);
        Assert.Equal(199.99, details2.Value);

        Assert.Equal("USD", details1.GetValue("currency").AsString);
        Assert.Equal("USD", details2.GetValue("currency").AsString);

        Assert.Equal("prod-123", details1.GetValue("productId").AsString);
        Assert.Equal("prod-123", details2.GetValue("productId").AsString);

        Assert.Equal(5, details1.GetValue("quantity").AsInteger);
        Assert.Equal(5, details2.GetValue("quantity").AsInteger);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Track_WhenInvalidTrackingEventName_DoesNotCallProviders(string? trackingEventName)
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._testProvider1, Provider1Name),
            new(this._testProvider2, Provider2Name)
        };

        var multiProvider = new MultiProvider(providerEntries, new FirstMatchStrategy());
        await multiProvider.InitializeAsync(this._evaluationContext, TestContext.Current.CancellationToken);

        // Act & Assert
        multiProvider.Track(trackingEventName!, this._evaluationContext, TrackingEventDetails.Empty);

        var provider1Invocations = this._testProvider1.GetTrackingInvocations();
        var provider2Invocations = this._testProvider2.GetTrackingInvocations();

        Assert.Empty(provider1Invocations);
        Assert.Empty(provider2Invocations);
    }
}
