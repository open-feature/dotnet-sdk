using System.Threading.Tasks;
using System.Threading;
using Xunit;

namespace OpenFeature.Tests;

public class ClearOpenFeatureInstanceFixture : IAsyncLifetime
{
    protected CancellationToken TestCancellationToken;

    public ValueTask InitializeAsync()
    {
        this.TestCancellationToken = TestContext.Current.CancellationToken;
        Api.ResetApi();

        return ValueTask.CompletedTask;
    }

    // Make sure the singleton is cleared between tests
    public async ValueTask DisposeAsync()
    {
        await Api.Instance.ShutdownAsync().ConfigureAwait(false);
    }
}
