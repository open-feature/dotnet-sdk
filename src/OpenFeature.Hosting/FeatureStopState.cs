namespace OpenFeature;

/// <summary>
/// Defines the various states for stopping a feature.
/// </summary>
public enum FeatureStopState
{
    /// <summary>
    /// The feature is in the process of stopping.
    /// </summary>
    Stopping,

    /// <summary>
    /// The feature is at the stop state.
    /// </summary>
    Stop,

    /// <summary>
    /// The feature has fully stopped.
    /// </summary>
    Stopped
}
