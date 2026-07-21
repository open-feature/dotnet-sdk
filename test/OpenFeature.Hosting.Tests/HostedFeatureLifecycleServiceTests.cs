using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace OpenFeature.Hosting.Tests;

public class HostedFeatureLifecycleServiceTests
{
    private readonly IFeatureLifecycleManager _featureLifecycleManager;

    public HostedFeatureLifecycleServiceTests()
    {
        _featureLifecycleManager = Substitute.For<IFeatureLifecycleManager>();
    }

    private HostedFeatureLifecycleService CreateSystemUnderTest(FeatureLifecycleStateOptions? options = null)
        => new(
            NullLogger<HostedFeatureLifecycleService>.Instance,
            _featureLifecycleManager,
            Options.Create(options ?? new FeatureLifecycleStateOptions()));

    [Theory]
    [InlineData(FeatureStartState.Starting)]
    [InlineData(FeatureStartState.Start)]
    [InlineData(FeatureStartState.Started)]
    public async Task GenericHostLifecycle_ShouldInitializeExactlyOnce_ForAnyStartState(FeatureStartState startState)
    {
        // Arrange
        var sut = CreateSystemUnderTest(new FeatureLifecycleStateOptions { StartState = startState });

        // Act - the generic host invokes all IHostedLifecycleService start callbacks in order.
        await sut.StartingAsync(CancellationToken.None);
        await sut.StartAsync(CancellationToken.None);
        await sut.StartedAsync(CancellationToken.None);

        // Assert
        await _featureLifecycleManager.Received(1).EnsureInitializedAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(FeatureStopState.Stopping)]
    [InlineData(FeatureStopState.Stop)]
    [InlineData(FeatureStopState.Stopped)]
    public async Task GenericHostLifecycle_ShouldShutdownExactlyOnce_ForAnyStopState(FeatureStopState stopState)
    {
        // Arrange
        var sut = CreateSystemUnderTest(new FeatureLifecycleStateOptions { StopState = stopState });

        // Act - the generic host invokes all IHostedLifecycleService callbacks in order.
        await sut.StartingAsync(CancellationToken.None);
        await sut.StartAsync(CancellationToken.None);
        await sut.StartedAsync(CancellationToken.None);
        await sut.StoppingAsync(CancellationToken.None);
        await sut.StopAsync(CancellationToken.None);
        await sut.StoppedAsync(CancellationToken.None);

        // Assert
        await _featureLifecycleManager.Received(1).ShutdownAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WebHostLifecycle_ShouldInitializeAndShutdown_WithDefaultOptions()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act - the legacy ASP.NET Core WebHost only invokes the IHostedService callbacks.
        await sut.StartAsync(CancellationToken.None);
        await sut.StopAsync(CancellationToken.None);

        // Assert
        await _featureLifecycleManager.Received(1).EnsureInitializedAsync(Arg.Any<CancellationToken>());
        await _featureLifecycleManager.Received(1).ShutdownAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(FeatureStartState.Starting)]
    [InlineData(FeatureStartState.Start)]
    [InlineData(FeatureStartState.Started)]
    public async Task WebHostLifecycle_ShouldInitializeExactlyOnce_ForAnyStartState(FeatureStartState startState)
    {
        // Arrange
        var sut = CreateSystemUnderTest(new FeatureLifecycleStateOptions { StartState = startState });

        // Act - the legacy ASP.NET Core WebHost only invokes the IHostedService callbacks.
        await sut.StartAsync(CancellationToken.None);

        // Assert
        await _featureLifecycleManager.Received(1).EnsureInitializedAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(FeatureStopState.Stopping)]
    [InlineData(FeatureStopState.Stop)]
    [InlineData(FeatureStopState.Stopped)]
    public async Task WebHostLifecycle_ShouldShutdownExactlyOnce_ForAnyStopState(FeatureStopState stopState)
    {
        // Arrange
        var sut = CreateSystemUnderTest(new FeatureLifecycleStateOptions { StopState = stopState });

        // Act - the legacy ASP.NET Core WebHost only invokes the IHostedService callbacks.
        await sut.StartAsync(CancellationToken.None);
        await sut.StopAsync(CancellationToken.None);

        // Assert
        await _featureLifecycleManager.Received(1).ShutdownAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(FeatureStartState.Starting)]
    [InlineData(FeatureStartState.Start)]
    [InlineData(FeatureStartState.Started)]
    public async Task GenericHostLifecycle_ShouldInitializeOnlyInConfiguredCallback(FeatureStartState startState)
    {
        // Arrange
        var sut = CreateSystemUnderTest(new FeatureLifecycleStateOptions { StartState = startState });

        // Act & Assert - initialization only happens once the configured callback is reached.
        await sut.StartingAsync(CancellationToken.None);
        await _featureLifecycleManager.Received(startState == FeatureStartState.Starting ? 1 : 0)
            .EnsureInitializedAsync(Arg.Any<CancellationToken>());

        await sut.StartAsync(CancellationToken.None);
        await _featureLifecycleManager.Received(startState == FeatureStartState.Started ? 0 : 1)
            .EnsureInitializedAsync(Arg.Any<CancellationToken>());

        await sut.StartedAsync(CancellationToken.None);
        await _featureLifecycleManager.Received(1).EnsureInitializedAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(FeatureStopState.Stopping)]
    [InlineData(FeatureStopState.Stop)]
    [InlineData(FeatureStopState.Stopped)]
    public async Task GenericHostLifecycle_ShouldShutdownOnlyInConfiguredCallback(FeatureStopState stopState)
    {
        // Arrange
        var sut = CreateSystemUnderTest(new FeatureLifecycleStateOptions { StopState = stopState });
        await sut.StartingAsync(CancellationToken.None);
        await sut.StartAsync(CancellationToken.None);
        await sut.StartedAsync(CancellationToken.None);

        // Act & Assert - shutdown only happens once the configured callback is reached.
        await sut.StoppingAsync(CancellationToken.None);
        await _featureLifecycleManager.Received(stopState == FeatureStopState.Stopping ? 1 : 0)
            .ShutdownAsync(Arg.Any<CancellationToken>());

        await sut.StopAsync(CancellationToken.None);
        await _featureLifecycleManager.Received(stopState == FeatureStopState.Stopped ? 0 : 1)
            .ShutdownAsync(Arg.Any<CancellationToken>());

        await sut.StoppedAsync(CancellationToken.None);
        await _featureLifecycleManager.Received(1).ShutdownAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WebHostLifecycle_ShouldShutdownExactlyOnce_WhenStartWasNeverInvoked()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act - hosts may stop services even if startup failed before StartAsync was invoked.
        await sut.StopAsync(CancellationToken.None);

        // Assert
        await _featureLifecycleManager.Received(1).ShutdownAsync(Arg.Any<CancellationToken>());
    }
}
