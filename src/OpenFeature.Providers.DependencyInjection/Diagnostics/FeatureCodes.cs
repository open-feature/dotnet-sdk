namespace OpenFeature.DependencyInjection.Diagnostics;

/// <summary>
/// Contains identifiers for experimental features and diagnostics in the OpenFeature framework.
/// </summary>
/// <remarks>
/// <c>Experimental</c> - This class includes identifiers that allow developers to track and conditionally enable
/// experimental features. Each identifier follows a structured code format to indicate the feature domain,
/// maturity level, and unique identifier. Note that experimental features are subject to change or removal 
/// in future releases.
/// <para>
/// <strong>Basic Information</strong><br/>
/// These identifiers conform to OpenFeature’s Diagnostics Specifications, allowing developers to recognize 
/// and manage experimental features effectively.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Code Structure:
///     - "OF" - Represents the OpenFeature library.
///     - "DI" - Indicates the Dependency Injection domain.
///     - "001" - Unique identifier for a specific feature.
/// </code>
/// </example>
internal static class FeatureCodes
{
    /// <summary>
    /// Identifier for the experimental Dependency Injection features within the OpenFeature framework.
    /// </summary>
    /// <remarks>
    /// <c>OFDI001</c> identifier marks experimental features in the Dependency Injection (DI) domain.
    /// 
    /// Usage:
    /// Developers can use this identifier to conditionally enable or test experimental DI features.
    /// It is part of the OpenFeature diagnostics system to help track experimental functionality.
    /// </remarks>
    public const string NewDi = "OFDI001";
}
