using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OpenFeature.Internal;

internal sealed partial class FeatureLifecycleManager : IFeatureLifecycleManager
{
    private readonly Api _featureApi;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FeatureLifecycleManager> _logger;

    public FeatureLifecycleManager(Api featureApi, IServiceProvider serviceProvider, ILogger<FeatureLifecycleManager> logger)
    {
        _featureApi = featureApi;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        this.LogStartingInitializationOfFeatureProvider();
        var featureProvider = _serviceProvider.GetService<FeatureProvider>();
        if (featureProvider == null)
        {
            throw new InvalidOperationException("Feature provider is not registered in the service collection.");
        }
        await _featureApi.SetProviderAsync(featureProvider).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask ShutdownAsync(CancellationToken cancellationToken = default)
    {
        this.LogShuttingDownFeatureProvider();
        await _featureApi.ShutdownAsync().ConfigureAwait(false);
    }

    [LoggerMessage(200, LogLevel.Information, "Starting initialization of the feature provider")]
    partial void LogStartingInitializationOfFeatureProvider();

    [LoggerMessage(200, LogLevel.Information, "Shutting down the feature provider")]
    partial void LogShuttingDownFeatureProvider();
}
