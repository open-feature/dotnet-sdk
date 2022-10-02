using OpenFeatureSDK.Model;

namespace OpenFeature.ServiceCollection.Feature;

/// <summary>
/// Used to retrieve configured <typeparamref name="T"/>
/// </summary>
/// <typeparam name="T">The type of flags being requested.</typeparam>
public interface IFeatures<T> where T : new()
{
    /// <summary>
    /// resolve features values and bind it to new object of type T
    /// </summary>
    /// <returns>instance of T that contains  feature values</returns>
    Task<T> GetValueAsync();
    /// <summary>
    /// Resolves a TPropertyType feature flag details
    /// </summary>
    /// <param name="name">property name</param>
    /// <typeparam name="TPropertyType">property type</typeparam>
    /// <returns>Resolved flag details <see cref="FlagEvaluationDetails{T}"/></returns>
    Task<FlagEvaluationDetails<TPropertyType>?> GetDetailsAsync<TPropertyType>(string name);
}
