namespace OpenFeature.Providers.MultiProvider.Models;

internal class ProviderStatus
{
    public string ProviderName { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
}
