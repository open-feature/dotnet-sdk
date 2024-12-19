using System;
using System.Threading;
using Xunit;

namespace OpenFeature.Tests;

public class ClearOpenFeatureInstanceFixture : IDisposable
{
    protected readonly CancellationToken TestCancellationToken;

    protected ClearOpenFeatureInstanceFixture()
    {
        this.TestCancellationToken = TestContext.Current.CancellationToken;
    }

    // Make sure the singleton is cleared between tests
    public void Dispose()
    {
        Api.Instance.SetContext(null);
        Api.Instance.ClearHooks();
        Api.Instance.SetProviderAsync(new NoOpFeatureProvider()).Wait();
    }
}
