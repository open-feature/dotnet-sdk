using System.Collections.ObjectModel;

namespace OpenFeature.DependencyInjection.Abstractions;

/// <summary>
/// Provider-focused options for configuring OpenFeature integrations.
/// Contains only contracts and metadata that integrations may need.
/// </summary>
public class OpenFeatureProviderOptions
{
    private readonly HashSet<string> _providerNames = [];

    /// <summary>
    /// Determines if a default provider has been registered.
    /// </summary>
    public bool HasDefaultProvider { get; private set; }

    /// <summary>
    /// The <see cref="Type"/> of the configured feature provider, if any.
    /// Typically set by higher-level configuration.
    /// </summary>
    public Type FeatureProviderType { get; protected internal set; } = null!;

    /// <summary>
    /// Gets a read-only list of registered provider names.
    /// </summary>
    public IReadOnlyCollection<string> ProviderNames
    {
        get
        {
            lock (_providerNames)
            {
                return new ReadOnlyCollection<string>([.. _providerNames]);
            }
        }
    }

    /// <summary>
    /// Registers the default provider name if no specific name is provided.
    /// Sets <see cref="HasDefaultProvider"/> to true.
    /// </summary>
    internal void AddDefaultProviderName() => AddProviderName(null);

    /// <summary>
    /// Registers a new feature provider name. This operation is thread-safe.
    /// </summary>
    /// <param name="name">The name of the feature provider to register. Registers as default if null.</param>
    internal void AddProviderName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            HasDefaultProvider = true;
            return;
        }

        lock (_providerNames)
        {
            _providerNames.Add(name!);
        }
    }
}
