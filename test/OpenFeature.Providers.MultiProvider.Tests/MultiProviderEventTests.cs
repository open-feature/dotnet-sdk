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
/// </summary>
public class MultiProviderEventTests
{
    private const string TestFlagKey = "test-flag";
    private const string Provider1Name = "provider1";
    private const string Provider2Name = "provider2";

    private readonly FeatureProvider _provider1 = new TestProvider(Provider1Name);
    private readonly FeatureProvider _provider2 = new TestProvider(Provider2Name);
    private readonly BaseEvaluationStrategy _strategy = Substitute.For<BaseEvaluationStrategy>();
    private readonly EvaluationContext _context = new EvaluationContextBuilder().Build();

    public MultiProviderEventTests()
    {
        _strategy.RunMode.Returns(RunMode.Sequential);
        _strategy.ShouldEvaluateThisProvider(Arg.Any<StrategyPerProviderContext<bool>>(), Arg.Any<EvaluationContext>()).Returns(true);
        _strategy.ShouldEvaluateNextProvider(Arg.Any<StrategyPerProviderContext<bool>>(), Arg.Any<EvaluationContext>(), Arg.Any<ProviderResolutionResult<bool>>()).Returns(false);
    }

    [Fact]
    public async Task InitializeAsync_OnSuccess_EmitsProviderReadyEvent()
    {
        // Arrange
        var multiProvider = CreateMultiProvider(_provider1, _provider2);

        // Act
        await multiProvider.InitializeAsync(_context);

        // Assert
        var events = await ReadEvents(multiProvider.GetEventChannel());
        Assert.Single(events);
        AssertEvent(events[0], "MultiProvider", ProviderEventTypes.ProviderReady, "MultiProvider successfully initialized");
    }

    [Fact]
    public async Task InitializeAsync_OnProviderFailure_EmitsProviderErrorEvent()
    {
        // Arrange
        var failedProvider = new TestProvider("failed", new InvalidOperationException("Init failed"));
        var multiProvider = CreateMultiProvider(failedProvider, _provider2);

        // Act & Assert
        await Assert.ThrowsAsync<AggregateException>(() => multiProvider.InitializeAsync(_context));

        // Verify the error event was emitted
        var events = await ReadEvents(multiProvider.GetEventChannel());
        Assert.Single(events);
        AssertEvent(events[0], "MultiProvider", ProviderEventTypes.ProviderError, errorType: ErrorType.ProviderFatal);
    }

    [Fact]
    public async Task EvaluateAsync_OnUnsupportedRunMode_EmitsProviderErrorEvent()
    {
        // Arrange
        var multiProvider = CreateMultiProvider(_provider1);
        _strategy.RunMode.Returns((RunMode)999); // Invalid run mode

        // Act
        var result = await multiProvider.ResolveBooleanValueAsync(TestFlagKey, false, _context);

        // Assert
        Assert.Equal(ErrorType.ProviderFatal, result.ErrorType);
        Assert.Equal(Reason.Error, result.Reason);

        // Verify the error event was emitted
        var events = await ReadEvents(multiProvider.GetEventChannel());
        Assert.Single(events);
        AssertEvent(events[0], "MultiProvider", ProviderEventTypes.ProviderError, errorType: ErrorType.ProviderFatal);
    }

    [Fact]
    public async Task EvaluateAsync_OnGeneralException_EmitsProviderErrorEvent()
    {
        // Arrange
        var multiProvider = CreateMultiProvider(_provider1);

        _strategy.DetermineFinalResult(Arg.Any<StrategyEvaluationContext<bool>>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<EvaluationContext>(), Arg.Any<List<ProviderResolutionResult<bool>>>())
            .Throws(new InvalidOperationException("Evaluation failed"));

        // Act
        var result = await multiProvider.ResolveBooleanValueAsync(TestFlagKey, false, _context);

        // Assert
        Assert.Equal(ErrorType.General, result.ErrorType);
        Assert.Equal(Reason.Error, result.Reason);

        // Verify the error event was emitted
        var events = await ReadEvents(multiProvider.GetEventChannel());
        Assert.Single(events);
        AssertEvent(events[0], "MultiProvider", ProviderEventTypes.ProviderError, errorType: ErrorType.General);
    }

    [Fact]
    public async Task HandleProviderEvent_OnConfigurationChanged_ReEmitsEventWithCorrectProviderName()
    {
        // Arrange
        var multiProvider = CreateMultiProvider(_provider1);
        var configEvent = new ProviderEventPayload
        {
            ProviderName = Provider1Name,
            Type = ProviderEventTypes.ProviderConfigurationChanged,
            Message = "Config changed",
            FlagsChanged = [TestFlagKey]
        };

        // Act - Simulate child provider emitting configuration changed event
        await EmitEventToProvider(_provider1, configEvent);
        await Task.Delay(50);

        // Assert
        var events = await ReadEvents(multiProvider.GetEventChannel());
        Assert.Single(events);
        AssertEvent(events[0], $"MultiProvider/{Provider1Name}", ProviderEventTypes.ProviderConfigurationChanged, "Config changed");
        Assert.Contains(TestFlagKey, events[0].FlagsChanged!);
    }

    [Fact]
    public async Task HandleProviderEvent_OnProviderReady_EmitsMultiProviderReadyWhenAllReady()
    {
        // Arrange
        var multiProvider = CreateMultiProvider(_provider1, _provider2);
        multiProvider.SetStatus(ProviderStatus.NotReady);

        // Act - Simulate both child providers becoming ready
        await EmitEventToProvider(_provider1, CreateEvent(ProviderEventTypes.ProviderReady));
        await EmitEventToProvider(_provider2, CreateEvent(ProviderEventTypes.ProviderReady));
        await Task.Delay(50);

        // Assert - Should emit MultiProvider ready event when all providers are ready
        var events = await ReadEvents(multiProvider.GetEventChannel(), expectedCount: 2);
        var readyEvent = events.FirstOrDefault(e => e.Type == ProviderEventTypes.ProviderReady);
        Assert.NotNull(readyEvent);
        AssertEvent(readyEvent, "MultiProvider", ProviderEventTypes.ProviderReady);
    }

    [Fact]
    public async Task HandleProviderEvent_OnProviderError_EmitsMultiProviderErrorEvent()
    {
        // Arrange
        var multiProvider = CreateMultiProvider(_provider1);
        multiProvider.SetStatus(ProviderStatus.Ready);

        // Act - Simulate child provider emitting error event
        await EmitEventToProvider(_provider1, CreateEvent(ProviderEventTypes.ProviderError, ErrorType.ProviderFatal));
        await Task.Delay(50);

        // Assert
        var events = await ReadEvents(multiProvider.GetEventChannel());
        Assert.Single(events);
        AssertEvent(events[0], "MultiProvider", ProviderEventTypes.ProviderError, errorType: ErrorType.ProviderFatal);
    }

    [Fact]
    public async Task HandleProviderEvent_OnProviderStale_EmitsMultiProviderStaleEvent()
    {
        // Arrange
        var multiProvider = CreateMultiProvider(_provider1);
        multiProvider.SetStatus(ProviderStatus.Ready);

        // Act - Simulate child provider emitting stale event
        await EmitEventToProvider(_provider1, CreateEvent(ProviderEventTypes.ProviderStale));
        await Task.Delay(50);

        // Assert
        var events = await ReadEvents(multiProvider.GetEventChannel());
        Assert.Single(events);
        AssertEvent(events[0], "MultiProvider", ProviderEventTypes.ProviderStale);
    }

    [Fact]
    public async Task HandleProviderEvent_OnSameStatus_DoesNotEmitEvent()
    {
        // Arrange
        var multiProvider = CreateMultiProvider(_provider1);
        multiProvider.SetStatus(ProviderStatus.Ready);

        // Act - Simulate child provider emitting ready event when MultiProvider is already ready
        await EmitEventToProvider(_provider1, CreateEvent(ProviderEventTypes.ProviderReady));
        await Task.Delay(50);

        // Assert - Should not emit any events since status didn't change
        var events = await ReadEvents(multiProvider.GetEventChannel(), expectedCount: 0, timeoutMs: 300);
        Assert.Empty(events);
    }

    [Fact]
    public async Task MultipleProviders_WithStatusTransitions_EmitsCorrectAggregateEvents()
    {
        // Arrange
        var multiProvider = CreateMultiProvider(_provider1, _provider2);
        await multiProvider.InitializeAsync(_context);
        await Task.Delay(50);

        // Act - Simulate one provider going to error state
        await EmitEventToProvider(_provider1, CreateEvent(ProviderEventTypes.ProviderError, ErrorType.General));
        await Task.Delay(50);
        // Simulate the error provider recovering
        await EmitEventToProvider(_provider1, CreateEvent(ProviderEventTypes.ProviderReady));
        await Task.Delay(50);

        // Assert - Should see: Init Ready -> Error -> Ready
        var events = await ReadEvents(multiProvider.GetEventChannel(), expectedCount: 3);
        Assert.Contains(events, e => e.Type == ProviderEventTypes.ProviderReady);
        Assert.Contains(events, e => e.Type == ProviderEventTypes.ProviderError);
    }

    [Fact]
    public async Task HandleProviderEvent_WithEventMetadata_PropagatesMetadata()
    {
        // Arrange
        var multiProvider = CreateMultiProvider(_provider1);
        var metadata = new Dictionary<string, object> { { "source", "test" } };
        var eventPayload = new ProviderEventPayload
        {
            ProviderName = Provider1Name,
            Type = ProviderEventTypes.ProviderConfigurationChanged,
            EventMetadata = new ImmutableMetadata(metadata)
        };

        // Act
        await EmitEventToProvider(_provider1, eventPayload);
        await Task.Delay(50);

        // Assert
        var events = await ReadEvents(multiProvider.GetEventChannel());
        Assert.Single(events);
        Assert.NotNull(events[0].EventMetadata);
        Assert.Equal("test", events[0].EventMetadata?.GetString("source"));
    }

    [Fact]
    public async Task ShutdownAsync_StopsEventProcessing()
    {
        // Arrange
        var multiProvider = CreateMultiProvider(_provider1);
        await multiProvider.InitializeAsync(_context);

        // Act
        await multiProvider.ShutdownAsync();

        // Try to emit an event after shutdown - it should not be processed
        await EmitEventToProvider(_provider1, CreateEvent(ProviderEventTypes.ProviderReady));
        await Task.Delay(50);

        // Assert - Should not process any events after shutdown
        var events = await ReadEvents(multiProvider.GetEventChannel(), expectedCount: 0, timeoutMs: 300);
        Assert.Empty(events);
    }

    [Fact]
    public async Task ShutdownAsync_WithProviderFailures_ThrowsAggregateException()
    {
        // Arrange
        var failingProvider = new TestProvider("failing", shutdownException: new InvalidOperationException("Shutdown failed"));
        var multiProvider = CreateMultiProvider(failingProvider, _provider2);
        await multiProvider.InitializeAsync(_context);

        // Act & Assert - Should throw AggregateException due to provider shutdown failure
        var exception = await Assert.ThrowsAsync<AggregateException>(() => multiProvider.ShutdownAsync());
        Assert.Contains("Failed to shutdown providers", exception.Message);
    }

    [Fact]
    public async Task DisposeAsync_CleansUpEventProcessing()
    {
        // Arrange
        var multiProvider = CreateMultiProvider(_provider1);
        await multiProvider.InitializeAsync(_context);

        // Act
        await multiProvider.DisposeAsync();

        // Assert - Should not throw and should handle disposal gracefully
        await Task.Delay(100); // Give time for any potential processing

        // Verify that subsequent operations on disposed provider throw
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            multiProvider.ResolveBooleanValueAsync(TestFlagKey, false));
    }

    // Helper methods
    private MultiProvider CreateMultiProvider(params FeatureProvider[] providers)
    {
        var entries = providers.Select((p, i) => new ProviderEntry(p, $"provider{i + 1}")).ToList();
        return new MultiProvider(entries, _strategy);
    }

    private static ProviderEventPayload CreateEvent(ProviderEventTypes type, ErrorType? errorType = null)
    {
        return new ProviderEventPayload
        {
            Type = type,
            ErrorType = errorType,
            Message = $"{type} event"
        };
    }

    private static async Task EmitEventToProvider(FeatureProvider provider, ProviderEventPayload eventPayload)
    {
        var eventChannel = provider.GetEventChannel();
        var eventWrapper = new Event { EventPayload = eventPayload, Provider = provider };
        await eventChannel.Writer.WriteAsync(eventWrapper);
    }

    private static async Task<List<ProviderEventPayload>> ReadEvents(Channel<object> channel, int expectedCount = 1, int timeoutMs = 1000)
    {
        var events = new List<ProviderEventPayload>();
        var cts = new CancellationTokenSource(timeoutMs);

        try
        {
            while (events.Count < expectedCount && !cts.Token.IsCancellationRequested)
            {
                if (!await channel.Reader.WaitToReadAsync(cts.Token))
                    continue;

                while (channel.Reader.TryRead(out var item) && events.Count < expectedCount)
                {
                    if (item is ProviderEventPayload payload)
                        events.Add(payload);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Timeout - return what we have
        }

        return events;
    }

    private static void AssertEvent(ProviderEventPayload eventPayload, string expectedProviderName,
        ProviderEventTypes expectedType, string? expectedMessage = null, ErrorType? errorType = null)
    {
        Assert.Equal(expectedProviderName, eventPayload.ProviderName);
        Assert.Equal(expectedType, eventPayload.Type);

        if (expectedMessage != null)
            Assert.Contains(expectedMessage, eventPayload.Message);

        if (errorType.HasValue)
            Assert.Equal(errorType.Value, eventPayload.ErrorType);
    }
}
