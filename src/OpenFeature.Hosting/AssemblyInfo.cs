using System.Runtime.CompilerServices;
using OpenFeature;
using OpenFeature.Hosting;
using OpenFeature.Hosting.Providers.Memory;

[assembly: TypeForwardedTo(typeof(OpenFeatureBuilder))]
[assembly: TypeForwardedTo(typeof(OpenFeatureOptions))]
[assembly: TypeForwardedTo(typeof(PolicyNameOptions))]
[assembly: TypeForwardedTo(typeof(OpenFeatureBuilderExtensions))]
[assembly: TypeForwardedTo(typeof(InMemoryProviderOptions))]
[assembly: TypeForwardedTo(typeof(FeatureBuilderExtensions))]
