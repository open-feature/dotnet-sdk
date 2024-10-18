using Microsoft.Extensions.DependencyInjection;

namespace OpenFeature;

/// <summary>
/// Describes a <see cref="OpenFeatureBuilder"/> backed by an <see cref="IServiceCollection"/>.
/// </summary>
/// <param name="Services">The <see cref="IServiceCollection"/> instance.</param>
public sealed record OpenFeatureBuilder(IServiceCollection Services)
{
    /// <summary>
    /// Indicates whether the evaluation context has been configured.
    /// This property is used to determine if specific configurations or services
    /// should be initialized based on the presence of an evaluation context.
    /// </summary>
    internal bool IsContextConfigured { get; set; }
}
