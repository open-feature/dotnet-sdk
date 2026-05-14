using System.Diagnostics;
using OpenFeature.Constant;

namespace OpenFeature;

static class OpenFeatureActivitySource
{
    static readonly ActivitySource Source = new("OpenFeature", GetLibraryVersion());

    internal static Activity? StartActivity(string name)
        => Source.StartActivity(name, ActivityKind.Internal);

    internal const string EvaluationActivityName = "feature_flag.evaluation";
    internal const string FeatureFlagKeyName = "feature_flag.key";
    internal const string FeatureFlagProviderName = "feature_flag.provider.name";
    internal const string FeatureFlagReasonName = "feature_flag.result.reason";
    internal const string FeatureFlagValueName = "feature_flag.result.value";
    internal const string FeatureFlagVariantName = "feature_flag.result.variant";
    internal const string FeatureFlagErrorMessageName = "feature_flag.error.message";

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
    {
        var version = typeof(OpenFeatureActivitySource).Assembly
            .GetName()
            .Version;

        // "3" = major.minor.patch only
        return version?.ToString(3) ?? "UNKNOWN";
    }
}
