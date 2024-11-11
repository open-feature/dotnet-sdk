using System;

namespace OpenFeature.Tests;

public class ClearOpenFeatureInstanceFixture : IDisposable
{
    // Make sure the singleton is cleared between tests
    public void Dispose()
    {
        Api.Instance.SetContext(null);
        Api.Instance.ClearHooks();
        Api.Instance.SetProviderAsync(new NoOpFeatureProvider()).Wait();
    }
}
