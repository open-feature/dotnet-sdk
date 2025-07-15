namespace OpenFeature.Providers.MultiProvider.Models;

internal class RegisteredProvider
{
    internal RegisteredProvider(FeatureProvider provider, string name)
    {
        this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        this.Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    internal FeatureProvider Provider { get; }

    internal string Name { get; }

    internal Constant.ProviderStatus Status { get; private set; } = Constant.ProviderStatus.NotReady;

    internal void SetStatus(Constant.ProviderStatus status)
    {
        this.Status = status;
    }
}
