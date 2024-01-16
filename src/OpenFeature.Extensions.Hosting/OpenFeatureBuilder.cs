using Microsoft.Extensions.DependencyInjection;

namespace OpenFeature;

/// <summary>
///
/// </summary>
/// <param name="Services"></param>
public sealed record OpenFeatureBuilder(IServiceCollection Services);
