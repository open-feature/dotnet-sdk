namespace OpenFeature.Providers.MultiProvider.Strategies.Models;

/// <summary>
/// Evaluation context specific to strategy evaluation containing flag-related information.
/// </summary>
public class StrategyEvaluationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StrategyEvaluationContext"/> class.
    /// </summary>
    /// <param name="flagKey">The feature flag key being evaluated.</param>
    /// <param name="flagType">The type of the flag value being evaluated.</param>
    public StrategyEvaluationContext(string flagKey, Type flagType)
    {
        FlagKey = flagKey;
        FlagType = flagType;
    }

    /// <summary>
    /// The feature flag key being evaluated.
    /// </summary>
    public string FlagKey { get; private set; }

    /// <summary>
    /// The type of the flag value being evaluated.
    /// </summary>
    public Type FlagType { get; private set; }
}
