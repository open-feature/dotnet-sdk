using System.Diagnostics;
using System.Reflection;
using OpenFeature.Constant;

namespace OpenFeature;

static class OpenFeatureActivitySource
{
    static readonly ActivitySource Source = new("OpenFeature", GetLibraryVersion());

    internal static Activity? StartActivity(string name)
        => Source.StartActivity(name, ActivityKind.Client);

    // Mapped to standard `error.types` https://opentelemetry.io/docs/specs/semconv/feature-flags/feature-flags-events/#evaluation-event
    internal static string GetFlagEvaluationErrorDescription(this ErrorType errorType) =>
        errorType switch
        {
            ErrorType.ProviderNotReady => "provider_not_ready",
            ErrorType.FlagNotFound => "flag_not_found",
            ErrorType.ParseError => "parse_error",
            ErrorType.TypeMismatch => "type_mismatch",
            ErrorType.General => "general",
            ErrorType.InvalidContext => "invalid_context",
            ErrorType.TargetingKeyMissing => "targeting_key_missing",
            ErrorType.ProviderFatal => "provider_fatal",
            _ => "_OTHER"
        };

    // Mapped to standard `feature_flag.result.reason` https://opentelemetry.io/docs/specs/semconv/feature-flags/feature-flags-events/#evaluation-event
    internal static string? GetFlagEvaluationReasonDescription(string? reason) =>
        reason switch
        {
            Reason.TargetingMatch => "targeting_match",
            Reason.Split => "split",
            Reason.Disabled => "disabled",
            Reason.Default => "default",
            Reason.Static => "static",
            Reason.Cached => "cached",
            Reason.Unknown => "unknown",
            Reason.Error => "error",
            _ => reason
        };

    static string GetLibraryVersion()
        => typeof(OpenFeatureActivitySource).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "UNKNOWN";
}
