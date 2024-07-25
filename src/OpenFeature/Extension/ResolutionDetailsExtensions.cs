using OpenFeature.Model;

namespace OpenFeature.Extension
{
    internal static class ResolutionDetailsExtensions
    {
        public static FlagEvaluationDetails<T> ToFlagEvaluationDetails<T>(this ResolutionDetails<T> details)
        {
            return new FlagEvaluationDetails<T>(details.FlagKey, details.Value, details.ErrorType, details.Reason,
                details.Variant, details.ErrorMessage, details.FlagMetadata);
        }
    }
}
