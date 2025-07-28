namespace OpenFeature.Providers.MultiProvider.Models;

internal class RegisteredProvider
{
#if NET9_0_OR_GREATER
    private readonly Lock _statusLock = new();
#else
    private readonly object _statusLock = new object();
#endif

    private volatile Constant.ProviderStatus _status = Constant.ProviderStatus.NotReady;

    internal RegisteredProvider(FeatureProvider provider, string name)
    {
        this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        this.Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    internal FeatureProvider Provider { get; }

    internal string Name { get; }

    internal Constant.ProviderStatus Status
    {
        get
        {
            lock (this._statusLock)
            {
                return this._status;
            }
        }
    }

    internal void SetStatus(Constant.ProviderStatus status)
    {
        lock (this._statusLock)
        {
            this._status = status;
        }
    }
}
