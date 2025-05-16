using System.Collections.Generic;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature.Telemetry;

/// <summary>
/// Class for creating evaluation events for feature flags.
/// </summary>
public static class EvaluationEventBuilder
{
    private const string EventName = "feature_flag.evaluation";

    /// <summary>
    /// Creates an evaluation event based on the provided hook context and flag evaluation details.
    /// </summary>
    /// <param name="hookContext">The context of the hook containing flag key and provider metadata.</param>
    /// <param name="details">The details of the flag evaluation including reason, variant, and metadata.</param>
    /// <returns>An instance of <see cref="EvaluationEvent"/> containing the event name, attributes, and body.</returns>
    public static EvaluationEvent Build(HookContext<Value> hookContext, FlagEvaluationDetails<Value> details)
    {
        var attributes = new Dictionary<string, object?>
        {
            { TelemetryConstants.Key, hookContext.FlagKey },
            { TelemetryConstants.Provider, hookContext.ProviderMetadata.Name }
        };

        attributes[TelemetryConstants.Reason] = !string.IsNullOrWhiteSpace(details.Reason)
            ? details.Reason?.ToLowerInvariant()
            : Reason.Unknown.ToLowerInvariant();
        attributes[TelemetryConstants.Variant] = details.Variant;
        attributes[TelemetryConstants.Value] = details.Value;

        if (details.FlagMetadata != null)
        {
            attributes[TelemetryConstants.ContextId] = details.FlagMetadata.GetString(TelemetryFlagMetadata.ContextId);
            attributes[TelemetryConstants.FlagSetId] = details.FlagMetadata.GetString(TelemetryFlagMetadata.FlagSetId);
            attributes[TelemetryConstants.Version] = details.FlagMetadata.GetString(TelemetryFlagMetadata.Version);
        }

        if (details.ErrorType != ErrorType.None)
        {
            attributes[TelemetryConstants.ErrorCode] = details.ErrorType.ToString().ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(details.ErrorMessage))
            {
                attributes[TelemetryConstants.ErrorMessage] = details.ErrorMessage;
            }
        }

        return new EvaluationEvent(EventName, attributes);
    }
}
