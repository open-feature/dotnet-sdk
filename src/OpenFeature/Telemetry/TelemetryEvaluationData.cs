namespace OpenFeature.Telemetry;

/**
* Event data, sometimes referred to as "body", is specific to a specific event.
* In this case, the event is `feature_flag.evaluation`. That's why the prefix
* is omitted from the values.
* @see https://opentelemetry.io/docs/specs/semconv/feature-flags/feature-flags-logs/
*/
public static class TelemetryEvaluationData
{
    /**
 * The evaluated value of the feature flag.
 *
 * - type: `undefined`
 * - requirement level: `conditionally required`
 * - condition: variant is not defined on the evaluation details
 * - example: `#ff0000`; `1`; `true`
 */
    public const string Value = "value";
}
