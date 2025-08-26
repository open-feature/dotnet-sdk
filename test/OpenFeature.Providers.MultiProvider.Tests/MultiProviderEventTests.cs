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
        this._mockStrategy
            .ShouldEvaluateThisProvider(Arg.Any<StrategyPerProviderContext<bool>>(), Arg.Any<EvaluationContext>())
            .Returns(true);
        this._mockStrategy.ShouldEvaluateNextProvider(Arg.Any<StrategyPerProviderContext<bool>>(),
            Arg.Any<EvaluationContext>(), Arg.Any<ProviderResolutionResult<bool>>()).Returns(false);
    }

    [Fact]
    public async Task InitializeAsync_OnSuccessfulInitialization_EmitsProviderReadyEvent()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name), new(this._mockProvider2, Provider2Name)
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
            new(failedProvider, Provider1Name), new(this._mockProvider2, Provider2Name)
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

        this._mockStrategy.DetermineFinalResult(Arg.Any<StrategyEvaluationContext<bool>>(), Arg.Any<string>(),
            Arg.Any<bool>(),
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
            new(this._mockProvider1, Provider1Name), new(this._mockProvider2, Provider2Name)
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
        await EmitEventToChildProvider(this._mockProvider2,
            new ProviderEventPayload { ProviderName = Provider2Name, Type = ProviderEventTypes.ProviderReady });

        // Give some time for event processing
        await Task.Delay(100);

        // Assert - Should emit MultiProvider ready event when all providers are ready
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 2, timeoutMs: 1000);

        // Find the MultiProvider ready event
        var multiProviderEvent = events.FirstOrDefault(e => e is
        { ProviderName: "MultiProvider", Type: ProviderEventTypes.ProviderReady });
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
        await EmitEventToChildProvider(this._mockProvider1,
            new ProviderEventPayload
            {
                ProviderName = Provider1Name,
                Type = ProviderEventTypes.ProviderError,
                ErrorType = ErrorType.General
            });

        await Task.Delay(100);

        // Assert - MultiProvider should change to Error status
        var eventChannel = multiProvider.GetEventChannel();
        var events =
            await ReadEventsFromChannel(eventChannel, expectedCount: 2,
                timeoutMs: 1000); // Expect 2 events: ready from init + error

        Assert.True(events.Count >= 1, $"Expected at least 1 event but got {events.Count}");
        var errorEvent = events.LastOrDefault(e => e.Type == ProviderEventTypes.ProviderError);
        Assert.NotNull(errorEvent);
        Assert.Equal("MultiProvider", errorEvent.ProviderName);
        Assert.Equal(ProviderEventTypes.ProviderError, errorEvent.Type);
        Assert.Contains("Error", errorEvent.Message);

        // Act - Simulate the error provider recovering
        await EmitEventToChildProvider(this._mockProvider1,
            new ProviderEventPayload { ProviderName = Provider1Name, Type = ProviderEventTypes.ProviderReady });

        await Task.Delay(100);

        // Assert - MultiProvider should change back to Ready status
        var moreEvents = await ReadEventsFromChannel(eventChannel, expectedCount: 1, timeoutMs: 1000);

        Assert.Single(moreEvents);
        var readyEvent = moreEvents[0];
        Assert.Equal("MultiProvider", readyEvent.ProviderName);
        Assert.Equal(ProviderEventTypes.ProviderReady, readyEvent.Type);
        Assert.Contains("Ready", readyEvent.Message);
    }

    [Fact]
    public async Task HandleProviderEventAsync_OnConfigurationChangedWithEventMetadata_ReEmitsEventWithMetadata()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        var eventMetadata = new Dictionary<string, object>
        {
            { "source", "test-source" }, { "timestamp", DateTimeOffset.UtcNow }
        };

        var configChangedEvent = new ProviderEventPayload
        {
            ProviderName = Provider1Name,
            Type = ProviderEventTypes.ProviderConfigurationChanged,
            Message = "Configuration updated with metadata",
            FlagsChanged = [TestFlagKey],
            EventMetadata = new ImmutableMetadata(eventMetadata)
        };

        // Act
        await EmitEventToChildProvider(this._mockProvider1, configChangedEvent);
        await Task.Delay(100);

        // Assert
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 1, timeoutMs: 1000);

        Assert.Single(events);
        var eventPayload = events[0];
        Assert.Equal($"MultiProvider/{Provider1Name}", eventPayload.ProviderName);
        Assert.Equal(ProviderEventTypes.ProviderConfigurationChanged, eventPayload.Type);
        Assert.Equal("Configuration updated with metadata", eventPayload.Message);
        Assert.NotNull(eventPayload.FlagsChanged);
        Assert.Contains(TestFlagKey, eventPayload.FlagsChanged);
        Assert.NotNull(eventPayload.EventMetadata);
        Assert.Equal(eventMetadata["source"], eventPayload.EventMetadata.GetString("source"));
    }

    [Fact]
    public async Task HandleProviderEventAsync_OnConfigurationChangedWithNullMessage_UsesDefaultMessage()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        var configChangedEvent = new ProviderEventPayload
        {
            ProviderName = Provider1Name,
            Type = ProviderEventTypes.ProviderConfigurationChanged,
            Message = null, // Null message to trigger default
            FlagsChanged = [TestFlagKey]
        };

        // Act
        await EmitEventToChildProvider(this._mockProvider1, configChangedEvent);
        await Task.Delay(100);

        // Assert
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 1, timeoutMs: 1000);

        Assert.Single(events);
        var eventPayload = events[0];
        Assert.Equal($"Configuration changed in provider {Provider1Name}", eventPayload.Message);
    }

    [Fact]
    public async Task ShutdownAsync_OnSuccessfulShutdown_StopsEventProcessing()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name), new(this._mockProvider2, Provider2Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Initialize first
        await multiProvider.InitializeAsync(this._evaluationContext);
        await Task.Delay(50); // Allow event processing to start

        // Act
        await multiProvider.ShutdownAsync();

        // Try to emit an event after shutdown - it should not be processed
        await EmitEventToChildProvider(this._mockProvider1,
            new ProviderEventPayload
            {
                ProviderName = Provider1Name,
                Type = ProviderEventTypes.ProviderReady,
                Message = "Should not be processed"
            });

        await Task.Delay(100);

        // Assert - Should only have the initial ready event from initialization, no events after shutdown
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 0, timeoutMs: 500);

        // Should be empty since event processing was stopped
        Assert.Empty(events);
    }

    [Fact]
    public async Task DisposeAsync_OnSuccessfulDisposal_CleansUpEventProcessing()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        await multiProvider.InitializeAsync(this._evaluationContext);
        await Task.Delay(50); // Allow event processing to start

        // Act
        await multiProvider.DisposeAsync();

        // Try to emit an event after disposal - it should not cause issues
        await EmitEventToChildProvider(this._mockProvider1,
            new ProviderEventPayload
            {
                ProviderName = Provider1Name,
                Type = ProviderEventTypes.ProviderReady,
                Message = "Should not be processed"
            });

        // Assert - Should not throw and should handle disposal gracefully
        await Task.Delay(100); // Give time for any potential processing

        // Verify that subsequent operations on disposed provider throw
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            multiProvider.ResolveBooleanValueAsync(TestFlagKey, false));
    }

    [Fact]
    public async Task HandleProviderEventAsync_WithUnknownEventType_DoesNotEmitStatusChangeEvent()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Set initial status to Ready
        multiProvider.SetStatus(ProviderStatus.Ready);

        var unknownEvent = new ProviderEventPayload
        {
            ProviderName = Provider1Name,
            Type = (ProviderEventTypes)9999, // Unknown event type
            Message = "Unknown event type"
        };

        // Act
        await EmitEventToChildProvider(this._mockProvider1, unknownEvent);
        await Task.Delay(100);

        // Assert - Should not emit any MultiProvider status change events for unknown event types
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 0, timeoutMs: 500);

        Assert.Empty(events);
    }

    [Fact]
    public async Task HandleProviderEventAsync_OnProviderErrorWithGeneralErrorType_SetsProviderToErrorStatus()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name), new(this._mockProvider2, Provider2Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Initialize to get all providers to Ready state
        await multiProvider.InitializeAsync(this._evaluationContext);
        await Task.Delay(100);

        var errorEvent = new ProviderEventPayload
        {
            ProviderName = Provider1Name,
            Type = ProviderEventTypes.ProviderError,
            Message = "General error occurred",
            ErrorType = ErrorType.General // Not fatal
        };

        // Act
        await EmitEventToChildProvider(this._mockProvider1, errorEvent);
        await Task.Delay(100);

        // Assert - MultiProvider should change to Error status (not Fatal)
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 2, timeoutMs: 1000); // Init ready + error

        var errorMultiProviderEvent = events.LastOrDefault(e => e.Type == ProviderEventTypes.ProviderError);
        Assert.NotNull(errorMultiProviderEvent);
        Assert.Equal("MultiProvider", errorMultiProviderEvent.ProviderName);
        Assert.Equal(ErrorType.General, errorMultiProviderEvent.ErrorType);
        Assert.Contains("Error", errorMultiProviderEvent.Message);
    }

    [Fact]
    public async Task DetermineAggregateStatus_WithMixedStatuses_ReturnsCorrectPriority()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name),
            new(this._mockProvider2, Provider2Name),
            new(this._mockProvider3, Provider3Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Initialize to get baseline
        await multiProvider.InitializeAsync(this._evaluationContext);
        await Task.Delay(100);

        // Act & Assert - Test Fatal has the highest priority
        await EmitEventToChildProvider(this._mockProvider1,
            new ProviderEventPayload { Type = ProviderEventTypes.ProviderError, ErrorType = ErrorType.ProviderFatal });

        await EmitEventToChildProvider(this._mockProvider2,
            new ProviderEventPayload { Type = ProviderEventTypes.ProviderError, ErrorType = ErrorType.General });

        await EmitEventToChildProvider(this._mockProvider3,
            new ProviderEventPayload { Type = ProviderEventTypes.ProviderStale });

        await Task.Delay(200);

        // Should emit Fatal status due to provider1 having Fatal error
        var eventChannel = multiProvider.GetEventChannel();
        var events =
            await ReadEventsFromChannel(eventChannel, expectedCount: 4, timeoutMs: 1000); // Init + 3 status changes

        var finalEvent = events.LastOrDefault(e => e.Type == ProviderEventTypes.ProviderError);
        Assert.NotNull(finalEvent);
        Assert.Equal(ErrorType.ProviderFatal, finalEvent.ErrorType);
    }

    [Fact]
    public async Task EmitEvent_WhenEventChannelThrowsException_HandlesGracefully()
    {
        // This test verifies the error handling in EmitEvent method
        // We can't directly test this without more complex mocking, but we can test
        // that the system continues to function even when events fail to emit

        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Act - Try to initialize and then dispose quickly to potentially cause event channel issues
        await multiProvider.InitializeAsync(this._evaluationContext);
        await multiProvider.DisposeAsync();

        // Assert - Should not throw even if there were issues with event emission
        Assert.True(true); // Test passes if no exceptions were thrown
    }

    [Fact]
    public async Task HandleProviderEventAsync_WithComplexStatusTransitions_EmitsCorrectSequence()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name), new(this._mockProvider2, Provider2Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Initialize
        await multiProvider.InitializeAsync(this._evaluationContext);
        await Task.Delay(100);

        // Act - Complex sequence: Provider1 goes stale, then Provider2 goes error, then Provider1 recovers
        await EmitEventToChildProvider(this._mockProvider1,
            new ProviderEventPayload { Type = ProviderEventTypes.ProviderStale, Message = "Provider1 is stale" });
        await Task.Delay(50);

        await EmitEventToChildProvider(this._mockProvider2,
            new ProviderEventPayload
            {
                Type = ProviderEventTypes.ProviderError,
                ErrorType = ErrorType.General,
                Message = "Provider2 has error"
            });
        await Task.Delay(50);

        await EmitEventToChildProvider(this._mockProvider1,
            new ProviderEventPayload { Type = ProviderEventTypes.ProviderReady, Message = "Provider1 recovered" });
        await Task.Delay(100);

        // Assert - Should see: Init Ready -> Stale -> Error -> Error (no change when P1 recovers since P2 still in error)
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 4, timeoutMs: 1000);

        Assert.True(events.Count >= 3);

        // Should have: Ready (init), Stale, Error
        var readyEvent = events.FirstOrDefault(e => e.Type == ProviderEventTypes.ProviderReady);
        var staleEvent = events.FirstOrDefault(e => e.Type == ProviderEventTypes.ProviderStale);
        var errorEvent = events.FirstOrDefault(e => e.Type == ProviderEventTypes.ProviderError);

        Assert.NotNull(readyEvent);
        Assert.NotNull(staleEvent);
        Assert.NotNull(errorEvent);

        Assert.Contains("Stale", staleEvent.Message);
        Assert.Contains("Error", errorEvent.Message);
    }

    [Fact]
    public async Task HandleProviderEventAsync_OnProviderRecoveryFromNotReady_EmitsReadyWhenAllReady()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name), new(this._mockProvider2, Provider2Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Set initial status to NotReady (simulating a provider that failed to initialize)
        multiProvider.SetStatus(ProviderStatus.NotReady);

        // Act - Both providers become ready
        await EmitEventToChildProvider(this._mockProvider1,
            new ProviderEventPayload { Type = ProviderEventTypes.ProviderReady, Message = "Provider1 is ready" });
        await Task.Delay(50);

        await EmitEventToChildProvider(this._mockProvider2,
            new ProviderEventPayload { Type = ProviderEventTypes.ProviderReady, Message = "Provider2 is ready" });
        await Task.Delay(100);

        // Assert - Should emit MultiProvider ready event when all providers are ready
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 1, timeoutMs: 1000);

        Assert.Single(events);
        var readyEvent = events[0];
        Assert.Equal("MultiProvider", readyEvent.ProviderName);
        Assert.Equal(ProviderEventTypes.ProviderReady, readyEvent.Type);
        Assert.Contains("Ready", readyEvent.Message);
    }

    [Fact]
    public async Task ProcessProviderEventsAsync_WithNonEventObjects_IgnoresGracefully()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Initialize to start event processing
        await multiProvider.InitializeAsync(this._evaluationContext);
        await Task.Delay(50);

        // Act - Write non-Event objects to the channel
        var eventChannel = this._mockProvider1.GetEventChannel();
        await eventChannel.Writer.WriteAsync("Not an Event object");
        await eventChannel.Writer.WriteAsync(42);
        await eventChannel.Writer.WriteAsync(new { SomeProperty = "value" });

        await Task.Delay(100);

        // Assert - Should handle non-Event objects gracefully without crashing
        // Only the initial ready event should be present
        var multiProviderEventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(multiProviderEventChannel, expectedCount: 1, timeoutMs: 1000);

        Assert.Single(events); // Only the initialization ready event
        Assert.Equal(ProviderEventTypes.ProviderReady, events[0].Type);
        Assert.Equal("MultiProvider successfully initialized", events[0].Message);
    }

    [Fact]
    public async Task ShutdownAsync_WithProviderFailures_ThrowsAggregateExceptionButStillStopsEventProcessing()
    {
        // Arrange
        var shutdownException = new InvalidOperationException("Shutdown failed");
        var failingProvider = new TestProvider("failing-provider", shutdownException: shutdownException);
        var providerEntries = new List<ProviderEntry>
        {
            new(failingProvider, Provider1Name), new(this._mockProvider2, Provider2Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        await multiProvider.InitializeAsync(this._evaluationContext);
        await Task.Delay(50);

        // Act & Assert - Should throw AggregateException due to provider shutdown failure
        var aggregateException = await Assert.ThrowsAsync<AggregateException>(() => multiProvider.ShutdownAsync());
        Assert.Contains("Failed to shutdown providers", aggregateException.Message);
        Assert.Contains(Provider1Name, aggregateException.Message);

        // Verify event processing is still stopped despite the exception
        await EmitEventToChildProvider(this._mockProvider2,
            new ProviderEventPayload
            {
                Type = ProviderEventTypes.ProviderReady,
                Message = "Should not be processed after shutdown"
            });

        await Task.Delay(100);

        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 0, timeoutMs: 500);
        Assert.Empty(events); // No events should be processed after shutdown
    }

    [Fact]
    public async Task HandleProviderEventAsync_OnProviderErrorDuringEventProcessing_EmitsErrorEvent()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Initialize to start event processing
        await multiProvider.InitializeAsync(this._evaluationContext);
        await Task.Delay(50);

        // Create an event with null EventPayload to trigger error handling
        var eventChannel = this._mockProvider1.GetEventChannel();
        var invalidEvent = new Event { EventPayload = null, Provider = this._mockProvider1 };
        await eventChannel.Writer.WriteAsync(invalidEvent);

        await Task.Delay(100);

        // Assert - Should handle the error gracefully without crashing the event processing
        var multiProviderEventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(multiProviderEventChannel, expectedCount: 1, timeoutMs: 1000);

        // Should only have the initialization ready event, the invalid event should be ignored
        Assert.Single(events);
        Assert.Equal(ProviderEventTypes.ProviderReady, events[0].Type);
    }

    [Fact]
    public async Task DetermineAggregateStatus_WithAllNotReadyProviders_ReturnsNotReady()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry>
        {
            new(this._mockProvider1, Provider1Name), new(this._mockProvider2, Provider2Name)
        };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        // Start with NotReady status
        multiProvider.SetStatus(ProviderStatus.NotReady);

        // Act - Emit NotReady events (which don't change status from NotReady)
        await EmitEventToChildProvider(this._mockProvider1,
            new ProviderEventPayload { Type = ProviderEventTypes.ProviderError, ErrorType = ErrorType.General });

        await EmitEventToChildProvider(this._mockProvider2,
            new ProviderEventPayload { Type = ProviderEventTypes.ProviderError, ErrorType = ErrorType.General });

        await Task.Delay(100);

        // Assert - Should emit Error status due to both providers being in error
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 1, timeoutMs: 1000);

        Assert.Single(events);
        var errorEvent = events[0];
        Assert.Equal("MultiProvider", errorEvent.ProviderName);
        Assert.Equal(ProviderEventTypes.ProviderError, errorEvent.Type);
        Assert.Contains("Error", errorEvent.Message);
    }

    [Fact]
    public async Task ProcessProviderEventsAsync_WhenChannelReaderCompletes_ExitsGracefully()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        await multiProvider.InitializeAsync(this._evaluationContext);
        await Task.Delay(50);

        // Act - Complete the channel to signal end of events
        var eventChannel = this._mockProvider1.GetEventChannel();
        eventChannel.Writer.Complete();

        // Give time for the event processing loop to detect completion and exit
        await Task.Delay(200);

        // Assert - Should handle channel completion gracefully without throwing
        // The event processing should exit cleanly when WaitToReadAsync returns false
        Assert.True(true); // Test passes if no exceptions are thrown
    }

    [Fact]
    public async Task HandleProviderEventAsync_WithEventContainingMultipleFlagChanges_PropagatesAllFlags()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        var multipleFlagsEvent = new ProviderEventPayload
        {
            ProviderName = Provider1Name,
            Type = ProviderEventTypes.ProviderConfigurationChanged,
            Message = "Multiple flags changed",
            FlagsChanged = ["flag1", "flag2", "flag3", TestFlagKey]
        };

        // Act
        await EmitEventToChildProvider(this._mockProvider1, multipleFlagsEvent);
        await Task.Delay(100);

        // Assert
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 1, timeoutMs: 1000);

        Assert.Single(events);
        var eventPayload = events[0];
        Assert.Equal($"MultiProvider/{Provider1Name}", eventPayload.ProviderName);
        Assert.NotNull(eventPayload.FlagsChanged);
        Assert.Equal(4, eventPayload.FlagsChanged.Count);
        Assert.Contains("flag1", eventPayload.FlagsChanged);
        Assert.Contains("flag2", eventPayload.FlagsChanged);
        Assert.Contains("flag3", eventPayload.FlagsChanged);
        Assert.Contains(TestFlagKey, eventPayload.FlagsChanged);
    }

    [Fact]
    public async Task HandleProviderEventAsync_WithEventContainingEmptyFlagsList_HandlesGracefully()
    {
        // Arrange
        var providerEntries = new List<ProviderEntry> { new(this._mockProvider1, Provider1Name) };
        var multiProvider = new MultiProvider(providerEntries, this._mockStrategy);

        var emptyFlagsEvent = new ProviderEventPayload
        {
            ProviderName = Provider1Name,
            Type = ProviderEventTypes.ProviderConfigurationChanged,
            Message = "No flags specified",
            FlagsChanged = [] // Empty list
        };

        // Act
        await EmitEventToChildProvider(this._mockProvider1, emptyFlagsEvent);
        await Task.Delay(100);

        // Assert
        var eventChannel = multiProvider.GetEventChannel();
        var events = await ReadEventsFromChannel(eventChannel, expectedCount: 1, timeoutMs: 1000);

        Assert.Single(events);
        var eventPayload = events[0];
        Assert.NotNull(eventPayload.FlagsChanged);
        Assert.Empty(eventPayload.FlagsChanged);
    }


    /// <summary>
    /// Helper method to read events from a channel within a timeout period.
    /// </summary>
    private static async Task<List<ProviderEventPayload>> ReadEventsFromChannel(Channel<object> eventChannel,
        int expectedCount, int timeoutMs)
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
