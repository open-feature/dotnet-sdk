using System.Threading.Tasks;
using Xunit;
using System.Threading;
using Xunit;

namespace OpenFeature.Tests;

public class ClearOpenFeatureInstanceFixture : IAsyncLifetime
{
    protected readonly CancellationToken TestCancellationToken;

    protected ClearOpenFeatureInstanceFixture()
    {
        this.TestCancellationToken = TestContext.Current.CancellationToken;
    }

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
