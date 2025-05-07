using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenFeature.DependencyInjection.Internal;

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

        var options = _serviceProvider.GetRequiredService<IOptions<OpenFeatureOptions>>().Value;
        if (options.HasDefaultProvider)
        {
            var featureProvider = _serviceProvider.GetRequiredService<FeatureProvider>();
            await _featureApi.SetProviderAsync(featureProvider).ConfigureAwait(false);
        }

        foreach (var name in options.ProviderNames)
        {
            var featureProvider = _serviceProvider.GetRequiredKeyedService<FeatureProvider>(name);
            await _featureApi.SetProviderAsync(name, featureProvider).ConfigureAwait(false);
        }

        var hooks = new List<Hook>();
        foreach (var hookName in options.HookNames)
        {
            var hook = _serviceProvider.GetRequiredKeyedService<Hook>(hookName);
            hooks.Add(hook);
        }

        _featureApi.AddHooks(hooks);

        foreach (var handlerName in options.HandlerNames)
        {
            var handler = _serviceProvider.GetRequiredKeyedService<EventHandlerDelegateWrapper>(handlerName);
            _featureApi.AddHandler(handler.ProviderEventType, handler.EventHandlerDelegate);
        }
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
