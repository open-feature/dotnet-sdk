namespace OpenFeature.Tests;

public class ClearOpenFeatureInstanceFixture : IAsyncLifetime
{
    public ValueTask InitializeAsync()
    {
        Api.ResetApi();

        return ValueTask.CompletedTask;
    }

    // Make sure the singleton is cleared between tests
    public async ValueTask DisposeAsync()
    {
        await Api.Instance.ShutdownAsync().ConfigureAwait(false);
    }
}
