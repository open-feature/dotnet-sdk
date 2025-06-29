namespace OpenFeature.Providers.MultiProvider.Strategies.Models;

/// <summary>
/// Represents an error encountered during the resolution process.
/// Contains the name of the provider that encountered the error and the error details.
/// </summary>
public class ProviderError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderError"/> class.
    /// </summary>
    /// <param name="providerName">The name of the provider that encountered the error.</param>
    /// <param name="error">The error details.</param>
    public ProviderError(string providerName, object? error)
    {
        this.ProviderName = providerName;
        this.Error = error;
    }

    /// <summary>
    /// Gets or sets the name of the provider that encountered the error.
    /// </summary>
    public string ProviderName { get; private set; }

    /// <summary>
    /// Gets or sets the error details.
    /// </summary>
    public object? Error { get; private set; }
}
