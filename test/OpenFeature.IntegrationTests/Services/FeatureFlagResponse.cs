namespace OpenFeature.IntegrationTests.Services;

public record FeatureFlagResponse<T>(string FeatureName, T FeatureValue) where T : notnull;
