namespace OpenFeature.Providers.MultiProvider.Models;

/// <summary>
/// Represents a registered provider with its unique assigned name.
/// </summary>
internal class RegisteredProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisteredProvider"/> class.
    /// </summary>
    /// <param name="provider">The feature provider instance.</param>
    /// <param name="name">The unique assigned name for the provider.</param>
    public RegisteredProvider(FeatureProvider provider, string name)
    {
        this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        this.Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Gets the feature provider instance.
    /// </summary>
    public FeatureProvider Provider { get; }

    /// <summary>
    /// Gets the unique assigned name for the provider.
    /// </summary>
    public string Name { get; }
}
