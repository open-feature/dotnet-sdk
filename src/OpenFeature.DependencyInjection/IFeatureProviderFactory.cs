namespace OpenFeature.DependencyInjection;

/// <summary>
/// Provides a contract for creating instances of <see cref="FeatureProvider"/>.
/// This factory interface enables custom configuration and initialization of feature providers 
/// to support domain-specific or application-specific feature flag management.
/// </summary>
#if NET8_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.Experimental(Diagnostics.FeatureCodes.NewDi)]
#endif
public interface IFeatureProviderFactory
{
    /// <summary>
    /// Creates an instance of a <see cref="FeatureProvider"/> configured according to 
    /// the specific settings implemented by the concrete factory.
    /// </summary>
    /// <returns>
    /// A new instance of <see cref="FeatureProvider"/>. 
    /// The configuration and behavior of this provider instance are determined by 
    /// the implementation of this method.
    /// </returns>
    FeatureProvider Create();
}
