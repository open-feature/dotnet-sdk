using OpenFeature.Model;

namespace OpenFeature.Providers.MultiProvider.Strategies.Models;

/// <summary>
/// Base class for provider resolution results.
/// </summary>
public class ProviderResolutionResult<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderResolutionResult{T}"/> class
    /// with the specified provider and resolution details.
    /// </summary>
    /// <param name="provider">The feature provider that produced this result.</param>
    /// <param name="providerName">The name of the provider that produced this result.</param>
    /// <param name="resolutionDetails">The resolution details.</param>
    public ProviderResolutionResult(FeatureProvider provider, string providerName, ResolutionDetails<T> resolutionDetails)
    {
        Provider = provider;
        ProviderName = providerName;
        ResolutionDetails = resolutionDetails;
    }

    /// <summary>
    /// The feature provider that produced this result.
    /// </summary>
    public FeatureProvider Provider { get; set; }

    /// <summary>
    /// The resolution details.
    /// </summary>
    public ResolutionDetails<T> ResolutionDetails { get; set; }

    /// <summary>
    /// The name of the provider that produced this result.
    /// </summary>
    public string ProviderName { get; set; }
}
