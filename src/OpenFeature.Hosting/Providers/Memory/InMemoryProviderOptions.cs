using OpenFeature.Providers.Memory;

namespace OpenFeature.Hosting.Providers.Memory;

/// <summary>
/// Options for configuring the in-memory feature flag provider.
/// </summary>
public class InMemoryProviderOptions : OpenFeatureOptions
{
    /// <summary>
    /// Gets or sets the feature flags to be used by the in-memory provider.
    /// </summary>
    /// <remarks>
    /// This property allows you to specify a dictionary of flags where the key is the flag name 
    /// and the value is the corresponding <see cref="Flag"/> instance.
    /// If no flags are provided, the in-memory provider will start with an empty set of flags.
    /// </remarks>
    public IDictionary<string, Flag>? Flags { get; set; }
}
