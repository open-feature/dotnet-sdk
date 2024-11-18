namespace OpenFeature;

/// <summary>
/// Represents the lifecycle state options for a feature, 
/// defining the states during the start and stop lifecycle.
/// </summary>
public class FeatureLifecycleStateOptions
{
    /// <summary>
    /// Gets or sets the state during the feature startup lifecycle.
    /// </summary>
    public FeatureStartState StartState { get; set; } = FeatureStartState.Starting;

    /// <summary>
    /// Gets or sets the state during the feature shutdown lifecycle.
    /// </summary>
    public FeatureStopState StopState { get; set; } = FeatureStopState.Stopping;
}
