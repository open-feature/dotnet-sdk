namespace OpenFeature.Telemetry;

/// <summary>
/// Well-known flag metadata attributes for telemetry events.
/// <remarks>See also: https://openfeature.dev/specification/appendix-d#flag-metadata</remarks>
/// </summary>
public static class TelemetryFlagMetadata
{
    /// <summary>
    /// The context identifier returned in the flag metadata uniquely identifies
    /// the subject of the flag evaluation. If not available, the targeting key
    /// should be used.
    /// </summary>
    public const string ContextId = "contextId";

    /// <summary>
    /// ///A logical identifier for the flag set.
    /// </summary>
    public const string FlagSetId = "flagSetId";

    /// <summary>
    /// A version string (format unspecified) for the flag or flag set.
    /// </summary>
    public const string Version = "version";

    /// <summary>
    /// The evaluated value of the feature flag.
    /// </summary>
    public const string Value = "value";
}
