using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OpenFeature.Internal;

internal sealed class FeatureLifecycleManager : IFeatureLifecycleManager
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
        _logger.LogInformation("Starting initialization of the feature provider");
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
        _logger.LogInformation("Shutting down the feature provider.");
        await _featureApi.ShutdownAsync().ConfigureAwait(false);
    }
}
