namespace OpenFeature.Telemetry;

/// <summary>
/// The attributes of an OpenTelemetry compliant event for flag evaluation.
/// <see href="https://opentelemetry.io/docs/specs/semconv/feature-flags/feature-flags-logs/"/>
/// </summary>
public static class TelemetryConstants
{
    /// <summary>
    /// The lookup key of the feature flag.
    /// </summary>
    public const string Key = "feature_flag.key";

    /// <summary>
    /// Describes a class of error the operation ended with.
    /// </summary>
    public const string ErrorCode = "error.type";

    /// <summary>
    /// A message explaining the nature of an error occurring during flag evaluation.
    /// </summary>
    public const string ErrorMessage = "error.message";

    /// <summary>
    /// A semantic identifier for an evaluated flag value.
    /// </summary>
    public const string Variant = "feature_flag.result.variant";

    /// <summary>
    /// The evaluated value of the feature flag.
    /// </summary>
    public const string Value = "feature_flag.result.value";

    /// <summary>
    /// The unique identifier for the flag evaluation context. For example, the targeting key.
    /// </summary>
    public const string ContextId = "feature_flag.context.id";

    /// <summary>
    /// The reason code which shows how a feature flag value was determined.
    /// </summary>
    public const string Reason = "feature_flag.result.reason";

    /// <summary>
    /// Describes a class of error the operation ended with.
    /// </summary>
    public const string Provider = "feature_flag.provider.name";

    /// <summary>
    /// The identifier of the flag set to which the feature flag belongs.
    /// </summary>
    public const string FlagSetId = "feature_flag.set.id";

    /// <summary>
    /// The version of the ruleset used during the evaluation. This may be any stable value which uniquely identifies the ruleset.
    /// </summary>
    public const string Version = "feature_flag.version";

}
