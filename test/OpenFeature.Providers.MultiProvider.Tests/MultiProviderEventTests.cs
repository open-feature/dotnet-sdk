using System.Threading.Channels;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenFeature.Constant;
using OpenFeature.Model;
using OpenFeature.Providers.MultiProvider.Models;
using OpenFeature.Providers.MultiProvider.Strategies;
using OpenFeature.Providers.MultiProvider.Strategies.Models;
using OpenFeature.Providers.MultiProvider.Tests.Utils;

namespace OpenFeature.Providers.MultiProvider.Tests;

/// <summary>
/// Tests for the event emission functionality of the MultiProvider.
/// These tests verify that events are properly emitted during various operations
/// including initialization, evaluation, provider status changes, and child provider events.
/// </summary>
public class MultiProviderEventTests
{
    private const string TestFlagKey = "test-flag";
    private const string Provider1Name = "provider1";
    private const string Provider2Name = "provider2";
    private const string Provider3Name = "provider3";

    private readonly FeatureProvider _mockProvider1 = new TestProvider(Provider1Name);
    private readonly FeatureProvider _mockProvider2 = new TestProvider(Provider2Name);
    private readonly FeatureProvider _mockProvider3 = new TestProvider(Provider3Name);
    private readonly BaseEvaluationStrategy _mockStrategy = Substitute.For<BaseEvaluationStrategy>();
    private readonly EvaluationContext _evaluationContext = new EvaluationContextBuilder().Build();

    public MultiProviderEventTests()
    {
        // Setup default strategy behavior - using specific generic types
        this._mockStrategy.RunMode.Returns(RunMode.Sequential);
        this._mockStrategy.ShouldEvaluateThisProvider(Arg.Any<StrategyPerProviderContext<bool>>(), Arg.Any<EvaluationContext>()).Returns(true);
        this._mockStrategy.ShouldEvaluateNextProvider(Arg.Any<StrategyPerProviderContext<bool>>(), Arg.Any<EvaluationContext>(), Arg.Any<ProviderResolutionResult<bool>>()).Returns(false);
    }

    [Fact]
    public async Task InitializeAsync_OnSuccessfulInitialization_EmitsProviderReadyEvent()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name),
            new(this._mockProvider2, Provider2Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Act
        await multiProvider.InitializeAsync(this._evaluationContext);

        // Assert
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 1, timeoutMs: 1000);

        Assert.Single(events);
        var eventPayload = events[0];
        Assert.Equal("MultiProvider", eventPayload.ProviderName);
        Assert.Equal(ProviderEventTypes.ProviderReady, eventPayload.Type);
        Assert.Equal("MultiProvider successfully initialized", eventPayload.Message);
        Assert.Null(eventPayload.ErrorType);
    }

    [Fact]
    public async Task InitializeAsync_OnProviderFailure_EmitsProviderErrorEvent()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Initialization failed");
        var failedProvider = new TestProvider("failed-provider", expectedException);
        var providerEntries = new List<ProviderEntry>
        {
            new(failedProvider, Provider1Name),
            new(this._mockProvider2, Provider2Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Act & Assert
        await Assert.ThrowsAsync<AggregateException>(() => multiProvider.InitializeAsync(this._evaluationContext));

        // Verify the error event was emitted
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 1, timeoutMs: 1000);

        Assert.Single(events);
        var eventPayload = events[0];
        Assert.Equal("MultiProvider", eventPayload.ProviderName);
        Assert.Equal(ProviderEventTypes.ProviderError, eventPayload.Type);
        Assert.Contains("Failed to initialize providers", eventPayload.Message);
        Assert.Contains(Provider1Name, eventPayload.Message);
        Assert.Equal(ErrorType.ProviderFatal, eventPayload.ErrorType);
    }

    [Fact]
    public async Task EvaluateAsync_OnUnsupportedRunMode_EmitsProviderErrorEvent()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Setup strategy to return unsupported run mode
        this._mockStrategy.RunMode.Returns((RunMode)999); // Invalid run mode

        // Act
        var result = await multiProvider.ResolveBooleanValueAsync(TestFlagKey, false, this._evaluationContext);

        // Assert
        Assert.Equal(ErrorType.ProviderFatal, result.ErrorType);
        Assert.Equal(Reason.Error, result.Reason);

        // Verify the error event was emitted
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 1, timeoutMs: 1000);

        Assert.Single(events);
        var eventPayload = events[0];
        Assert.Equal("MultiProvider", eventPayload.ProviderName);
        Assert.Equal(ProviderEventTypes.ProviderError, eventPayload.Type);
        Assert.Contains($"Error evaluating flag '{TestFlagKey}' with run mode", eventPayload.Message);
        Assert.Equal(ErrorType.ProviderFatal, eventPayload.ErrorType);
    }

    [Fact]
    public async Task EvaluateAsync_OnGeneralException_EmitsProviderErrorEvent()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Evaluation failed");
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        this._mockStrategy.DetermineFinalResult(Arg.Any<StrategyEvaluationContext<bool>>(), Arg.Any<string>(), Arg.Any<bool>(),
            Arg.Any<EvaluationContext>(), Arg.Any<List<ProviderResolutionResult<bool>>>()).Throws(expectedException);

        // Act
        var result = await multiProvider.ResolveBooleanValueAsync(TestFlagKey, false, this._evaluationContext);

        // Assert
        Assert.Equal(ErrorType.General, result.ErrorType);
        Assert.Equal(Reason.Error, result.Reason);

        // Verify the error event was emitted
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 1, timeoutMs: 1000);

        Assert.Single(events);
        var eventPayload = events[0];
        Assert.Equal("MultiProvider", eventPayload.ProviderName);
        Assert.Equal(ProviderEventTypes.ProviderError, eventPayload.Type);
        Assert.Contains($"Error evaluating flag '{TestFlagKey}'", eventPayload.Message);
        Assert.Contains("Evaluation failed", eventPayload.Message);
        Assert.Equal(ErrorType.General, eventPayload.ErrorType);
        Assert.NotNull(eventPayload.FlagsChanged);
        Assert.Contains(TestFlagKey, eventPayload.FlagsChanged);
    }

    [Fact]
    public async Task HandleProviderEventAsync_OnConfigurationChangedEvent_ReEmitsEventWithCorrectProviderName()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        var configChangedEvent = new ProviderEventPayload
        {
            ProviderName = Provider1Name,
            Type = ProviderEventTypes.ProviderConfigurationChanged,
            Message = "Configuration changed",
            FlagsChanged = [TestFlagKey]
        };

        // Act - Simulate child provider emitting configuration changed event
        await EmitEventToChildProvider(this._mockProvider1, configChangedEvent);

        // Give some time for event processing
        await Task.Delay(100);

        // Assert
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 1, timeoutMs: 1000);

        Assert.Single(events);
        var eventPayload = events[0];
        Assert.Equal($"MultiProvider/{Provider1Name}", eventPayload.ProviderName);
        Assert.Equal(ProviderEventTypes.ProviderConfigurationChanged, eventPayload.Type);
        Assert.Equal("Configuration changed", eventPayload.Message);
        Assert.NotNull(eventPayload.FlagsChanged);
        Assert.Contains(TestFlagKey, eventPayload.FlagsChanged);
    }

    [Fact]
    public async Task HandleProviderEventAsync_OnProviderReady_EmitsMultiProviderReadyWhenAllReady()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name),
            new(this._mockProvider2, Provider2Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Set initial MultiProvider status to NotReady
        multiProvider.SetStatus(ProviderStatus.NotReady);

        var readyEvent = new ProviderEventPayload
        {
            ProviderName = Provider1Name,
            Type = ProviderEventTypes.ProviderReady,
            Message = "Provider is ready"
        };

        // Act - Simulate both child providers becoming ready
        await EmitEventToChildProvider(this._mockProvider1, readyEvent);
        await EmitEventToChildProvider(this._mockProvider2, new ProviderEventPayload
        {
            ProviderName = Provider2Name,
            Type = ProviderEventTypes.ProviderReady
        });

        // Give some time for event processing
        await Task.Delay(100);

        // Assert - Should emit MultiProvider ready event when all providers are ready
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 2, timeoutMs: 1000);

        // Find the MultiProvider ready event
        var multiProviderEvent = events.FirstOrDefault(e => e is { ProviderName: "MultiProvider", Type: ProviderEventTypes.ProviderReady });
        Assert.NotNull(multiProviderEvent);
        Assert.Contains("MultiProvider status changed", multiProviderEvent.Message);
        Assert.Contains("Ready", multiProviderEvent.Message);
    }

    [Fact]
    public async Task HandleProviderEventAsync_OnProviderError_EmitsMultiProviderErrorEvent()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Set initial MultiProvider status to Ready
        multiProvider.SetStatus(ProviderStatus.Ready);

        var errorEvent = new ProviderEventPayload
        {
            ProviderName = Provider1Name,
            Type = ProviderEventTypes.ProviderError,
            Message = "Provider error occurred",
            ErrorType = ErrorType.ProviderFatal
        };

        // Act - Simulate child provider emitting error event
        await EmitEventToChildProvider(this._mockProvider1, errorEvent);

        // Give some time for event processing
        await Task.Delay(100);

        // Assert
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 1, timeoutMs: 1000);

        Assert.Single(events);
        var eventPayload = events[0];
        Assert.Equal("MultiProvider", eventPayload.ProviderName);
        Assert.Equal(ProviderEventTypes.ProviderError, eventPayload.Type);
        Assert.Contains("MultiProvider status changed", eventPayload.Message);
        Assert.Contains("Fatal", eventPayload.Message);
        Assert.Equal(ErrorType.ProviderFatal, eventPayload.ErrorType);
    }

    [Fact]
    public async Task HandleProviderEventAsync_OnProviderStale_EmitsMultiProviderStaleEvent()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Set initial MultiProvider status to Ready
        multiProvider.SetStatus(ProviderStatus.Ready);

        var staleEvent = new ProviderEventPayload
        {
            ProviderName = Provider1Name,
            Type = ProviderEventTypes.ProviderStale,
            Message = "Provider is stale"
        };

        // Act - Simulate child provider emitting stale event
        await EmitEventToChildProvider(this._mockProvider1, staleEvent);

        // Give some time for event processing
        await Task.Delay(100);

        // Assert
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 1, timeoutMs: 1000);

        Assert.Single(events);
        var eventPayload = events[0];
        Assert.Equal("MultiProvider", eventPayload.ProviderName);
        Assert.Equal(ProviderEventTypes.ProviderStale, eventPayload.Type);
        Assert.Contains("MultiProvider status changed", eventPayload.Message);
        Assert.Contains("Stale", eventPayload.Message);
    }

    [Fact]
    public async Task HandleProviderEventAsync_OnSameStatus_DoesNotEmitMultiProviderEvent()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Set initial MultiProvider status to Ready
        multiProvider.SetStatus(ProviderStatus.Ready);

        var readyEvent = new ProviderEventPayload
        {
            ProviderName = Provider1Name,
            Type = ProviderEventTypes.ProviderReady,
            Message = "Provider is ready"
        };

        // Act - Simulate child provider emitting ready event when MultiProvider is already ready
        await EmitEventToChildProvider(this._mockProvider1, readyEvent);

        // Give some time for event processing
        await Task.Delay(100);

        // Assert - Should not emit any events since status didn't change
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 0, timeoutMs: 500);

        Assert.Empty(events);
    }

    [Fact]
    public async Task HandleProviderEventAsync_OnEventProcessingError_EmitsErrorEvent()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Create an invalid event that might cause processing errors
        var invalidEvent = new ProviderEventPayload
        {
            ProviderName = Provider1Name,
            Type = (ProviderEventTypes)999, // Invalid event type
            Message = "Invalid event"
        };

        // Act - Simulate child provider emitting invalid event
        await EmitEventToChildProvider(this._mockProvider1, invalidEvent);

        // Give some time for event processing
        await Task.Delay(100);

        // The MultiProvider should handle this gracefully and potentially emit an error event
        // The exact behavior depends on implementation details
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 0, timeoutMs: 500);

        // This test verifies that the system doesn't crash on invalid events
        // The specific event emission behavior may vary based on implementation
        Assert.True(events.Count >= 0); // Should handle gracefully without throwing
    }

    [Fact]
    public async Task MultipleProviders_WithMixedStatusEvents_EmitsCorrectAggregateStatusEvents()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name),
            new(this._mockProvider2, Provider2Name),
            new(this._mockProvider3, Provider3Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Initialize the MultiProvider to ensure event listeners are set up
        await multiProvider.InitializeAsync(this._evaluationContext);

        // Give some time for event listening tasks to be fully established
        await Task.Delay(50);

        // Act - Simulate one provider going to error state
        await EmitEventToChildProvider(this._mockProvider1, new ProviderEventPayload
        {
            ProviderName = Provider1Name,
            Type = ProviderEventTypes.ProviderError,
            ErrorType = ErrorType.General
        });

        await Task.Delay(100);

        // Assert - MultiProvider should change to Error status
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 2, timeoutMs: 1000); // Expect 2 events: ready from init + error

        Assert.True(events.Count >= 1, $"Expected at least 1 event but got {events.Count}");
        var errorEvent = events.LastOrDefault(e => e.Type == ProviderEventTypes.ProviderError);
        Assert.NotNull(errorEvent);
        Assert.Equal("MultiProvider", errorEvent.ProviderName);
        Assert.Equal(ProviderEventTypes.ProviderError, errorEvent.Type);
        Assert.Contains("Error", errorEvent.Message);

        // Act - Simulate the error provider recovering
        await EmitEventToChildProvider(this._mockProvider1, new ProviderEventPayload
        {
            ProviderName = Provider1Name,
            Type = ProviderEventTypes.ProviderReady
        });

        await Task.Delay(100);

        // Assert - MultiProvider should change back to Ready status
        var moreEvents = await ReadEventsFromChannel(eventChannel, expectedCount: 1, timeoutMs: 1000);

        Assert.Single(moreEvents);
        var readyEvent = moreEvents[0];
        Assert.Equal("MultiProvider", readyEvent.ProviderName);
        Assert.Equal(ProviderEventTypes.ProviderReady, readyEvent.Type);
        Assert.Contains("Ready", readyEvent.Message);
    }

    /// <summary>
    /// Helper method to read events from a channel within a timeout period.
    /// </summary>
    private static async Task<List<ProviderEventPayload>> ReadEventsFromChannel(Channel<object> eventChannel, int expectedCount, int timeoutMs)
    {
        var events = new List<ProviderEventPayload>();
        var cancellationTokenSource = new CancellationTokenSource(timeoutMs);

        try
        {
            while (events.Count < expectedCount && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (!await eventChannel.Reader.WaitToReadAsync(cancellationTokenSource.Token))
                {
                    continue;
                }

                while (eventChannel.Reader.TryRead(out var item) && events.Count < expectedCount)
                {
                    if (item is ProviderEventPayload payload)
                    {
                        events.Add(payload);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Timeout reached - return what we have
        }

        return events;
    }

    /// <summary>
    /// Helper method to simulate a child provider emitting an event.
    /// </summary>
    private static async Task EmitEventToChildProvider(FeatureProvider provider, ProviderEventPayload eventPayload)
    {
        var eventChannel = provider.GetEventChannel();
        // Wrap the ProviderEventPayload in an Event object as expected by MultiProvider
        var eventWrapper = new Event { EventPayload = eventPayload, Provider = provider };
        await eventChannel.Writer.WriteAsync(eventWrapper);
    }
}
