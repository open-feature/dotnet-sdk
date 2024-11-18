namespace OpenFeature;

/// <summary>
/// Defines the various states for starting a feature.
/// </summary>
public enum FeatureStartState
{
    /// <summary>
    /// The feature is in the process of starting.
    /// </summary>
    Starting,

    /// <summary>
    /// The feature is at the start state.
    /// </summary>
    Start,

    /// <summary>
    /// The feature has fully started.
    /// </summary>
    Started
}
