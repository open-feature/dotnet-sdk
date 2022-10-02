using OpenFeatureSDK.Model;

namespace OpenFeature.ServiceCollection;
/// <summary>
///  OpenFeature configuration options
/// </summary>
public class OpenFeatureOption
{
    /// <summary>
    /// Gets or sets the type of the feature provider must be a subtype of FeatureProvider cref="OpenFeatureSDK.FeatureProvider".
    /// </summary>
    public Type? FeatureProvider { get; set; }
    /// <summary>
    ///  Gets or sets the name  of the Open Feature Clientt.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    ///  Gets or sets the version  of the Open Feature Client.
    /// </summary>
    public string Version { get; set; }
    /// <summary>
    ///  Gets or sets the type of the hooks must be a subtype of Hook cref="OpenFeatureSDK.Hook".
    /// </summary>
    public IEnumerable<Type> Hooks { get; set; } = Enumerable.Empty<Type>();
    /// <summary>
    ///  Gets or sets the resolver function that maps a property name to a feature.
    /// </summary>
    public Func<string,string> PropertyNameResolver { get; set; } = s => s;
    /// <summary>
    ///  Gets or sets the type of EvaluationContext must be a subtype of EvaluationContext cref="OpenFeatureSDK.EvaluationContext".
    /// </summary>

    public Type? EvaluationContext { get; set; }

}
