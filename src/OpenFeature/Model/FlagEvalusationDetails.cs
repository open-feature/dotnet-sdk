using OpenFeature.Constant;

namespace OpenFeature.Model
{
    public class FlagEvaluationDetails<T>
    {
        public T Value { get; }
        public string FlagKey { get; }
        public ErrorType ErrorType { get; }
        public string Reason { get; }
        public string Variant { get; }

        public FlagEvaluationDetails(string flagKey, T value, ErrorType errorType, string reason, string variant)
        {
            this.Value = value;
            this.FlagKey = flagKey;
            this.ErrorType = errorType;
            this.Reason = reason;
            this.Variant = variant;
        }
    }
}
