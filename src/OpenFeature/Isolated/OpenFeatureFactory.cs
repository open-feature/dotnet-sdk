namespace OpenFeature.Isolated;

/// <summary>
/// Factory for creating isolated instances of the OpenFeature API.
/// </summary>
/// <remarks>
/// This factory provides a method to create new, independent instances of the OpenFeature API with fully isolated state. Each isolated instance maintains its own providers, evaluation context, hooks, event handlers,
/// and transaction context propagators. It does not share state with the global <see cref="Api.Instance"/> singleton or with any other isolated instance.
/// </remarks>
public static class OpenFeatureFactory
{
    /// <summary>
    /// Creates a new, independent instance of the OpenFeature API with fully isolated state.
    /// <para>
    /// Each isolated instance maintains its own providers, evaluation context, hooks, event handlers,
    /// and transaction context propagators. It does not share state with the global <see cref="Api.Instance"/>
    /// singleton or with any other isolated instance.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// A single provider instance should not be bound to more than one API instance at a time.
    /// Attempting to do so will result in an <see cref="InvalidOperationException"/>.
    /// </para>
    /// </remarks>
    /// <returns>A new, independent <see cref="Api"/> instance.</returns>
    /// <seealso href="https://openfeature.dev/specification/sections/flag-evaluation#18-isolated-api-instances">Specification 1.8 - Isolated API Instances</seealso>
    public static Api CreateIsolated() => new Api();
}
