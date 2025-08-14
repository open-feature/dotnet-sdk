namespace OpenFeature.Hosting;

/// <summary>
/// Defines the contract for managing the lifecycle of a feature api.
/// </summary>
public interface IFeatureLifecycleManager
{
    /// <summary>
    /// Ensures that the feature provider is properly initialized and ready to be used.
    /// This method should handle all necessary checks, configuration, and setup required to prepare the feature provider.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A Task representing the asynchronous operation of initializing the feature provider.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the feature provider is not registered or is in an invalid state.</exception>
    ValueTask EnsureInitializedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gracefully shuts down the feature api, ensuring all resources are properly disposed of and any persistent state is saved.
    /// This method should handle all necessary cleanup and shutdown operations for the feature provider.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A Task representing the asynchronous operation of shutting down the feature provider.</returns>
    ValueTask ShutdownAsync(CancellationToken cancellationToken = default);
}
