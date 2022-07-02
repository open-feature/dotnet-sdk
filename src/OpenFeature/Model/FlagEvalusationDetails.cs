namespace OpenFeature.Model
{
    public class FlagEvaluationDetails<T>
    {
        public T Value { get; }
        public string FlagKey { get; }
        public string ErrorCode { get; }
        public string Reason { get; }
        public string Variant { get; }
        
        public FlagEvaluationDetails(string flagKey, T value, string errorCode, string reason, string variant)
        {
            Value = value;
            FlagKey = flagKey;
            ErrorCode = errorCode;
            Reason = reason;
            Variant = variant;    
        }
    }
}