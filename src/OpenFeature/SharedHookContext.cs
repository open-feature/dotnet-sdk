using System;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature;

/// <summary>
/// Component of the hook context which shared between all hook instances
/// </summary>
/// <param name="flagKey">Feature flag key</param>
/// <param name="defaultValue">Default value</param>
/// <param name="flagValueType">Flag value type</param>
/// <param name="clientMetadata">Client metadata</param>
/// <param name="providerMetadata">Provider metadata</param>
/// <typeparam name="T">Flag value type</typeparam>
internal class SharedHookContext<T>(
    string? flagKey,
    T defaultValue,
    FlagValueType flagValueType,
    ClientMetadata? clientMetadata,
    Metadata? providerMetadata)
{
    /// <summary>
    /// Feature flag being evaluated
    /// </summary>
    public string FlagKey { get; } = flagKey ?? throw new ArgumentNullException(nameof(flagKey));

    /// <summary>
    /// Default value if flag fails to be evaluated
    /// </summary>
    public T DefaultValue { get; } = defaultValue;

    /// <summary>
    /// The value type of the flag
    /// </summary>
    public FlagValueType FlagValueType { get; } = flagValueType;

    /// <summary>
    /// Client metadata
    /// </summary>
    public ClientMetadata ClientMetadata { get; } =
        clientMetadata ?? throw new ArgumentNullException(nameof(clientMetadata));

    /// <summary>
    /// Provider metadata
    /// </summary>
    public Metadata ProviderMetadata { get; } =
        providerMetadata ?? throw new ArgumentNullException(nameof(providerMetadata));

    /// <summary>
    /// Create a hook context from this shared context.
    /// </summary>
    /// <param name="evaluationContext">Evaluation context</param>
    /// <returns>A hook context</returns>
    public HookContext<T> ToHookContext(EvaluationContext? evaluationContext)
    {
        return new HookContext<T>(this, evaluationContext, new HookData());
    }
}
