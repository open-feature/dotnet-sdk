using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace OpenFeature.Internal;

internal sealed class OpenFeatureHostedService(Api api, IEnumerable<FeatureProvider> providers) : IHostedLifecycleService
{
    readonly Api _api = Check.NotNull(api);
    readonly IEnumerable<FeatureProvider> _providers = Check.NotNull(providers);

    async Task IHostedLifecycleService.StartingAsync(CancellationToken cancellationToken)
    {
        foreach (var provider in this._providers)
        {
            await this._api.SetProviderAsync(provider.GetMetadata()?.Name ?? string.Empty, provider).ConfigureAwait(false);

            if (this._api.GetProviderMetadata() is { Name: "No-op Provider" })
                await this._api.SetProviderAsync(provider).ConfigureAwait(false);
        }
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    Task IHostedLifecycleService.StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    Task IHostedLifecycleService.StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    Task IHostedService.StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    Task IHostedLifecycleService.StoppedAsync(CancellationToken cancellationToken) => this._api.ShutdownAsync();
}
