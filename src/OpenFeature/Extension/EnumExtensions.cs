using OpenFeature.Constant;

namespace OpenFeature.Extension;

internal static class EnumExtensions
{
    /// <summary>
    /// Gets the description of an enum value without using reflection.
    /// This is AOT-compatible and only supports specific known enum types.
    /// </summary>
    /// <param name="value">The enum value to get the description for</param>
    /// <returns>The description string or the enum value as string if no description is available</returns>
    public static string GetDescription(this Enum value)
    {
        return value switch
        {
            // ErrorType descriptions
            ErrorType.None => "NONE",
            ErrorType.ProviderNotReady => "PROVIDER_NOT_READY",
            ErrorType.FlagNotFound => "FLAG_NOT_FOUND",
            ErrorType.ParseError => "PARSE_ERROR",
            ErrorType.TypeMismatch => "TYPE_MISMATCH",
            ErrorType.General => "GENERAL",
            ErrorType.InvalidContext => "INVALID_CONTEXT",
            ErrorType.TargetingKeyMissing => "TARGETING_KEY_MISSING",
            ErrorType.ProviderFatal => "PROVIDER_FATAL",

            // Fallback for any other enum types
            _ => value.ToString()
        };
    }
}
