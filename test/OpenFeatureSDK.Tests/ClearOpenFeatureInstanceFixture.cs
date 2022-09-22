namespace OpenFeatureSDK.Tests
{
    public class ClearOpenFeatureInstanceFixture
    {
        // Make sure the singleton is cleared between tests
        public ClearOpenFeatureInstanceFixture()
        {
            OpenFeature.Instance.SetContext(null);
            OpenFeature.Instance.ClearHooks();
            OpenFeature.Instance.SetProvider(new NoOpFeatureProvider());
        }
    }
}
