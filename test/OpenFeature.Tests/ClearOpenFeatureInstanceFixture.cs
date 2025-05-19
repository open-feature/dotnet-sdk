using Xunit;

namespace OpenFeature.Tests;

public class ClearOpenFeatureInstanceFixture : IAsyncLifetime
{
    public Task InitializeAsync()
    {
        Api.ResetApi();

        return Task.CompletedTask;
    }

    // Make sure the singleton is cleared between tests
    public async Task DisposeAsync()
    {
        await Api.Instance.ShutdownAsync().ConfigureAwait(false);
    }
}
