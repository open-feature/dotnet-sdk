namespace OpenFeature.Providers.MultiProvider.Strategies.Models;

/// <summary>
/// Evaluation context specific to strategy evaluation containing flag-related information.
/// </summary>
/// <typeparam name="T">The type of the flag value being evaluated.</typeparam>
public class StrategyEvaluationContext<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StrategyEvaluationContext{T}"/> class.
    /// </summary>
    /// <param name="flagKey">The feature flag key being evaluated.</param>
    public StrategyEvaluationContext(string flagKey)
    {
        this.FlagKey = flagKey;
    }

    /// <summary>
    /// The feature flag key being evaluated.
    /// </summary>
    public string FlagKey { get; private set; }
}
