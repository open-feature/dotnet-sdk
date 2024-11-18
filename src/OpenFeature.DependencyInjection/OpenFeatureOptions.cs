namespace OpenFeature.DependencyInjection;

/// <summary>
///  Options to configure OpenFeature
/// </summary>
public class OpenFeatureOptions
{
    private readonly HashSet<string> _providerNames = [];

    /// <summary>
    /// Determines if a default provider has been registered.
    /// </summary>
    public bool HasDefaultProvider { get; private set; }

    /// <summary>
    /// The type of the configured feature provider.
    /// </summary>
    public Type FeatureProviderType { get; protected internal set; } = null!;

    /// <summary>
    /// Gets a read-only list of registered provider names.
    /// </summary>
    public IReadOnlyCollection<string> ProviderNames => _providerNames;

    /// <summary>
    /// Registers the default provider name if no specific name is provided.
    /// Sets <see cref="HasDefaultProvider"/> to true.
    /// </summary>
    public void AddDefaultProviderName() => AddProviderName(null);

    /// <summary>
    /// Registers a new feature provider name. This operation is thread-safe.
    /// </summary>
    /// <param name="name">The name of the feature provider to register. Registers as default if null.</param>
    public void AddProviderName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            HasDefaultProvider = true;
        }
        else
        {
            lock (_providerNames)
            {
                _providerNames.Add(name!);
            }
        }
    }
}
