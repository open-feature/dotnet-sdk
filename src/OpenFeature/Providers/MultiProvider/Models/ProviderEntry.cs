namespace OpenFeature.Providers.MultiProvider.Models;

/// <summary>
/// Represents an entry for a provider in the multi-provider configuration.
/// </summary>
public class ProviderEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderEntry"/> class.
    /// </summary>
    /// <param name="provider">The feature provider instance.</param>
    /// <param name="name">Optional custom name for the provider. If not provided, the provider's metadata name will be used.</param>
    public ProviderEntry(FeatureProvider provider, string? name = null)
    {
        this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        this.Name = name;
    }

    /// <summary>
    /// Gets the feature provider instance.
    /// </summary>
    public FeatureProvider Provider { get; }

    /// <summary>
    /// Gets the optional custom name for the provider.
    /// If null, the provider's metadata name should be used.
    /// </summary>
    public string? Name { get; }
}
