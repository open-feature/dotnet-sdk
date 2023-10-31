namespace OpenFeature.Tests
{
    public class ClearOpenFeatureInstanceFixture
    {
        // Make sure the singleton is cleared between tests
        public ClearOpenFeatureInstanceFixture()
        {
            Api.Instance.SetContext(null);
            Api.Instance.ClearHooks();
            Api.Instance.SetProvider(new NoOpFeatureProvider()).Wait();
        }
    }
}
