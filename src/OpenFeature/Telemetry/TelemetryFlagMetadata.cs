namespace OpenFeature.Telemetry;

/**
 * Well-known flag metadata attributes for telemetry events.
 * @see https://openfeature.dev/specification/appendix-d#flag-metadata
 */
public static class TelemetryFlagMetadata
{
    /**
     * The context identifier returned in the flag metadata uniquely identifies
     * the subject of the flag evaluation. If not available, the targeting key
     * should be used.
     */
    public const string ContextId = "contextId";

    /**
     * A logical identifier for the flag set.
     */
    public const string FlagSetId = "flagSetId";

    /**
     * A version string (format unspecified) for the flag or flag set.
     */
    public const string Version = "version";
}
