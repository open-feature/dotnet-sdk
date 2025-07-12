using OpenFeature.Constant;

namespace OpenFeature.Providers.MultiProvider.Strategies.Models;

/// <summary>
/// Per-provider context containing provider-specific information for strategy evaluation.
/// </summary>
public class StrategyPerProviderContext : StrategyEvaluationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StrategyPerProviderContext"/> class.
    /// </summary>
    /// <param name="provider">The feature provider instance.</param>
    /// <param name="providerName">The name/identifier of the provider.</param>
    /// <param name="providerStatus">The current status of the provider.</param>
    /// <param name="key">The feature flag key being evaluated.</param>
    /// <param name="flagType">The type of the flag value being evaluated.</param>
    public StrategyPerProviderContext(FeatureProvider provider, string providerName, ProviderStatus providerStatus, string key, Type flagType)
        : base(key, flagType)
    {
        this.Provider = provider;
        this.ProviderName = providerName;
        this.ProviderStatus = providerStatus;
    }

    /// <summary>
    /// The feature provider instance.
    /// </summary>
    public FeatureProvider Provider { get; }

    /// <summary>
    /// The name/identifier of the provider.
    /// </summary>
    public string ProviderName { get; }

    /// <summary>
    /// The current status of the provider.
    /// </summary>
    public ProviderStatus ProviderStatus { get; }
}
