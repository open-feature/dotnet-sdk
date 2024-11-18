using OpenFeature.Providers.Memory;

namespace OpenFeature.DependencyInjection.Providers.Memory;

/// <summary>
/// A factory for creating instances of <see cref="InMemoryProvider"/>, 
/// an in-memory implementation of <see cref="FeatureProvider"/>. 
/// This factory allows for the customization of feature flags to facilitate 
/// testing and lightweight feature flag management without external dependencies.
/// </summary>
#if NET8_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.Experimental(Diagnostics.FeatureCodes.NewDi)]
#endif
public class InMemoryProviderFactory : IFeatureProviderFactory
{
    /// <summary>
    /// Gets or sets the collection of feature flags used to configure the 
    /// <see cref="InMemoryProvider"/> instances. This dictionary maps 
    /// flag names to <see cref="Flag"/> instances, enabling pre-configuration 
    /// of features for testing or in-memory evaluation.
    /// </summary>
    internal IDictionary<string, Flag>? Flags { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="InMemoryProvider"/> with the specified 
    /// flags set in <see cref="Flags"/>. This instance is configured for in-memory 
    /// feature flag management, suitable for testing or lightweight feature toggling scenarios.
    /// </summary>
    /// <returns>
    /// A configured <see cref="InMemoryProvider"/> that can be used to manage 
    /// feature flags in an in-memory context.
    /// </returns>
    public FeatureProvider Create() => new InMemoryProvider(Flags);
}
