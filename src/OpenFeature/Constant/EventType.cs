namespace OpenFeature.Constant;

/// <summary>
/// The ProviderEventTypes enum represents the available event types of a provider.
/// </summary>
public enum ProviderEventTypes
{
    /// <summary>
    /// ProviderReady should be emitted by a provider upon completing its initialisation.
    /// </summary>
    ProviderReady,
    /// <summary>
    /// ProviderError should be emitted by a provider upon encountering an error.
    /// </summary>
    ProviderError,
    /// <summary>
    /// ProviderConfigurationChanged should be emitted by a provider when a flag configuration has been changed.
    /// </summary>
    ProviderConfigurationChanged,
    /// <summary>
    /// ProviderStale should be emitted by a provider when it goes into the stale state.
    /// </summary>
    ProviderStale
}
