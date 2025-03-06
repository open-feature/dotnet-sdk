using System.Collections.Generic;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace OpenFeature.Telemetry;

public class EvaluationEvent
{
    public string Name { get; set; }
    public Dictionary<string, object?> Attributes { get; set; }
    public Dictionary<string, object> Body { get; set; }
}

public static class EvaluationEventFactory
{

    private const string EventName = "feature_flag.evaluation";

    public static EvaluationEvent CreateEvaluationEvent(HookContext<Value> hookContext, FlagEvaluationDetails<Value> details)
    {
        var attributes = new Dictionary<string, object?>
        {
            { TelemetryConstants.Key, hookContext.FlagKey },
            { TelemetryConstants.Provider, hookContext.ProviderMetadata.Name }
        };

        attributes[TelemetryConstants.Reason] = !string.IsNullOrWhiteSpace(details.Reason) ? details.Reason?.ToLowerInvariant() : "unknown";

        var body = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(details.Variant))
        {
            attributes[TelemetryConstants.Variant] = details.Variant ?? details.Value.ToString();
        }

        attributes[TelemetryFlagMetadata.ContextId] = details.FlagMetadata?.GetString(TelemetryFlagMetadata.ContextId);
        attributes[TelemetryFlagMetadata.FlagSetId] = details.FlagMetadata?.GetString(TelemetryFlagMetadata.FlagSetId);
        attributes[TelemetryFlagMetadata.Version] = details.FlagMetadata?.GetString(TelemetryFlagMetadata.Version);

        if (details.ErrorType != ErrorType.None)
        {
            attributes[TelemetryConstants.ErrorCode] = details.ErrorType.ToString();

            if (!string.IsNullOrWhiteSpace(details.ErrorMessage))
            {
                attributes[TelemetryConstants.ErrorMessage] = details.ErrorMessage ?? "N/A";
            }
        }

        return new EvaluationEvent
        {
            Name = EventName,
            Attributes = attributes,
            Body = body
        };
    }
}
