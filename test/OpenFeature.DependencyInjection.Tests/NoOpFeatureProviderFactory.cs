namespace OpenFeature.DependencyInjection.Tests;

#if NET8_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.Experimental(Diagnostics.FeatureCodes.NewDi)]
#endif
public class NoOpFeatureProviderFactory : IFeatureProviderFactory
{
    public FeatureProvider Create() => new NoOpFeatureProvider();
}
