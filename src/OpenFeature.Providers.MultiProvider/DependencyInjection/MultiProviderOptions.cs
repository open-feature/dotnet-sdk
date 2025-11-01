using OpenFeature.Hosting;
using OpenFeature.Providers.MultiProvider.Models;
using OpenFeature.Providers.MultiProvider.Strategies;

namespace OpenFeature.Providers.MultiProvider.DependencyInjection;

/// <summary>
/// Options for configuring the multi-provider.
/// </summary>
public class MultiProviderOptions : OpenFeatureOptions
{
    /// <summary>
    /// Gets or sets the list of provider entries for the multi-provider.
    /// </summary>
    public List<ProviderEntry> ProviderEntries { get; set; } = [];

    /// <summary>
    /// Gets or sets the evaluation strategy to use for the multi-provider.
    /// If not set, the FirstMatchStrategy will be used by default.
    /// </summary>
    public BaseEvaluationStrategy? EvaluationStrategy { get; set; }
}
