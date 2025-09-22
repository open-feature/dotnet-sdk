namespace OpenFeature.Providers.MultiProvider.Strategies.Models;

/// <summary>
/// Specifies how providers should be evaluated.
/// </summary>
public enum RunMode
{
    /// <summary>
    /// Providers are evaluated one after another in sequence.
    /// </summary>
    Sequential,

    /// <summary>
    /// Providers are evaluated concurrently in parallel.
    /// </summary>
    Parallel
}
