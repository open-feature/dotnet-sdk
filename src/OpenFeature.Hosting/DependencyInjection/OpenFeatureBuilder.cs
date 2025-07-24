using Microsoft.Extensions.DependencyInjection;

namespace OpenFeature.DependencyInjection;

/// <summary>
/// Describes a <see cref="OpenFeatureBuilder"/> backed by an <see cref="IServiceCollection"/>.
/// </summary>
/// <param name="services">The services being configured.</param>
public class OpenFeatureBuilder(IServiceCollection services)
{
    /// <summary> The services being configured. </summary>
    public IServiceCollection Services { get; } = services;

    /// <summary>
    /// Indicates whether the evaluation context has been configured.
    /// This property is used to determine if specific configurations or services
    /// should be initialized based on the presence of an evaluation context.
    /// </summary>
    public bool IsContextConfigured { get; internal set; }

    /// <summary>
    /// Indicates whether the policy has been configured.
    /// </summary>
    public bool IsPolicyConfigured { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether a default provider has been registered.
    /// </summary>
    public bool HasDefaultProvider { get; internal set; }

    /// <summary>
    /// Gets the count of domain-bound providers that have been registered.
    /// This count does not include the default provider.
    /// </summary>
    public int DomainBoundProviderRegistrationCount { get; internal set; }

    /// <summary>
    /// Validates the current configuration, ensuring that a policy is set when multiple providers are registered
    /// or when a default provider is registered alongside another provider.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if multiple providers are registered without a policy, or if both a default provider 
    /// and an additional provider are registered without a policy configuration.
    /// </exception>
    public void Validate()
    {
        if (!IsPolicyConfigured)
        {
            if (DomainBoundProviderRegistrationCount > 1)
            {
                throw new InvalidOperationException("Multiple providers have been registered, but no policy has been configured.");
            }

            if (HasDefaultProvider && DomainBoundProviderRegistrationCount == 1)
            {
                throw new InvalidOperationException("A default provider and an additional provider have been registered without a policy configuration.");
            }
        }
    }
}
