using OpenFeature.Providers.Memory;

namespace OpenFeature.IntegrationTests.Services;

internal interface IFeatureFlagConfigurationService
{
    Dictionary<string, Flag> GetFlags();
}
