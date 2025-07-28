namespace OpenFeature.Providers.MultiProvider.Models;

internal class ChildProviderStatus
{
    public string ProviderName { get; set; } = string.Empty;
    public Exception? Error { get; set; }
}
