using OpenFeature.Model;

namespace OpenFeature.Providers.MultiProvider.Strategies.Models;

/// <summary>
/// Represents the final result of a feature flag resolution operation from a multi-provider strategy.
/// Contains the resolved details, the provider that successfully resolved the flag, and any errors encountered during the resolution process.
/// </summary>
public class FinalResult<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FinalResult{T}"/> class.
    /// </summary>
    /// <param name="details">The resolution details containing the resolved value and associated metadata.</param>
    /// <param name="provider">The provider that successfully resolved the feature flag.</param>
    /// <param name="providerName">The name of the provider that successfully resolved the feature flag.</param>
    /// <param name="errors">The list of errors encountered during the resolution process.</param>
    public FinalResult(ResolutionDetails<T> details, FeatureProvider provider, string providerName, List<ProviderError>? errors)
    {
        this.Details = details;
        this.Provider = provider;
        this.ProviderName = providerName;
        this.Errors = errors ?? [];
    }

    /// <summary>
    /// Gets or sets the resolution details containing the resolved value and associated metadata.
    /// </summary>
    public ResolutionDetails<T> Details { get; private set; }

    /// <summary>
    /// Gets or sets the provider that successfully resolved the feature flag.
    /// </summary>
    public FeatureProvider Provider { get; private set; }

    /// <summary>
    /// Gets or sets the name of the provider that successfully resolved the feature flag.
    /// </summary>
    public string ProviderName { get; private set; }

    /// <summary>
    /// Gets or sets the list of errors encountered during the resolution process.
    /// </summary>
    public List<ProviderError> Errors { get; private set; }
}
