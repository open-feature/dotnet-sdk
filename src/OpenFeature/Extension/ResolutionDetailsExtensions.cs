using OpenFeature.Model;

namespace OpenFeature.Extention
{
    public static class ResolutionDetailsExtensions
    {
        public static FlagEvaluationDetails<T> ToFlagEvaluationDetails<T>(this ResolutionDetails<T> details)
        {
            return new FlagEvaluationDetails<T>(details.FlagKey, details.Value, details.ErrorCode, details.Reason, details.Variant);
        }
    }
}