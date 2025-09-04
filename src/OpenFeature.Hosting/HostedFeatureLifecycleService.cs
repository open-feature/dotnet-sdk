using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFeature.Hosting;

/// <summary>
/// A hosted service that manages the lifecycle of features within the application.
/// It ensures that features are properly initialized when the service starts
/// and gracefully shuts down when the service stops.
/// </summary>
public sealed partial class HostedFeatureLifecycleService : IHostedLifecycleService
{
    private readonly ILogger<HostedFeatureLifecycleService> _logger;
    private readonly IFeatureLifecycleManager _featureLifecycleManager;
    private readonly IOptions<FeatureLifecycleStateOptions> _featureLifecycleStateOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="HostedFeatureLifecycleService"/> class.
    /// </summary>
    /// <param name="logger">The logger used to log lifecycle events.</param>
    /// <param name="featureLifecycleManager">The feature lifecycle manager responsible for initialization and shutdown.</param>
    /// <param name="featureLifecycleStateOptions">Options that define the start and stop states of the feature lifecycle.</param>
    public HostedFeatureLifecycleService(
        ILogger<HostedFeatureLifecycleService> logger,
        IFeatureLifecycleManager featureLifecycleManager,
        IOptions<FeatureLifecycleStateOptions> featureLifecycleStateOptions)
    {
        _logger = logger;
        _featureLifecycleManager = featureLifecycleManager;
        _featureLifecycleStateOptions = featureLifecycleStateOptions;
    }

    /// <summary>
    /// Ensures that the feature is properly initialized when the service starts.
    /// </summary>
    public async Task StartingAsync(CancellationToken cancellationToken)
        => await InitializeIfStateMatchesAsync(FeatureStartState.Starting, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Ensures that the feature is in the "Start" state.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
        => await InitializeIfStateMatchesAsync(FeatureStartState.Start, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Ensures that the feature is fully started and operational.
    /// </summary>
    public async Task StartedAsync(CancellationToken cancellationToken)
        => await InitializeIfStateMatchesAsync(FeatureStartState.Started, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gracefully shuts down the feature when the service is stopping.
    /// </summary>
    public async Task StoppingAsync(CancellationToken cancellationToken)
        => await ShutdownIfStateMatchesAsync(FeatureStopState.Stopping, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Ensures that the feature is in the "Stop" state.
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken)
        => await ShutdownIfStateMatchesAsync(FeatureStopState.Stop, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Ensures that the feature is fully stopped and no longer operational.
    /// </summary>
    public async Task StoppedAsync(CancellationToken cancellationToken)
        => await ShutdownIfStateMatchesAsync(FeatureStopState.Stopped, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Initializes the feature lifecycle if the current state matches the expected start state.
    /// </summary>
    private async Task InitializeIfStateMatchesAsync(FeatureStartState expectedState, CancellationToken cancellationToken)
    {
        if (_featureLifecycleStateOptions.Value.StartState == expectedState)
        {
            this.LogInitializingFeatureLifecycleManager(expectedState);
            await _featureLifecycleManager.EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Shuts down the feature lifecycle if the current state matches the expected stop state.
    /// </summary>
    private async Task ShutdownIfStateMatchesAsync(FeatureStopState expectedState, CancellationToken cancellationToken)
    {
        if (_featureLifecycleStateOptions.Value.StopState == expectedState)
        {
            this.LogShuttingDownFeatureLifecycleManager(expectedState);
            await _featureLifecycleManager.ShutdownAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    [LoggerMessage(200, LogLevel.Information, "Initializing the Feature Lifecycle Manager for state {State}.")]
    partial void LogInitializingFeatureLifecycleManager(FeatureStartState state);

    [LoggerMessage(200, LogLevel.Information, "Shutting down the Feature Lifecycle Manager for state {State}")]
    partial void LogShuttingDownFeatureLifecycleManager(FeatureStopState state);
}
