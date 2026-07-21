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

    // Hosts that support IHostedLifecycleService (e.g. the .NET 8+ generic host) always invoke
    // StartingAsync before StartAsync. If StartingAsync was never invoked, the host only supports
    // IHostedService (e.g. the legacy ASP.NET Core WebHost), and we fall back to StartAsync/StopAsync.
    private bool _lifecycleCallbacksSupported;

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
    {
        _lifecycleCallbacksSupported = true;
        await InitializeIfStateMatchesAsync(FeatureStartState.Starting, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Ensures that the feature is in the "Start" state.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_lifecycleCallbacksSupported && _featureLifecycleStateOptions.Value.StartState != FeatureStartState.Start)
        {
            this.LogLifecycleCallbacksNotSupportedFallback(nameof(StartAsync));
            await InitializeIfStateMatchesAsync(_featureLifecycleStateOptions.Value.StartState, cancellationToken).ConfigureAwait(false);
            return;
        }

        await InitializeIfStateMatchesAsync(FeatureStartState.Start, cancellationToken).ConfigureAwait(false);
    }

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
    {
        if (!_lifecycleCallbacksSupported && _featureLifecycleStateOptions.Value.StopState != FeatureStopState.Stop)
        {
            this.LogLifecycleCallbacksNotSupportedFallback(nameof(StopAsync));
            await ShutdownIfStateMatchesAsync(_featureLifecycleStateOptions.Value.StopState, cancellationToken).ConfigureAwait(false);
            return;
        }

        await ShutdownIfStateMatchesAsync(FeatureStopState.Stop, cancellationToken).ConfigureAwait(false);
    }

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

    [LoggerMessage(201, LogLevel.Information, "The host does not support IHostedLifecycleService callbacks. Falling back to {Method} to manage the feature lifecycle.")]
    partial void LogLifecycleCallbacksNotSupportedFallback(string method);
}
