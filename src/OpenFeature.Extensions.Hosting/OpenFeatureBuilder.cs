using Microsoft.Extensions.DependencyInjection;

namespace OpenFeature;

/// <summary>
/// Describes a <see cref="OpenFeatureBuilder"/> backed by an <see cref="IServiceCollection"/>.
/// </summary>
/// <param name="Services"><see cref="IServiceCollection"/></param>
public sealed record OpenFeatureBuilder(IServiceCollection Services);
