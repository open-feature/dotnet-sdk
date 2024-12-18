using System.Threading.Tasks;
using Xunit;

namespace OpenFeature.Tests;

public class ClearOpenFeatureInstanceFixture : IAsyncLifetime
{
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    // Make sure the singleton is cleared between tests
    public async Task DisposeAsync()
    {
        await Api.Instance.ShutdownAsync().ConfigureAwait(false);
        // Api.Instance.SetContext(null);
        // Api.Instance.ClearHooks();
        // Api.Instance.SetProviderAsync(new NoOpFeatureProvider()).Wait();
    }
}
